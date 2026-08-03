using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Region : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer regionRenderer;
    [SerializeField] private Material outlineMaterial;
    [Header("Region Settings")]
    public int ownerId;
    public bool selected;
    public bool highlighted;
    public int id;
    public RegionType type;
    public List<Region> neighbors;
    public Army garrison;
    public Dictionary<int, Army> visitors = new();
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
        if (NetworkManager.Singleton == null) return;
        if (!NetworkManager.Singleton.IsHost) return;

        garrison.ownerId = ownerId;
        if (garrison.size >= sendSize && destination != null)
        {
            if (Tick(ref sendTimer, sendTime)) SendArmy(garrison, sendSize, destination);
        }
        Grow();

        if (selected)
        {
            
        }
        else
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
        Army sent = new Army(army.ownerId, amount, army.speed, this, destination);
        army.size -= amount;
        Mover.Spawn(moverPrefab, sent, transform.position, this, destination);
        return true;
    }

    public bool SendVisitor(PlayerData visitorPlayer, int amount, Region destination)
    {
        if (!visitors.TryGetValue((int)visitorPlayer.ClientId, out Army visitor)) return false;
        if (!SendArmy(visitor, amount, destination)) return false;
        if (visitor.size <= 0) visitors.Remove((int)visitorPlayer.ClientId);
        return true;
    }

    public bool ReceiveMover(Mover mover)
    {
        if (mover.army.ownerId == ownerId)
        {
            if (garrison.size >= limit)
            {
                mover.retreating = true;
                return false;
            }
            else
            {
                garrison.size += mover.army.size;
                Destroy(mover.gameObject);
                return true;
            }
        }
        else if (Utilities.CheckIfHasAlly(ownerId, (int)mover.army.ownerId))
        {
            if (!visitors.TryGetValue((int)mover.army.ownerId, out Army visitor))
            {
                visitors.Add((int)mover.army.ownerId, mover.army);
                Destroy(mover.gameObject);
                return true;
            }
            else if (visitor.size < limit)
            {
                visitor.size += mover.army.size;
                Destroy(mover.gameObject);
                return true;
            }
            else
            {
                mover.retreating = true;
                return false;
            }
        }
        else if (battle == null)
        {
            battle = gameObject.AddComponent<Battle>();
            battle.ReceiveMover(mover);
            Destroy(mover.gameObject);
            battle.ReceiveArmy(garrison);
            battle.moverPrefab = moverPrefab;
            battle.fightTime = moverPrefab.GetComponent<Mover>().battlePrefab.GetComponent<Battle>().fightTime;
            foreach (var kvp in new List<KeyValuePair<int, Army>>(visitors))
            {
                if (battle.ReceiveArmy(kvp.Value)) 
                {
                    visitors.Remove(kvp.Key);
                    return true;
                }
                else
                {
                    mover.retreating = true;
                    return false;
                }

            }
        }
        else if (battle.ReceiveMover(mover)) Destroy(mover.gameObject);
        return true;
    }

    public void SwitchTo(int newOwnerId, Army army)
    {
        this.ownerId = newOwnerId;
        garrison = army;
        battle = null;
        destination = null;
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
