using UnityEngine;

public class Colorizer : MonoBehaviour
{
    Renderer render;
    Material material;
    Mover mover;
    Region region;
    Battle battle;

    void Start()
    {
        render = GetComponent<Renderer>();
        material = render.material;
        mover = GetComponentInParent<Mover>();
        battle = GetComponentInParent<Battle>();
        region = GetComponentInParent<Region>();

        
        if (PlayerDataManager.Instance == null) {return;}

        if (mover)
        {
            material.color = PlayerDataManager.Instance.FindPlayerDataByClientId((ulong)mover.army.ownerId).Color;
        }
        else if (battle)
        {
            
        }
        else if (region)
        {
            material.color = PlayerDataManager.Instance.FindPlayerDataByClientId((ulong)region.garrison.ownerId).Color;
        }
    }

    void Update()
    {
        if (PlayerDataManager.Instance == null) {return;}

        if (mover)
        {
            material.color = PlayerDataManager.Instance.FindPlayerDataByClientId((ulong)mover.army.ownerId).Color;
        }
        else if (battle)
        {
            
        }
        else if (region)
        {
            material.color = PlayerDataManager.Instance.FindPlayerDataByClientId((ulong)region.garrison.ownerId).Color;
        }
    }
}
