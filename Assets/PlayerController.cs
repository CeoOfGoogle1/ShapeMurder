using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Player player;
    public Region selectedRegion;

    void Update()
    {
        if (Mouse.current == null) return;

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        if (hit.transform.TryGetComponent(out Region region))
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
            else if (selectedRegion == null && (region.player.Id == player.Id || region.player.Allies.Contains(player.Id)))
            {
                region.selected = true;
                selectedRegion = region;
                foreach(var neighbor in selectedRegion.neighbors)
                {
                    neighbor.highlighted = true;
                }
            }
        }
        else if (hit.transform.TryGetComponent(out Battle battle))
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
        else if (hit.transform.TryGetComponent(out Mover mover) && mover.army.player.Id == player.Id)
        {
            mover.retreating = true;
        }
    }
}
