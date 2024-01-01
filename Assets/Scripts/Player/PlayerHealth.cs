using System;
using System.Linq.Expressions;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    public int maxHealth = 10;
    public NetworkVariable<int> currentHealth = new(10);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentHealth.OnValueChanged += SyncPlayerHeath;
    }

    private void SyncPlayerHeath(int prevVal, int newVal)
    {
        currentHealth.Value = Math.Clamp(newVal, 0, maxHealth);
        if (IsOwner)
        {
            UIManager.Instance.health.UpdateBar(currentHealth.Value, maxHealth);
            if(prevVal > newVal)
            {
                UIManager.Instance.health.ShowOverlay(3f, false);
            }
            else if(prevVal < newVal)
            {
                UIManager.Instance.health.ShowOverlay(3f, true);
            }
        }

        if(currentHealth.Value == 0)
        {
            UIManager.Instance.gameOver.GameOver(false);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void IncrementPlayerHealthServerRpc(int incr)
    {
        currentHealth.Value += incr;
    }
}
