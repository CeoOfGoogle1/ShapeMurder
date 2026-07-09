using UnityEngine;

public class Army : MonoBehaviour
{
    [Header("Army Settings")]
    public int team;
    public Shape destination;
    public int size;
    public float speed;
    public GameObject fight;
    public GameObject army;
    public bool merging;

    void Move()
    {
        Vector3 direction = transform.position - destination.transform.position;
        transform.position += direction * speed * Time.deltaTime;
    }

    void Update()
    {
        Move();
    }

    void OnCollisionEnter(Collision collision)
    {
        Army other = collision.gameObject.GetComponent<Army>();
        if (other == null || other.merging) return;
        merging = true;
        
        GameObject newFight = Instantiate(fight);
        Fight stats = newFight.GetComponent<Fight>();
        stats.teamA = team;
        stats.destinationA = destination;
        stats.sizeA = size;
        stats.armyA = army;
        stats.teamB = other.team;
        stats.destinationB = other.destination;
        stats.sizeB = other.size;
        stats.armyB = other.army;
        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}
