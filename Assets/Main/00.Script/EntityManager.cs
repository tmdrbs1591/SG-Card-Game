using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EntityManager : MonoBehaviour
{

    public static EntityManager instance;
    void Awake() => instance = this;

    [SerializeField] GameObject entitiyPrefab;
    [SerializeField] List<Entity> myEntities;
    [SerializeField] List<Entity> otherEntities;
    [SerializeField] Entity myEmptyEntitiy;
    [SerializeField] Entity myBossEntitiy;
    [SerializeField] Entity otherBossEntitiy;

    const int MAX_ENTITY_COUNT = 6;
    public bool IsFullMyEntities => myEntities.Count >= MAX_ENTITY_COUNT && !ExistMyEmptyEntity;
    bool IsFullOtherEntities => otherEntities.Count >= MAX_ENTITY_COUNT;
    bool ExistMyEmptyEntity => myEntities.Exists(x => x == myEmptyEntitiy);
    int MyEmptyEntitiyIndex => myEntities.FindIndex(x => x == myEmptyEntitiy);

    WaitForSeconds delay1 = new WaitForSeconds(1);

    private void Start()
    {
        TurnManager.OnTurnStarted += OnTurnStarted;
    }
    private void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;

    }
    void OnTurnStarted(bool myTurn)
    {
        if (!myTurn)
            StartCoroutine(AICo());

    }
    IEnumerator AICo()
    {
        CardManager.instance.TryPutCard(false);
        yield return delay1;

        TurnManager.instance.EndTurn(); // 공격로직
    }
    void EntityAlignment(bool isMine)
    {
        float targetY = isMine ? -4.35f : 4.15f;
        var targetEntities = isMine ? myEntities : otherEntities;

        for (int i = 0; i < myEntities.Count; i++)
        {
            float targetX = (targetEntities.Count - 1) * -3.4f + i * 6.8f;

            var targetEntitiy = targetEntities[i];
            targetEntitiy.originPos = new Vector3(targetX, targetY, 0);
            targetEntitiy.MoveTransform(targetEntitiy.originPos, true, 0.5f);
            targetEntitiy.GetComponent<Order>()?.SetOriginOrder(i);


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
        EntityAlignment(isMine); return true;
    }

 
}
