using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

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
    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;
    [SerializeField] Transform otherCardLeft;
    [SerializeField] Transform otherCardRight;
    

    List<Item> itemBuffer;



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

        for (int i = 0;i < itemBuffer.Count;i++)
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
    }
    private void OnDestroy()
    {
        TurnManager.OnAddCard -= AddCard;

    }
    private void Update()
    {
        
    }

    void AddCard(bool isMine)
    {
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        var card = cardObject.GetComponent<Card>();
        card.SetUp(PopItem(),isMine);
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
            originCardRPSs = RoundAlugnment(myCardLeft, myCardRight,myCards.Count,0.5f,Vector3.one * 12.5f);
        }
        else
        {
            originCardRPSs = RoundAlugnment(otherCardLeft, otherCardRight, otherCards.Count, -0.5f, Vector3.one * 12.5f);

        }
        var targerCards = isMine ? myCards : otherCards;
        for (int i = 0; i < targerCards.Count; i++)
        {
            var targetCard = targerCards[i];

            targetCard.originsPRS = originCardRPSs[i];
            targetCard.MoveTransform(targetCard.originsPRS, true, 0.7f);
        }
    }

    List<PRS> RoundAlugnment(Transform leftTr, Transform rightTr, int objCount, float height , Vector3 scale)
    {
        float[] objLerps = new float[objCount];
        List<PRS> results = new List<PRS>(objCount);

        switch (objCount)
        {
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2: objLerps = new float[] { 0.27f , 0.73f }; break;
            case 3: objLerps = new float[] { 0.1f, 0.5f ,0.9f }; break;
            default:
                float interval =1f / (objCount - 1);
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
}
