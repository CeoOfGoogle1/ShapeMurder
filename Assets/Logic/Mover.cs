using UnityEngine;

public class Mover : MonoBehaviour
{
    public Army army;
    public bool retreating;
    bool previousRetreating;
    float retreatingTimer;
    bool cantCollide;
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
        if (combat != null) combat.ReceiveMover(this);

        Region region = collision.gameObject.GetComponent<Region>();
        if (region != null) region.ReceiveMover(this);

        Mover other = collision.gameObject.GetComponent<Mover>();
        if (other == null || other.army.player.Allies.Contains(army.player.Id)) return;

        // Both movers get this collision callback - only the lower InstanceID proceeds,
        // otherwise you'd spin up two separate Battles from one collision.
        if (GetInstanceID() > other.GetInstanceID()) return;
        
        GameObject newBattle = Instantiate(battlePrefab, transform.position, Quaternion.identity);
        Battle battle = newBattle.GetComponent<Battle>();
        battle.ReceiveMover(other);
        battle.ReceiveMover(this);

        bool otherConsumed = battle.ReceiveMover(other);
        bool thisConsumed = battle.ReceiveMover(this);
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
