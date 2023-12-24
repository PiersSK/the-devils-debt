using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Image healthBar;
    private NetworkVariable<int> currentHealth = new(3);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsServer) currentHealth.Value = maxHealth;
        currentHealth.OnValueChanged += UpdateCurrentHealth;
    }

    private void UpdateCurrentHealth(int prevVal, int newVal) {
        currentHealth.Value = newVal;
        float percHealth = (float)newVal / maxHealth;
        healthBar.fillAmount = percHealth;
        healthBar.color = new Color(healthBar.color.r, percHealth, healthBar.color.b);
        if (currentHealth.Value <= 0) DeathServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DamageToEnemyServerRpc(int damage)
    {
        currentHealth.Value = Math.Clamp(currentHealth.Value - damage, 0, maxHealth);
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeathServerRpc()
    {
        Destroy(gameObject);
        NetworkObject.Despawn();
    }
}
