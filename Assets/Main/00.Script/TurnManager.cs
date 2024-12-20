using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.Playables;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }
    [Header("Develop")]
    [SerializeField]
    [Tooltip("���� �� ��带 ���մϴ�.")] ETurnMode eTurnMode;
    [SerializeField]
    [Tooltip("ī�� ����� �ſ� �������ϴ�.")] bool fastMode;
    [SerializeField]
    [Tooltip("���� ī�尳���� ���մϴ�.")] int startCardCount;

    [Header("������Ƽ")]
    public bool myTurn;
    public bool isLoading;

    enum ETurnMode { Random, My, Other }
    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
    WaitForSeconds delay07 = new WaitForSeconds(0.7f);

    public static Action<bool> OnAddCard;
    public static event Action<bool> OnTurnStarted;

    void GameSetUp()
    {
        if (fastMode)
            delay05 = new WaitForSeconds(0.05f);
        switch (eTurnMode)
        {
            case ETurnMode.Random:
                myTurn = Random.Range(0, 2) == 0;
                break;
            case ETurnMode.My:
                myTurn = true;
                break;
            case ETurnMode.Other:
                myTurn = false;
                break;
            default:
                break;
        }
    }

    public IEnumerator StartGameCo()
    {
        GameSetUp();
        isLoading = true;

        for (int i = 0; i < startCardCount; i++)
        {
            yield return delay05;
            OnAddCard?.Invoke(false);
            yield return delay05;
            OnAddCard?.Invoke(true);
        }
        StartCoroutine(StartTurnCo());
    }

    public IEnumerator StartTurnCo()
    {

        isLoading = true;
        if (myTurn)
            GameManager.instance.Notification("������");
        yield return delay07;
        OnAddCard?.Invoke(myTurn);
        yield return delay07;
        isLoading = false;
        OnTurnStarted?.Invoke(myTurn);
    }

    public void EndTurn()
    {
        myTurn = !myTurn;
        StartCoroutine(StartTurnCo());
    }
}
