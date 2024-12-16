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

    [SerializeField] NotificationPanel notificationPanel;
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
    }

    void StartGame()
    {
        StartCoroutine(TurnManager.instance.StartGameCo());
    }

    public void Notification(string message)
    {
        notificationPanel.Show(message);
    }
}
