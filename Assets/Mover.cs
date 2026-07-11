using UnityEngine;

public class Mover : MonoBehaviour
{
    public Army army;
    public Region origin;
    public Region destination;
    public bool retreating;
    bool previousRetreating;
    float retreatingTimer;
    bool cantCollide;
    public GameObject battle;

    void Update()
    {
        Vector3 direction = destination.transform.position - transform.position;
        if (retreating)
        {
            direction = origin.transform.position - transform.position;
        }
        transform.position += direction.normalized * army.speed * Time.deltaTime;

        if (retreating != previousRetreating)
        {
            if (retreating) cantCollide = true;
        }
        previousRetreating = retreating;

        if (cantCollide)
        {
            if (retreatingTimer >= 1)
            {
                cantCollide = false;
                retreatingTimer = 0;
            }
            retreatingTimer += 1 * Time.deltaTime;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (cantCollide) return;

        Battle combat = collision.gameObject.GetComponent<Battle>();
        if (combat != null)
        {
            combat.ReceiveMover(this);
        }

        Region region = collision.gameObject.GetComponent<Region>();
        if (region != null)
        {
            region.ReceiveMover(this);
        }

        Mover other = collision.gameObject.GetComponent<Mover>();
        if (other == null || other.army.player.Allies.Contains(army.player.Id)) return;

        // Both movers get this collision callback - only the lower InstanceID proceeds,
        // otherwise you'd spin up two separate Battles from one collision.
        if (GetInstanceID() > other.GetInstanceID()) return;
        
        GameObject newBattle = Instantiate(battle, transform.position, Quaternion.identity);
        Battle stats = newBattle.GetComponent<Battle>();
        stats.ReceiveMover(other);
        stats.ReceiveMover(this);

        bool otherConsumed = stats.ReceiveMover(other);
        bool thisConsumed = stats.ReceiveMover(this);
        if (otherConsumed) Destroy(other.gameObject);
        if (thisConsumed) Destroy(gameObject);
    }

    // The one place a Mover GameObject comes into existence for an existing Army.
    // Region.Send/SendVisitor and Battle's post-fight respawn all funnel through here,
    // so there's exactly one path that creates a moving unit and exactly one rule
    // for destroying it (whenever it stops moving under its own power).
    public static Mover Spawn(GameObject prefab, Army army, Vector3 position, Region origin, Region destination)
    {
        GameObject unit = Instantiate(prefab, position, Quaternion.identity);
        Mover mover = unit.GetComponent<Mover>();
        mover.army = army;
        mover.origin = origin;
        mover.destination = destination;
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

    public Army(Player player, int size, float speed)
    {
        this.player = player;
        this.size = size;
        this.speed = speed;
    }
}
