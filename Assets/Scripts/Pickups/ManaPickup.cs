using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ManaPickup : Pickup
{
    private int manaValue = 1;

    protected override void OnTriggerEnter(Collider other)
    {
        if (CanPickUp(other.transform))
        {
            other.GetComponent<PlayerMana>().IncrementPlayerMana(manaValue);
            DespawnServerRpc();
        }
    }

    protected override bool CanPickUp(Transform player)
    {
        PlayerMana playerMana = player.GetComponent<PlayerMana>();
        return playerMana != null && playerMana.currentMana.Value != playerMana.maxMana;

    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnServerRpc()
    {
        Destroy(gameObject);
        GetComponent<NetworkObject>().Despawn();
    }
}
