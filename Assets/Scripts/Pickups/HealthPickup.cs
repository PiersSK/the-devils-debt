using Unity.Netcode;
using UnityEngine;

public class HealthPickup : Pickup
{
    private int healthValue = 2;

    protected override void OnTriggerEnter(Collider other)
    {
        if (CanPickUp(other.transform)) {
            other.GetComponent<PlayerHealth>().IncrementPlayerHealthServerRpc(healthValue);
            DespawnServerRpc();
        }
    }

    protected override bool CanPickUp(Transform player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        return playerHealth != null && playerHealth.currentHealth.Value != playerHealth.maxHealth;

    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnServerRpc()
    {
        Destroy(gameObject);
        GetComponent<NetworkObject>().Despawn();
    }
}
