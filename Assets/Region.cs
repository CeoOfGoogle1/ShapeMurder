using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class Region : MonoBehaviour
{
    [Header("Region Settings")]
    public List<Region> neighbors;
    public Player player;
    public Army garrison;
    public List<Army> visitors;
    public int limit;

    [Header("Send Settings")]
    public GameObject army;
    public Region destination;
    public int sendSize;
    public float sendTime;
    public float sendTimer = 0;

    [Header("Grow Settings")]
    public int growSize;
    public float growTime;
    public float growTimer = 0;

    [Header("Invader Settings")]
    public List<Army> invaders;
    public float fightSpeed;
    public float fightTime;

    void Grow()
    {
        if (garrison.size >= limit) return;

        growTimer += Time.deltaTime;
        if (growTimer >= growTime)
        {
            garrison.size += growSize;
            growTimer = 0;
        }
    }

    void Send()
    {
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendTime)
        {
            GameObject unit = Instantiate(army, transform.position, Quaternion.identity);
            Army stats = unit.GetComponent<Army>();
            garrison.size -= sendSize;
            stats.player = player;
            stats.origin = this;
            stats.destination = destination;
            stats.size = sendSize;
            sendTimer = 0;
        }
    }

    public void ReceiveArmy(Army army)
    {
        if (army.player.Equals(player))
        {
            if (garrison.size >= limit)
            {
                RetreatArmy(army);
            }
            garrison.size += army.size;
            Destroy(army.gameObject);
        }
        else if (player.Allies.Contains(army.player.Id))
        {
            visitors.Add(army);
            army.gameObject.SetActive(false);
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

        foreach (var visitor in visitors)
        {
            if (visitor.size <= 0)
            {
                Destroy(visitor.gameObject);
            }
        }

        if (garrison.size <= 0 && invaders.Count == 1 && visitors.Count < 1)
        {
            player = invaders[0].player;
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

    public void RetreatArmy(Army army)
    {
        invaders.Remove(army);
        army.retreating = true;
        army.gameObject.SetActive(true);
        army.size--;
    }
}
