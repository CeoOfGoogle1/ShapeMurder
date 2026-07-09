using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour
{
    [Header("Battle Settings")]
    public List<Army> parties;
    public float fightSpeed;
    public float fightTime;

    void Update()
    {
        foreach (var party in parties)
        {
            if (party.size <= 0)
            {
                Destroy(party.gameObject);
            }
        }

        if (parties.Count < 1)
        {
            Destroy(gameObject);
        }

        if (parties.Count == 1)
        {
            parties[0].gameObject.SetActive(true);
            Destroy(gameObject);
        }

        fightTime += fightSpeed * Time.deltaTime;
        if (fightTime >= 10)
        {
            foreach (var party in parties) party.size--;
            fightTime = 0;
        }
    }

    public void ReceiveParty(Army party)
    {
        parties.Add(party);
        party.gameObject.SetActive(false);
    }
}
