using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionHandler : NetworkBehaviour
{
    public Region selectedRegion;
    private ObjectHighlighter objectHighlighter;

    void Start()
    {
        if (objectHighlighter == null) objectHighlighter = FindAnyObjectByType<ObjectHighlighter>(FindObjectsInactive.Include);
    }

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

                /*selectedRegion.selected = false;
                foreach(var Neighbor in selectedRegion.neighbors)
                {
                    Neighbor.highlighted = false;
                }*/

                objectHighlighter.gameObject.SetActive(false);
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
        Debug.Log($"region == null: {region == null}");
        Debug.Log($"objectHighlighter == null: {objectHighlighter == null}");

        if (region != null)
        {
            Debug.Log($"region.mesh == null: {region.mesh == null}");
        }

        if (objectHighlighter != null)
        {
            Debug.Log($"objectHighlighter.meshFilter == null: {objectHighlighter.meshFilter == null}");
        }


        selectedRegion = region;

        objectHighlighter.transform.position = region.transform.position;
        objectHighlighter.meshFilter.sharedMesh = region.mesh.sharedMesh;
        objectHighlighter.gameObject.SetActive(true);

        /*region.selected = true;
        foreach(var neighbor in selectedRegion.neighbors)
        {
            neighbor.highlighted = true;
        }*/
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
