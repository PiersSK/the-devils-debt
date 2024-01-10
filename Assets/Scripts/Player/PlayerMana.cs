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

    //Returns bool to let you know if you could spend this or not
    public bool IncrementPlayerMana(int incr)
    {
        if(currentMana.Value + incr < 0)
        {
            UIManager.Instance.notification.ShowNotification("Insufficient Mana!");
            return false;
        }
        else
        {
            IncrementPlayerManaServerRpc(incr);
            return true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncrementPlayerManaServerRpc(int incr)
    {

        currentMana.Value += incr;
    }
}
