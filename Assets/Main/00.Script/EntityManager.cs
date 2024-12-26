using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EntityManager : MonoBehaviour
{

    public static EntityManager instance;
    void Awake() => instance = this;

    [SerializeField] GameObject entitiyPrefab;
    [SerializeField] GameObject damagePrefab;
    [SerializeField] List<Entity> myEntities;
    [SerializeField] List<Entity> otherEntities;
    [SerializeField] GameObject TargerPicker;
    [SerializeField] Entity myEmptyEntitiy;
    [SerializeField] Entity myBossEntitiy;
    [SerializeField] Entity otherBossEntitiy;

    const int MAX_ENTITY_COUNT = 6;
    public bool IsFullMyEntities => myEntities.Count >= MAX_ENTITY_COUNT && !ExistMyEmptyEntity;
    bool IsFullOtherEntities => otherEntities.Count >= MAX_ENTITY_COUNT;
    bool ExistTargetPickEntity => targetPickEntity != null;
    bool ExistMyEmptyEntity => myEntities.Exists(x => x == myEmptyEntitiy);
    int MyEmptyEntitiyIndex => myEntities.FindIndex(x => x == myEmptyEntitiy);
    bool CanMouseInput => TurnManager.instance.myTurn && !TurnManager.instance.isLoading;


    Entity selectEnetity;
    Entity targetPickEntity;
    WaitForSeconds delay1 = new WaitForSeconds(1);
    WaitForSeconds delay2 = new WaitForSeconds(2);

    private void Start()
    {
        TurnManager.OnTurnStarted += OnTurnStarted;
    }
    private void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;

    }
    private void Update()
    {
        ShowTargetPicker(ExistTargetPickEntity);
    }


    void OnTurnStarted(bool myTurn)
    {
        AttackableReset(myTurn);
        if (!myTurn)
            StartCoroutine(AICo());

    }
    IEnumerator AICo()
    {
        CardManager.instance.TryPutCard(false);
        yield return delay1;
        var attackers = new List<Entity>(otherEntities.FindAll(x => x.attackable == true));
        for(int i = 0; i < attackers.Count; i++)
        {
            int rand = Random.Range(i, attackers.Count);
            Entity temp = attackers[i];
            attackers[i] = attackers[rand];
            attackers[rand] = temp;
        }

        foreach(var attacker in attackers)
        {
            var defenders = new List<Entity>(myEntities);
            defenders.Add(myBossEntitiy);
            int rand = Random.Range(0,defenders.Count);
            Attack(attacker, defenders[rand]);

            if (TurnManager.instance.isLoading)
                yield break;

            yield return delay2;
        }

        TurnManager.instance.EndTurn(); // 공격로직
    }
  void EntityAlignment(bool isMine)
{
    float targetY = isMine ? -4.35f : 4.15f;
    var targetEntities = isMine ? myEntities : otherEntities;

    // 첫 번째 엔티티의 X 위치를 0으로 설정 (기준점)
    float startingX = (targetEntities.Count - 1) * -3.4f;

    // 엔티티들의 X 위치를 적절히 설정
    for (int i = 0; i < targetEntities.Count; i++)
    {
        float targetX = startingX + i * 6.8f;

        var targetEntity = targetEntities[i];
        targetEntity.originPos = new Vector3(targetX, targetY, 0);
        targetEntity.MoveTransform(targetEntity.originPos, true, 0.5f);
        targetEntity.GetComponent<Order>()?.SetOriginOrder(i);
    }
}

    public void InsertMyEmptyEntity(float xPos)
    {
        if (IsFullMyEntities)
            return;

        if (!ExistMyEmptyEntity)
            myEntities.Add(myEmptyEntitiy);

        Vector3 emptyEntitiyPos = myEmptyEntitiy.transform.position;
        emptyEntitiyPos.x = xPos;
        myEmptyEntitiy.transform.position = emptyEntitiyPos;

        int _emptyEntitiyIndex = MyEmptyEntitiyIndex;
        myEntities.Sort((entitiy1, entitiy2) => entitiy1.transform.position.x.CompareTo(entitiy2.transform.position.x));
        if (MyEmptyEntitiyIndex != _emptyEntitiyIndex)
            EntityAlignment(true);
    }
    public void RemoveMyEmptyEntity()
    {
        if (!ExistMyEmptyEntity)
            return;

        myEntities.RemoveAt(MyEmptyEntitiyIndex);
        EntityAlignment(true);
    }

    public bool SpawnEntity(bool isMine, Item item, Vector3 spawnPos)
    {
        if (isMine)
        {
            if (IsFullMyEntities || !ExistMyEmptyEntity)
                return false;
        }
        else
        {
            if (IsFullOtherEntities)
                return false;
        }
        var entityObject = Instantiate(entitiyPrefab, spawnPos, Utils.QI);
        var entitiy = entityObject.GetComponent<Entity>();

        if (isMine)
        {
            myEntities[MyEmptyEntitiyIndex] = entitiy;
        }
        else
        {
            otherEntities.Insert(Random.Range(0, otherEntities.Count), entitiy);
        }

        entitiy.isMine = isMine;
        entitiy.SetUp(item);
        EntityAlignment(isMine); 
        AudioManager.instance.PlaySound(transform.position, 1, Random.Range(1f, 1.2f), 1f);

        return true;
    }

    public void EntityMouseDown(Entity entity)
    {
        if (!CanMouseInput)
            return;

        selectEnetity = entity;
    }
    public void EntityMouseUp()
    {
        if (!CanMouseInput)
            return;

        if (selectEnetity && targetPickEntity && selectEnetity.attackable)
            Attack(selectEnetity, targetPickEntity);

        selectEnetity = null;
        targetPickEntity = null;
    }

    public void EntityMouseDrag()
    {
        if (!CanMouseInput || selectEnetity == null)
            return;

        bool existTarget = false;
        foreach (var hit in Physics2D.RaycastAll(Utils.MousePos, Vector3.forward))
        {
            Entity entity = hit.collider?.GetComponent<Entity>();

            if (entity != null && !entity.isMine && selectEnetity.attackable)
            {
                targetPickEntity = entity;
                existTarget = true;
                break;
            }
        }
        if (!existTarget)
            targetPickEntity = null;
    }
    void Attack(Entity attacker, Entity defender)
    {
        attacker.attackable = false;
        attacker.GetComponent<Order>().SetMostFrontOrder(true);

        AudioManager.instance.PlaySound(transform.position, 2, Random.Range(1f, 1.2f), 1f);

        DG.Tweening.Sequence sequence = DOTween.Sequence()
            .Append(attacker.transform.DOMove(defender.originPos, 0.4f)).SetEase(Ease.InSine)
            .AppendCallback(() =>
            {
                attacker.Damaged(defender.attack);
                defender.Damaged(attacker.attack);
                SpawnDamage(defender.attack, attacker.transform);
                SpawnDamage(attacker.attack, defender.transform);
            })
            .Append(attacker.transform.DOMove(attacker.originPos, 0.4f)).SetEase(Ease.OutSine)
            .OnComplete(() => AttackCallback(attacker,defender));
    }

    void AttackCallback(params Entity[] entities)
    {
        foreach (var entity in entities)
        {
            if (!entity.isDie || entity.isBossOrEmpty)
                continue;
            if (entity.isMine)
                myEntities.Remove(entity);
            else
                otherEntities.Remove(entity);

            DG.Tweening.Sequence sequence = DOTween.Sequence()
                .Append(entity.transform.DOShakePosition(1.3f))
                .Append(entity.transform.DOScale(Vector3.zero, 0.3f)).SetEase(Ease.OutCirc)
                .OnComplete(() =>
                {
                    EntityAlignment(entity.isMine);
                    Destroy(entity.gameObject);
                });
        }
        StartCoroutine(CheckBossDie());
    }
    IEnumerator CheckBossDie()
    {
        yield return delay2;

        if(myBossEntitiy.isDie)
            StartCoroutine(GameManager.instance.GameOver(false));

        if(otherBossEntitiy.isDie)
            StartCoroutine(GameManager.instance.GameOver(true));
    }

    public void DamageBoss(bool isMine,int damage)
    {
        var targetBossEntity = isMine ? myBossEntitiy : otherBossEntitiy;
        targetBossEntity.Damaged(damage);
        StartCoroutine(CheckBossDie());
    }

    private void ShowTargetPicker(bool isShow)
    {
        TargerPicker.SetActive(isShow);
        if (ExistTargetPickEntity)
            TargerPicker.transform.position = targetPickEntity.transform.position;
    }
    void SpawnDamage(int damage , Transform tr)
    {
        if (damage <= 0)
            return;
       
        var damageComponent = Instantiate(damagePrefab).GetComponent<Damage>();
        damageComponent.SetupTransform(tr);
        damageComponent.Damaged(damage);
    }
    public void AttackableReset(bool isMine)
    {
        var targetEntites = isMine ? myEntities : otherEntities;
        targetEntites.ForEach(x => x.attackable = true);
    }


}
