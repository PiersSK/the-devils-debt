using Unity.Netcode;
using UnityEngine;

public class HealthPickup : Pickup
{
    private int healthValue = 2;

    protected override void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth= other.GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.currentHealth.Value != playerHealth.maxHealth) {
            playerHealth.IncrementPlayerHealthServerRpc(healthValue);
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
