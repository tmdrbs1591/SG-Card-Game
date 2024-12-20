using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour
{
    [SerializeField] Sprite active;
    [SerializeField] Sprite inactive;
    [SerializeField] TMP_Text btnText;

    private void Start()
    {
        SetUp(false);
        TurnManager.OnTurnStarted += SetUp;
    }
    private void OnDestroy()
    {
        TurnManager.OnTurnStarted -= SetUp;
    }

    public void SetUp(bool isActive)
    {
        GetComponent<Image>().sprite = isActive ? active : inactive;
        GetComponent<Button>().interactable = isActive;
        btnText.color = isActive ? new Color32(255, 195, 90, 255) : new Color32(55, 55, 55, 255);
    }
}   
