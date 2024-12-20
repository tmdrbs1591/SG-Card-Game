using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using System.Xml.Serialization;
using System;
using Random = UnityEngine.Random;

public class CardManager : MonoBehaviour
{
    public static CardManager instance { get; private set; }
    private void Awake()
    {
        instance = this;
    }

    [SerializeField] ItemSO itemSO;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] List<Card> myCards;
    [SerializeField] List<Card> otherCards;
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform otherCardSpawnPoint;
    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;
    [SerializeField] Transform otherCardLeft;
    [SerializeField] Transform otherCardRight;
    [SerializeField] ECardState eCardState;


    List<Item> itemBuffer;

    Card selectCard;
    bool isMyCardDrag;
    bool onMyCardArea;
    enum ECardState { Nothing, CanMouseOver, CanMouseDrag}
    int myPutCount;

    public Item PopItem()
    {
        if (itemBuffer.Count == 0)
        {
            SetUpItemBuffer();
        }

        Item item = itemBuffer[0];
        itemBuffer.RemoveAt(0);
        return item;
    }
    void SetUpItemBuffer()
    {
        itemBuffer = new List<Item>();
        for (int i = 0; i < itemSO.items.Length; i++)
        {
            Item item = itemSO.items[i];
            for (int j = 0; j < item.percent; j++)
            {
                itemBuffer.Add(item);
            }
        }

        for (int i = 0; i < itemBuffer.Count; i++)
        {
            int rand = Random.Range(i, itemBuffer.Count);
            Item temp = itemBuffer[rand];
            itemBuffer[i] = itemBuffer[rand];
            itemBuffer[rand] = temp;
        }
    }
    private void Start()
    {
        SetUpItemBuffer();
        TurnManager.OnAddCard += AddCard;
        TurnManager.OnTurnStarted += OnTurnStarted;
    }
    private void OnDestroy()
    {
        TurnManager.OnAddCard -= AddCard;
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }
    void OnTurnStarted(bool myTurn)
    {
        if (myTurn)
            myPutCount = 0;
    }
    private void Update()
    {
        if (isMyCardDrag)
        {
            CardDrag();
        }

        DetectCardArea();
        SetECardStage();
    }



    void AddCard(bool isMine)
    {
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        var card = cardObject.GetComponent<Card>();
        card.SetUp(PopItem(), isMine);
        (isMine ? myCards : otherCards).Add(card);

        SetOriginOrder(isMine);
        CardAlignment(isMine);
    }

    void SetOriginOrder(bool isMine)
    {
        int count = isMine ? myCards.Count : otherCards.Count;
        for (int i = 0; i < count; i++)
        {
            var targetCard = isMine ? myCards[i] : otherCards[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }

    void CardAlignment(bool isMine)
    {
        List<PRS> originCardRPSs = new List<PRS>();
        if (isMine)
        {
            originCardRPSs = RoundAlugnment(myCardLeft, myCardRight, myCards.Count, 0.5f, Vector3.one * 12f);
        }
        else
        {
            originCardRPSs = RoundAlugnment(otherCardLeft, otherCardRight, otherCards.Count, -0.5f, Vector3.one * 12f);

        }
        var targerCards = isMine ? myCards : otherCards;
        for (int i = 0; i < targerCards.Count; i++)
        {
            var targetCard = targerCards[i];

            targetCard.originsPRS = originCardRPSs[i];
            targetCard.MoveTransform(targetCard.originsPRS, true, 0.7f);
        }
    }

    List<PRS> RoundAlugnment(Transform leftTr, Transform rightTr, int objCount, float height, Vector3 scale)
    {
        float[] objLerps = new float[objCount];
        List<PRS> results = new List<PRS>(objCount);

        switch (objCount)
        {
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2: objLerps = new float[] { 0.27f, 0.73f }; break;
            case 3: objLerps = new float[] { 0.1f, 0.5f, 0.9f }; break;
            default:
                float interval = 1f / (objCount - 1);
                for (int i = 0; i < objCount; i++)
                    objLerps[i] = interval * i;
                break;
        }

        for (int i = 0; i < objCount; i++)
        {
            var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
            var targetRot = Quaternion.identity;
            if (objCount >= 4)
            {
                float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
                curve = height >= 0 ? curve : -curve;
                targetPos.y += curve;
                targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
            }
            results.Add(new PRS(targetPos, targetRot, scale));
        }
        return results;
    }

    public bool TryPutCard(bool isMine)
    {
        if (isMine && myPutCount >= 1)
            return false;

        if (isMine && otherCards.Count <= 0)
            return false;

        Card card = isMine ? selectCard : otherCards[Random.Range(0, otherCards.Count)];
        var spawnPos = isMine ? Utils.MousePos : otherCardSpawnPoint.position;
        var targetCards = isMine ? myCards : otherCards;

        if (EntityManager.instance.SpawnEntity(isMine, card.item, spawnPos))
        {
            targetCards.Remove(card);
            card.transform.DOKill();
            DestroyImmediate(card.gameObject);
            if (isMine) {
            selectCard = null;
                myPutCount++;
            
            }
            CardAlignment(isMine);
            return true;
        }
        else
        {
            targetCards.ForEach(x => x.GetComponent<Order>().SetMostFrontOrder(false));
            CardAlignment(isMine);
            return false; 

        }
    }


    #region MyCard
    public void CardMouseOver(Card card)
    {
        if (eCardState == ECardState.Nothing)
            return;
        selectCard = card;
        EnlargeCard(true, card);
    }

    public void CardMouseExit(Card card)
    {

        EnlargeCard(false, card);

    }
    public void CardMouseDown()
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;
        isMyCardDrag = true;
    }
    public void CardMouseUp() 
    {
        isMyCardDrag = false;
            if (eCardState != ECardState.CanMouseDrag)
                return;

        if (onMyCardArea)
            EntityManager.instance.RemoveMyEmptyEntity();
        else
            TryPutCard(true);
    }
    private void CardDrag()
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;
        if (!onMyCardArea)
        {
            selectCard.MoveTransform(new PRS(Utils.MousePos, Utils.QI, selectCard.originsPRS.scale), false);
            EntityManager.instance.InsertMyEmptyEntity(Utils.MousePos.x);
        }
    }
    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos,Vector3.forward);
        int layer = LayerMask.NameToLayer("MyCardArea");
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }
    void EnlargeCard(bool isEnlarge, Card card)
    {
        if (isEnlarge)
        {
            Vector3 enlargePos = new Vector3(card.originsPRS.pos.x, -5f, -10f);
            card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 15.5f), false);

        }
        else
        {
            card.MoveTransform(card.originsPRS, false);
        }
        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
    }

    private void SetECardStage()
    {
        if (TurnManager.instance.isLoading)
        {
            eCardState = ECardState.Nothing;
        }
        else if (!TurnManager.instance.myTurn || myPutCount ==1 || EntityManager.instance.IsFullMyEntities)
        {
            eCardState = ECardState.CanMouseOver;
        }
        else if (TurnManager.instance.myTurn && myPutCount ==0)
        {
            eCardState = ECardState.CanMouseDrag;
        }
    }
    #endregion
}
