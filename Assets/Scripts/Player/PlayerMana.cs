using System;
using Unity.Netcode;

public class PlayerMana : NetworkBehaviour
{
    public int maxMana = 10;
    public NetworkVariable<int> currentMana = new(10);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentMana.OnValueChanged += SyncPlayerMana;
    }

    private void SyncPlayerMana(int prevVal, int newVal)
    {
        currentMana.Value = Math.Clamp(newVal, 0, maxMana);
        if (IsOwner)
        {
            UIManager.Instance.mana.UpdateBar(currentMana.Value, maxMana);
            if (prevVal < newVal)
            {
                UIManager.Instance.mana.ShowOverlay(3f, false);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncrementPlayerManaServerRpc(int incr)
    {
        currentMana.Value += incr;
    }
}
