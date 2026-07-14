using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Player player;
    public Region selectedRegion;

    void Update()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Region region = hit.transform.GetComponent<Region>();
                Battle battle = hit.transform.GetComponent<Battle>();
                Mover mover = hit.transform.GetComponent<Mover>();
                if (region != null)
                {
                    if (selectedRegion != null && region != selectedRegion)
                    {
                        foreach (var neighbor in selectedRegion.neighbors)
                        {
                            if (region == neighbor)
                            {
                                selectedRegion.destination = region;
                                selectedRegion.selected = false;
                                foreach(var Neighbor in selectedRegion.neighbors)
                                {
                                    Neighbor.highlighted = false;
                                }
                                selectedRegion = null;
                            }
                        }
                    }
                    else if (region.player.Equals(player) || region.player.Allies.Contains(player.Id))
                    {
                        region.selected = true;
                        selectedRegion = region;
                        foreach(var neighbor in selectedRegion.neighbors)
                        {
                            neighbor.highlighted = true;
                        }
                    }
                }
                else if (battle != null)
                {
                    foreach (var side in battle.sides)
                    {
                        foreach (var army in side.armies)
                        {
                            if (army.player.Equals(player))
                            {
                                battle.RetreatArmy(army);
                            }
                        }
                    }
                }
                else if (mover != null && mover.army.player.Equals(player))
                {
                    mover.retreating = true;
                }
            }
        }
    }
}
