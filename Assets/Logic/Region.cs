using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Region : MonoBehaviour
{
    [Header("Region Settings")]
    public bool selected;
    public bool highlighted;
    public int id;
    public RegionType type;
    public List<Region> neighbors;
    public Player player;
    public Army garrison;
    private Dictionary<int, Army> visitors = new();
    public int limit;

    [Header("Send Settings")]
    public GameObject moverPrefab;
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

    void Update()
    {   
        if (garrison.size >= sendSize && destination != null)
        {
            if (Tick(ref sendTimer, sendTime)) SendArmy(garrison, sendSize, destination);
        }
        Grow();

        if (selected)
        {
            
        }

        if (highlighted)
        {
            
        }
    }

    void Grow()
    {
        if (garrison.size >= limit) return;
        if (Tick(ref growTimer, growTime)) garrison.size += growSize;
    }

    bool SendArmy(Army army, int amount, Region destination)
    {
        if (amount > army.size) return false;
        Army sent = new Army(army.player, amount, army.speed, this, destination);
        army.size -= amount;
        Mover.Spawn(moverPrefab, sent, transform.position, this, destination);
        return true;
    }

    public bool SendVisitor(Player visitorPlayer, int amount, Region destination)
    {
        if (!visitors.TryGetValue(visitorPlayer.Id, out Army visitor)) return false;
        if (!SendArmy(visitor, amount, destination)) return false;
        if (visitor.size <= 0) visitors.Remove(visitorPlayer.Id);
        return true;
    }

    public void ReceiveMover(Mover mover)
    {
        if (mover.army.player.Equals(player))
        {
            if (garrison.size >= limit) mover.retreating = true;
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
            else mover.retreating = true;
        }
        else if (battle == null)
        {
            battle = gameObject.AddComponent<Battle>();
            battle.ReceiveMover(mover);
            battle.ReceiveArmy(garrison);
            foreach (var kvp in new List<KeyValuePair<int, Army>>(visitors))
            {
                if (battle.ReceiveArmy(kvp.Value)) visitors.Remove(kvp.Key);
            }
            Destroy(mover.gameObject);
        }
        else if (battle.ReceiveMover(mover)) Destroy(mover.gameObject);
    }

    public void SwitchTo(Player player, Army army)
    {
        this.player = player;
        garrison = army;
        battle = null;
    }

    bool Tick(ref float timer, float interval)
    {
        timer += Time.deltaTime;
        if (timer < interval) return false;
        timer = 0;
        return true;
    }
}

public class RegionType
{
    public int cost;
    public int gain;
}
