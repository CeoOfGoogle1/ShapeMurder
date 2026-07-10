using UnityEngine;

public class Army : MonoBehaviour
{
    [Header("Army Settings")]
    public Player player;
    public Region origin;
    public Region destination;
    public int size;
    public float speed;
    public GameObject battle;
    public bool fighting;
    public bool retreating;
    bool previousRetreating;
    float retreatingTimer;
    bool cantCollide;
    void Update()
    {
        Vector3 direction = destination.transform.position - transform.position;
        if (retreating)
        {
            direction = origin.transform.position - transform.position;
        }
        transform.position += direction * speed * Time.deltaTime;

        if (retreating != previousRetreating)
        {
            if (retreating)
            {
                cantCollide = true;
            }
        }

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

        Region shape = collision.gameObject.GetComponent<Region>();
        if (shape != null)
        {
            shape.ReceiveArmy(this);
        }

        Battle combat = collision.gameObject.GetComponent<Battle>();
        if (combat != null)
        {
            combat.ReceiveArmy(this);
        }

        Army other = collision.gameObject.GetComponent<Army>();
        if (other == null || other.fighting || other.player.Equals(player)) return;
        fighting = true;
        
        GameObject newBattle = Instantiate(battle, transform.position, Quaternion.identity);
        Battle stats = newBattle.GetComponent<Battle>();
        stats.ReceiveArmy(this);
    }
}
