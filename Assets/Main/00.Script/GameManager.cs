using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance {  get; private set; }


    private void Awake()
    {
        instance = this;
    }

    [Multiline(10)]
    [SerializeField] string cheatInfo;

    [SerializeField] NotificationPanel notificationPanel;
    [SerializeField] ResultPanel resultPanel;
    [SerializeField] GameObject endTurnBtn;
     
    WaitForSeconds delay2 = new WaitForSeconds(2);

    private void Start()
    {
        StartGame();
    }
    private void Update()
    {
#if UNITY_EDITOR
        InputCardKey();
#endif
    }

    void InputCardKey()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {
            TurnManager.OnAddCard?.Invoke(true);
        }
        if (Input.GetKeyUp(KeyCode.G))
        {
            TurnManager.OnAddCard?.Invoke(false);

        }
        if (Input.GetKeyUp(KeyCode.H))
        {
            TurnManager.instance.EndTurn();

        }
        if (Input.GetKeyUp(KeyCode.J))
        {
            CardManager.instance.TryPutCard(false);
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            EntityManager.instance.DamageBoss(true, 19);
        }
        if (Input.GetKeyUp(KeyCode.B))
        {
            EntityManager.instance.DamageBoss(false, 19);
        }
    }

    void StartGame()
    {
        StartCoroutine(TurnManager.instance.StartGameCo());
    }

    public void Notification(string message)
    {
        notificationPanel.Show(message);
    }

    public IEnumerator GameOver(bool isMyWin)
    {
        TurnManager.instance.isLoading = true;
        endTurnBtn.SetActive(false);
        yield return delay2;

        TurnManager.instance.isLoading = true;
        resultPanel.Show(isMyWin ? "½Â¸®" : "ÆÐ¹è");
    }
}
