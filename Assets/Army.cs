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
    void Update()
    {
        Vector3 direction = destination.transform.position - transform.position;
        if (retreating) { direction = origin.transform.position - transform.position; }
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        Region shape = collision.gameObject.GetComponent<Region>();
        if (shape != null)
        {
            if (shape.garrison.size < shape.cap)
            {
                shape.ReceiveArmy(this);
                Destroy(gameObject);
            }
            else
            {
                retreating = true;
            }
            
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
