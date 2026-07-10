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
    public List<Army> invaders;
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

    public void ReceiveArmy(Army army)
    {
        if (army.team == team)
        {
            if (garrison.size >= cap)
            {
                RetreatParty(army);
            }
            garrison.size += army.size;
            Destroy(army.gameObject);
        }
        else
        {
            invaders.Add(army);
            army.gameObject.SetActive(false);
        }
    }

    void Fight()
    {
        fightTime += fightSpeed * Time.deltaTime;
        if (fightTime >= 10)
        {
            garrison.size -= 1;
            foreach (var invader in invaders)
            {
                invader.size--;
            }
            fightTime = 0;
        }

        foreach (var invader in invaders)
        {
            if (invader.size <= 0)
            {
                Destroy(invader.gameObject);
            }
        }

        if (garrison.size <= 0 && invaders.Count == 1)
        {
            team = invaders[0].team;
            garrison.size = invaders[0].size;
            Destroy(invaders[0].gameObject);
        }
    }

    void Update()
    {
        Grow();
        if (garrison.size >= sendSize && destination != null) Send();
        if (invaders.Count > 0) Fight();
    }

    public void RetreatParty(Army army)
    {
        invaders.Remove(army);
        army.retreating = true;
        army.gameObject.SetActive(true);
        army.size--;
    }
}
