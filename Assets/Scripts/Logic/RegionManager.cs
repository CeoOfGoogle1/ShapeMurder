using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// server only. NOT networkbehaviour
public class RegionManager : MonoBehaviour
{
    [SerializeField] List<Region> regions;
    
    public static RegionManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(this);


        // Temporary fucking shit
        regions = FindObjectsByType<Region>(FindObjectsSortMode.None).ToList();
    }

    public Region FindRegionById(int id)
    {
        foreach (Region region in regions)
        {
            if (region.id == id) return region;
        }
        
        Debug.LogWarning("No region with given id was found");
        return null;
    }
}
