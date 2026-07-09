using UnityEngine;

public class Fight : MonoBehaviour
{
    [Header("Part A Settings")]
    public int teamA;
    public Shape destinationA;
    public int sizeA;
    public GameObject armyA;
    [Header("Party B Settings")]
    public int teamB;
    public Shape destinationB;
    public int sizeB;
    public GameObject armyB;

    [Header("Fight Settings")]
    public float fightSpeed;
    public float fightTime;

    void Update()
    {
        if (sizeA <= 0)
        {
            GameObject winner = Instantiate(armyB);
            Army stats = winner.GetComponent<Army>();
            stats.team = teamB;
            stats.destination = destinationB;
            stats.size = sizeB;
        }
        else if (sizeB <= 0)
        {
            GameObject winner = Instantiate(armyA);
            Army stats = winner.GetComponent<Army>();
            stats.team = teamA;
            stats.destination = destinationA;
            stats.size = sizeA;
        }

        fightTime += fightSpeed * Time.deltaTime;
        if (fightTime >= 10)
        {
            sizeA =- 1;
            sizeB =- 1;
            fightTime = 0;
        }
    }
}
