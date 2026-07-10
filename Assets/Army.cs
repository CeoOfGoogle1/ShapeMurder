using UnityEngine;

public class Army : MonoBehaviour
{
    [Header("Army Settings")]
    public int team;
    public Region origin;
    public Region destination;
    public int size;
    public float speed;
    public GameObject battle;
    public bool fighting;
    public bool retreating;
    bool previousRetreating;
    float retreatingTime;
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
            if (retreatingTime >= 1)
            {
                cantCollide = false;
                retreatingTime = 0;
            }
            retreatingTime += 1 * Time.deltaTime;
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
            combat.ReceiveParty(this);
        }

        Army other = collision.gameObject.GetComponent<Army>();
        if (other == null || other.fighting || other.team == team) return;
        fighting = true;
        
        GameObject newBattle = Instantiate(battle, transform.position, Quaternion.identity);
        Battle stats = newBattle.GetComponent<Battle>();
        stats.ReceiveParty(this);
    }
}
