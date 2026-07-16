using System.Linq;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public Army army;
    public bool retreating;
    public float retreatingTime;
    public float retreatingTimer;
    public bool cantCollide;
    bool previousRetreating;
    public GameObject battlePrefab;

    void Update()
    {
        Vector3 direction = army.destination.transform.position - transform.position;
        if (retreating)
        {
            direction = army.origin.transform.position - transform.position;
        }
        transform.position += direction.normalized * army.speed * Time.deltaTime;

        if (retreating != previousRetreating) if (retreating) cantCollide = true;
        previousRetreating = retreating;

        if (cantCollide)
        {
            if (retreatingTimer >= retreatingTime)
            {
                cantCollide = false;
                retreatingTimer = 0;
            }
            retreatingTimer += 1 * Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (cantCollide) return;

        if (collision.transform.TryGetComponent(out Battle battle)) battle.ReceiveMover(this);

        if (collision.transform.TryGetComponent(out Region region) && (retreating || region == army.destination))
        { 
            if (!region.ReceiveMover(this) && region == army.origin)
            {
                Destroy(gameObject);
            }
        }

       if (!collision.transform.TryGetComponent(out Mover other)) return;

        if (other.army.player.Id == army.player.Id ||
            other.army.player.Allies.Contains(army.player.Id)) return;

        // Both movers get this collision callback - only the lower InstanceID proceeds,
        // otherwise you'd spin up two separate Battles from one collision.
        if (GetInstanceID() > other.GetInstanceID()) return;
        
        GameObject battleObject = Instantiate(battlePrefab, transform.position, Quaternion.identity);
        Battle newBattle = battleObject.GetComponent<Battle>();

        bool otherConsumed = newBattle.ReceiveMover(other);
        bool thisConsumed = newBattle.ReceiveMover(this);
        if (otherConsumed) Destroy(other.gameObject);
        if (thisConsumed) Destroy(gameObject);
    }

    public static Mover Spawn(GameObject prefab, Army army, Vector3 position, Region origin, Region destination)
    {
        GameObject unit = Instantiate(prefab, position, Quaternion.identity);
        Mover mover = unit.GetComponent<Mover>();
        mover.army = army;
        mover.army.origin = origin;
        mover.army.destination = destination;
        return mover;
    }
}

[System.Serializable]
public class Army
{
    [Header("Army Settings")]
    public Player player;
    public int size;
    public float speed;
    public Region origin;
    public Region destination;

    public Army(Player player, int size, float speed, Region origin, Region destination)
    {
        this.player = player;
        this.size = size;
        this.speed = speed;
        this.origin = origin;
        this.destination = destination;
    }
}
