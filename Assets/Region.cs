using System.Collections.Generic;
using UnityEngine;

public class Region : MonoBehaviour
{
    [Header("Region Settings")]
    public List<Region> neighbors;
    public int team;
    public Army garrison;
    public int cap;

    [Header("Send Settings")]
    public GameObject army;
    public Region destination;
    public int sendSize;
    public float sendSpeed;
    public float sendTime;

    [Header("Grow Settings")]
    public int growSize;
    public float growSpeed;
    public float growTime;

    [Header("Invader Settings")]
    public Army invader;
    public float fightSpeed;
    public float fightTime;

    void Grow()
    {
        if (garrison.size >= cap) return;

        growTime += growSpeed * Time.deltaTime;
        if (growTime >= 10)
        {
            garrison.size += growSize;
            growTime = 0;
        }
    }

    void Send()
    {
        sendTime += sendSpeed * Time.deltaTime;
        if (sendTime >= 10)
        {
            GameObject unit = Instantiate(army, transform.position, Quaternion.identity);
            Army stats = unit.GetComponent<Army>();
            garrison.size -= sendSize;
            stats.team = team;
            stats.origin = this;
            stats.destination = destination;
            stats.size = sendSize;
            sendTime = 0;
        }
    }

    void Update()
    {
        Grow();
        if (garrison.size >= sendSize && destination != null) Send();
        if (invader != null) Fight();
    }

    public void ReceiveArmy(Army army)
    {
        if (army.team == team)
        {
            garrison.size += army.size; 
        }
        else
        {
            invader.team = army.team;
            invader.size = army.size;
        }
    }

    void Fight()
    {
        fightTime += fightSpeed * Time.deltaTime;
        if (fightTime >= 10)
        {
            garrison.size -= 1;
            invader.size -= 1;
            fightTime = 0;
        }

        if (garrison.size <= 0)
        {
            team = invader.team;
            garrison.size = invader.size;
        }
    }
}
