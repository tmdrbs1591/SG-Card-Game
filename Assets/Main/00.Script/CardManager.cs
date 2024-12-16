using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {
            AddCard(true);
        }
        if (Input.GetKeyUp(KeyCode.G))
        {
            AddCard(false);
        }
    }

    void AddCard(bool isMine)
    {
        var cardObject = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
        var card = cardObject.GetComponent<Card>();
        card.SetUp(PopItem(),isMine);
        (isMine ? myCards : otherCards).Add(card);
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
}
