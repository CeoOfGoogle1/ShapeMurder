using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Battle : MonoBehaviour
{
    [Header("Battle Settings")]
    public List<Side> sides;
    public float fightTime;
    public float fightTimer;
    public bool ended;

    void Update()
    {
        foreach (var side in sides)
        {
            foreach (var army in side.armies)
            {
                if (army.size <= 0)
                {
                    side.armies.Remove(army);
                    Destroy(army.gameObject);
                }
            }
        }

        sides.RemoveAll(s => s.armies.Count == 0);
        ended = sides.Count <= 1;

        if (ended)
        {
            foreach(var army in sides[0].armies)
            {
                army.gameObject.SetActive(true);
            }
            Destroy(gameObject);
        }

        Fight();
    }

    void Fight()
    {
        fightTimer += Time.deltaTime;
        if (fightTimer >= fightTime)
        {
            foreach (var side in sides)
            {
                foreach (var army in side.armies)
                {
                    army.size--;
                }
            }
            fightTimer = 0;
        }
    }

    public void ReceiveArmy(Army army)
    {
        List<Side> validSides = new();

        foreach (var side in sides)
        {
            if (side.IsFriendlyWith(army.player))
            {
                validSides.Add(side);
            }
        }

        if (validSides.Count == 0)
        {
            Side newSide = new Side();
            newSide.armies.Add(army);
            sides.Add(newSide);
            army.gameObject.SetActive(false);
        }
        else if (validSides.Count == 1)
        {
            validSides[0].armies.Add(army);
            army.gameObject.SetActive(false);
        }
        else
        {
            army.retreating = true;
        }
    }

    public void RetreatArmy(Army army)
    {
        foreach (var side in sides) side.armies.Remove(army);
        army.retreating = true;
        army.gameObject.SetActive(true);
        army.size--;
    }
}

[System.Serializable]
public class Side
{
    public List<Army> armies = new();

    public bool IsFriendlyWith(Player player)
    {
        foreach (var army in armies)
        {
            if (army.player.Equals(player)) continue;

            if (!army.player.Allies.Contains(player.Id)) return false;
        }
        return true;
    }
}
