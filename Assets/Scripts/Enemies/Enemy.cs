using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private NetworkVariable<int> currentHealth = new(3);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentHealth.Value = maxHealth;
        currentHealth.OnValueChanged += UpdateCurrentHealth;
    }

    private void UpdateCurrentHealth(int prevVal, int newVal) {
        currentHealth.Value = newVal;
        if (currentHealth.Value <= 0) DeathServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DamageToEnemyServerRpc(int damage)
    {
        currentHealth.Value -= damage;
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeathServerRpc()
    {
        Destroy(gameObject);
        NetworkObject.Despawn();
    }
}
