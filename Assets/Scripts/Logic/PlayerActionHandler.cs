using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionHandler : NetworkBehaviour
{
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
                if(!IsHost)
                {
                    RequestSetDestinationServerRpc(selectedRegion.id, region.id);

                    Debug.Log("Requesting server to set destination");
                }
                else
                {
                    SetDestination(selectedRegion, region);
                }

                selectedRegion.selected = false;
                foreach(var Neighbor in selectedRegion.neighbors)
                {
                    Neighbor.highlighted = false;
                }
                selectedRegion = null;
            }
            else if (selectedRegion == null && (region.ownerId == (int)NetworkManager.Singleton.LocalClientId || Utilities.CheckIfHasAlly(region.ownerId, (int)NetworkManager.Singleton.LocalClientId)))
            {
                SelectRegion(region);
            }
        }
        else if (hit.transform.TryGetComponent(out Battle battle))
        {
            RetreatUnitFromBattle(battle);
        }
        else if (hit.transform.TryGetComponent(out Mover mover) && mover.army.ownerId == (int)NetworkManager.Singleton.LocalClientId)
        {
            mover.retreating = true;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestSetDestinationServerRpc(int fromRegionId, int destinationRegionId, RpcParams rpcParams = default)
    {
        Debug.Log($"Received request to set destination from {fromRegionId} to {destinationRegionId}");

        if (!IsHost) return;

        Debug.Log("Its host. Proceed");

        Region fromRegion = RegionManager.Instance.FindRegionById(fromRegionId);

        if (fromRegion.ownerId != (int)rpcParams.Receive.SenderClientId)
        {
            Debug.Log("from region owner Id is NOT Sender client id");
            return;
        }

        Region destinationRegion = RegionManager.Instance.FindRegionById(destinationRegionId);

        SetDestination(fromRegion, destinationRegion);
    }

    private void SetDestination(Region fromRegion, Region destinationRegion)
    {
        Debug.Log("Trying to set destination..");

        foreach (var neighbor in fromRegion.neighbors)
        {
            if (destinationRegion == neighbor)
            {
                Debug.Log("Destination Region is a neighbor of fromregion. Operation Succesful.");
                fromRegion.destination = destinationRegion;
            }
        }
    }

    private void SelectRegion(Region region)
    {
        region.selected = true;
        selectedRegion = region;
        foreach(var neighbor in selectedRegion.neighbors)
        {
            neighbor.highlighted = true;
        }
    }

    private void RetreatUnitFromBattle(Battle battle)
    {
        foreach (var side in battle.sides)
        {
            foreach (var army in side.armies)
            {
                if (army.ownerId == (int)NetworkManager.Singleton.LocalClientId)
                {
                    battle.RetreatArmy(army);
                }
            }
        }
    }
}
