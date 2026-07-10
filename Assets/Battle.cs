using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour
{
    [Header("Battle Settings")]
    public List<Army> armies;
    public float fightTime;
    public float fightTimer;
    public bool ended;

    void Update()
    {
        foreach (var army in armies)
        {
            if (army.size <= 0)
            {
                Destroy(army.gameObject);
            }
        }

        if (armies.Count < 1)
        {
            Destroy(gameObject);
        }

        if (armies.Count == 1)
        {
            armies[0].gameObject.SetActive(true);
            Destroy(gameObject);
        }

        if (ended)
        {
            foreach(var army in armies)
            {
                army.gameObject.SetActive(true);
            }
            Destroy(gameObject);
        }

        foreach (var army in armies)
        {
            Army a = army;
            foreach (var b in armies)
            {
                if (a != b)
                {
                    if (!a.player.Allies.Contains(b.player))
                    {
                        ended = false;
                    }
                    else
                    {
                        ended = true;
                    }
                }
            }
        }

        fightTimer += Time.deltaTime;
        if (fightTimer >= fightTime)
        {
            foreach (var army in armies) army.size--;
            fightTimer = 0;
        }
    }

    public void ReceiveArmy(Army army)
    {
        armies.Add(army);
        army.gameObject.SetActive(false);
    }

    public void RetreatArmy(Army army)
    {
        armies.Remove(army);
        army.retreating = true;
        army.gameObject.SetActive(true);
        army.size--;
    }
}
