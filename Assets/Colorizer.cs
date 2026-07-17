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
    }

    void Update()
    {
        if (mover)
        {
            material.color = mover.army.player.Color;
        }
        else if (battle)
        {
            
        }
        else if (region)
        {
            material.color = region.garrison.player.Color;
        }
    }
}
