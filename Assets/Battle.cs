using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Battle : MonoBehaviour
{
    [Header("Battle Settings")]
    public List<Side> sides;
    public float fightTime;
    public float fightTimer;
    public bool ended;
    public GameObject newMover;

    // Remembers where a mover-derived army was headed, so a surviving field-battle
    // army can be respawned as a Mover continuing its journey after the fight ends.
    // Armies seeded directly from a Region (garrison/visitors) never get an entry here,
    // which is exactly how we know they belong to the region rather than back on the road.
    private readonly Dictionary<Army, (Region origin, Region destination)> routes = new();

    void Update()
    {
        foreach (var side in sides)
        {
            side.armies.RemoveAll(army => army.size <= 0);
        }
        sides.RemoveAll(s => s.armies.Count == 0);

        if (sides.Count == 0)
        {
            // Mutual annihilation - nobody survived on either side.
            Region wipedRegion = GetComponent<Region>();
            if (wipedRegion != null) wipedRegion.battle = null;
            Destroy(gameObject);
            return;
        }

        ended = sides.Count == 1;
        if (ended)
        {
            ResolveBattle();
            return;
        }

        Fight();
    }

    void ResolveBattle()
    {
        Side winner = sides[0];
        Region region = GetComponent<Region>();

        if (region != null)
        {
            // Merge the WHOLE winning side into the new garrison, not just the biggest army —
            // otherwise allied survivors beyond the single largest army just evaporate.
            int total = 0;
            Army leader = winner.armies[0];
            foreach (var army in winner.armies)
            {
                total += army.size;
                if (army.size > leader.size) leader = army;
            }

            region.SwitchTo(leader.player, new Army(leader.player, total, leader.speed));
            Destroy(this);
        }
        else
        {
            // Field battle: every survivor that came from a Mover gets a fresh Mover
            // spawned so it can continue toward its original destination.
            foreach (var army in winner.armies)
            {
                if (routes.TryGetValue(army, out var route))
                {
                    Mover.Spawn(newMover, army, transform.position, route.origin, route.destination);
                }
            }
            Destroy(gameObject);
        }
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

    // Adds an Army (no Mover attached) to whichever side it's friendly with,
    // or starts a new side if it's an enemy of everyone currently fighting.
    // Returns false — and does nothing — if the army is friendly with more than
    // one existing side (ambiguous loyalty), so callers can leave it out of the fight.
    public bool SeedSide(Army army)
    {
        List<Side> validSides = new();
        foreach (var side in sides)
            if (side.IsFriendlyWith(army.player)) validSides.Add(side);

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
        return false; // allied with two or more warring sides - sits this one out
    }

    // For movers arriving mid-battle: same side-selection logic as SeedSide,
    // plus route bookkeeping and telling the caller whether to destroy the Mover.
    public bool ReceiveMover(Mover mover)
    {
        if (SeedSide(mover.army))
        {
            routes[mover.army] = (mover.origin, mover.destination);
            return true; // caller should Destroy the mover's GameObject
        }
        mover.retreating = true; // ambiguous loyalty - let it turn back, don't destroy it
        return false;
    }

    public void RetreatArmy(Army army)
    {
        foreach (var side in sides) side.armies.Remove(army);
        army.size--;

        if (routes.TryGetValue(army, out var route))
        {
            Mover retreater = Mover.Spawn(newMover, army, transform.position, route.origin, route.destination);
            retreater.retreating = true;
            routes.Remove(army);
        }
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
