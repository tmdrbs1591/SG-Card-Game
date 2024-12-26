using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Entity : MonoBehaviour
{
    [SerializeField] Item item;
    [SerializeField] SpriteRenderer entity;
    [SerializeField] Image character;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text attackTMP;
    [SerializeField] TMP_Text healthTMP;
    [SerializeField] GameObject sleepParticle;

    public int attack;
    public int health;
    public bool isMine;
    public bool isDie;
    public bool isBossOrEmpty;
    public bool attackable;
    public Vector3 originPos;
    int liveCount;


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
        if (isBossOrEmpty)
            return;

        if (isMine == myTurn)
            liveCount++;

        sleepParticle.SetActive(liveCount < 1);
    }
    public void SetUp(Item item)
    {
        attack = item.attack;
        health = item.health;   

        this.item = item;
        character.sprite = this.item.sprite;
        nameTMP.text = this.item.name;  
        attackTMP.text = attack.ToString();
        healthTMP.text = health.ToString();
    }



    private void OnMouseDown()
    {
        if (isMine)
            EntityManager.instance.EntityMouseDown(this);
    }

    private void OnMouseUp()
    {
        
        if(isMine)
            EntityManager.instance.EntityMouseUp();

    }

    private void OnMouseDrag()
    {
        if(isMine)
            EntityManager.instance.EntityMouseDrag();

    }

    public bool Damaged(int damage)
    {
        health -= damage;
        healthTMP.text = health.ToString();


        CameraShake.instance.Shake();


        if (health <= 0)
        {
            isDie = true;
            return true;
        }
        return false;
    }



    public void MoveTransform(Vector3 pos, bool useDotween, float dotweenTime = 0)
    {
        if (useDotween)
        {
            transform.DOMove(pos, dotweenTime);
        }
        else
        {
            transform.position = pos;
        }
    }
}
