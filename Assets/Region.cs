using System.Collections.Generic;
using UnityEngine;

public class Region : MonoBehaviour
{
    [Header("Region Settings")]
    public List<Region> neighbors;
    public Player player;
    public Army garrison;
    private Dictionary<int, Army> visitors = new();
    public int limit;

    [Header("Send Settings")]
    public GameObject newArmy;
    public Region destination;
    public int sendSize;
    public float sendTime;
    public float sendTimer = 0;

    [Header("Grow Settings")]
    public int growSize;
    public float growTime;
    public float growTimer = 0;

    [Header("Invasion Settings")]
    public Battle battle;

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
            Army sent = new Army(player, sendSize, 1);
            garrison.size -= sendSize;
            Mover.Spawn(newArmy, sent, transform.position, this, destination);
            sendTimer = 0;
        }
    }

    public bool SendVisitor(Player visitorPlayer, int amount, Region destination)
    {
        if (!visitors.TryGetValue(visitorPlayer.Id, out Army visitor)) return false;
        if (amount <= 0 || amount > visitor.size) return false;

        Army sent = new Army(visitorPlayer, amount, visitor.speed);
        visitor.size -= amount;
        if (visitor.size <= 0) visitors.Remove(visitorPlayer.Id);

        Mover.Spawn(newArmy, sent, transform.position, this, destination);
        return true;
    }

    public void ReceiveMover(Mover mover)
    {
        if (mover.army.player.Equals(player))
        {
            if (garrison.size >= limit)
            {
                mover.retreating = true;
            }
            else
            {
                garrison.size += mover.army.size;
                Destroy(mover.gameObject);
            }
        }
        else if (player.Allies.Contains(mover.army.player.Id))
        {
            if (!visitors.TryGetValue(mover.army.player.Id, out Army visitor))
            {
                visitors.Add(mover.army.player.Id, mover.army);
                Destroy(mover.gameObject);
            }
            else if (visitor.size < limit)
            {
                visitor.size += mover.army.size;
                Destroy(mover.gameObject);
            }
            else
            {
                mover.retreating = true;
            }
        }
        else
        {
            if (battle == null)
            {
                battle = gameObject.AddComponent<Battle>();

                // Seed both sides directly from data (no Mover needed for garrison).
                battle.SeedSide(mover.army);
                battle.SeedSide(garrison);

                // Each visitor decides for itself which side (if any) it belongs to.
                // If it's allied with both attacker and defender, SeedSide returns false
                // and it stays behind in the region, untouched by the battle.
                foreach (var kvp in new List<KeyValuePair<int, Army>>(visitors))
                {
                    if (battle.SeedSide(kvp.Value)) visitors.Remove(kvp.Key);
                }

                Destroy(mover.gameObject);
            }
            else
            {
                bool consumed = battle.ReceiveMover(mover);
                if (consumed) Destroy(mover.gameObject); // only destroy if it actually joined a side
            }
        }
    }

    void Update()
    {
        Grow();
        if (garrison.size >= sendSize && destination != null) Send();
    }

    public void SwitchTo(Player player, Army army)
    {
        this.player = player;
        garrison = army;
        battle = null;
    }
}
