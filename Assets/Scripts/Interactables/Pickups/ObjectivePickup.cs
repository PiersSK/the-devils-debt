using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ObjectivePickup : PickupInteractable
{
    protected override void Interact()
    {
        PickupKeyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PickupKeyServerRpc()
    {
        ObjectiveController.Instance.ProgressObjective();
        Destroy(gameObject);
        GetComponent<NetworkObject>().Despawn();
    }
}
