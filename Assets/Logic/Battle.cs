using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour
{
    [Header("Battle Settings")]
    public List<Side> sides;
    public float fightTime;
    public float fightTimer;
    public bool ended;
    public GameObject moverPrefab;

    void Update()
    {
        foreach (var side in sides) side.armies.RemoveAll(army => army.size <= 0);
        sides.RemoveAll(s => s.armies.Count == 0);

        if (sides.Count == 0)
        {
            // Everyone died
            Region region = GetComponent<Region>();
            if (region != null) region.battle = null;
            Destroy(gameObject);
        }

        ended = sides.Count == 1;
        if (ended) Win();
        else if (Tick(ref fightTimer, fightTime)) Fight(sides);
    }

    void Win()
    {
        Side winner = sides[0];
        Region region = GetComponent<Region>();

        if (region != null)
        {
            // Siege, biggest army gets the region, others move on
            Army leader = winner.armies[0];
            foreach (var army in winner.armies) if (army.size > leader.size) leader = army;

            foreach (var army in winner.armies)
            {
                if (army == leader) return;
                Mover.Spawn(moverPrefab, army, transform.position, army.origin, army.destination);
            }
            region.SwitchTo(leader.player, new Army(leader.player, leader.size, leader.speed, null, null));
            Destroy(this);
        }
        else
        {
            // Field battle, everyone goes moves on
            foreach (var army in winner.armies)
            {
                Mover.Spawn(moverPrefab, army, transform.position, army.origin, army.destination);
            }
            Destroy(gameObject);
        }
    }

    void Fight(List<Side> sides)
    {
        foreach (var side in sides)
        {
            foreach (var army in side.armies)
            {
                army.size--;
            }
        }
    }

    public bool ReceiveMover(Mover mover)
    {
        if (ReceiveArmy(mover.army)) return true;
        mover.retreating = true; //allied with two or more sides
        return false;
    }

    public bool ReceiveArmy(Army army)
    {
        List<Side> validSides = new();
        foreach (var side in sides) if (side.IsFriendlyWith(army.player)) validSides.Add(side);

        if (validSides.Count == 1)
        {
            validSides[0].AddArmy(army);
            return true;
        }
        if (validSides.Count == 0)
        {
            Side newSide = new Side();
            newSide.AddArmy(army);
            sides.Add(newSide);
            return true;
        }
        return false;
    }

    public void RetreatArmy(Army army)
    {
        foreach (var side in sides) side.armies.Remove(army);
        // Cost of retreating
        army.size--;
        Mover retreater = Mover.Spawn(moverPrefab, army, transform.position, army.origin, army.destination);
        retreater.retreating = true;
    }

    bool Tick(ref float timer, float interval)
    {
        timer += Time.deltaTime;
        if (timer < interval) return false;
        timer = 0;
        return true;
    }
}

[System.Serializable]
public class Side
{
    public List<Army> armies = new();

    public void AddArmy(Army incoming)
    {
        foreach (var army in armies)
        {
            if (army.player.Equals(incoming.player))
            {
                army.size += incoming.size;
                return;
            }
        }
        armies.Add(incoming);
    }

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
