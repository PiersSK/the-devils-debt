using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ManaPickup : Pickup
{
    private int manaValue = 1;

    protected override void OnTriggerEnter(Collider other)
    {
        PlayerMana playerMana = other.GetComponent<PlayerMana>();
        if (playerMana != null && playerMana.currentMana.Value != playerMana.maxMana)
        {
            playerMana.IncrementPlayerManaServerRpc(manaValue);
            DespawnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnServerRpc()
    {
        Destroy(gameObject);
        GetComponent<NetworkObject>().Despawn();
    }
}
