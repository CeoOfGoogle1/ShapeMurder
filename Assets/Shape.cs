using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    [Header("Shape Settings")]
    public List<Shape> neighbors;
    public int team;
    public int population;

    [Header("Send Settings")]
    public GameObject army;
    public Shape destination;
    public int sendSize;
    public float sendSpeed;
    public float sendTime;

    [Header("Grow Settings")]
    public int growSize;
    public float growSpeed;
    public float growTime;

    [Header("Invaders Settings")]
    public List<Army> invaders;

    void Grow()
    {
        growTime =+ growSpeed * Time.deltaTime;
        if (growTime >= 10)
        {
            population += growSize;
            growTime = 0;
        }
    }

    void Send()
    {
        sendTime =+ sendSpeed * Time.deltaTime;
        if (sendTime >= 10)
        {
            GameObject unit = Instantiate(army);
            Army stats = unit.GetComponent<Army>();
            stats.team = team;
            stats.destination = destination;
            stats.size = sendSize;
            sendTime = 0;
        }
    }

    void Update()
    {
        Grow();
        if (population > 0 && destination != null) Send();
    }
}
