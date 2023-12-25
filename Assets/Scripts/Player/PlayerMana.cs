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
        if(IsOwner) UpdateManaUI();
    }

    private void UpdateManaUI()
    {
        UIManager.Instance.playerUI_manaVal.text = currentMana.Value.ToString();
        UIManager.Instance.playerUI_manaBar.fillAmount = (float)currentMana.Value / maxMana;
    }

    [ServerRpc]
    public void IncrementPlayerManaServerRpc(int incr)
    {
        currentMana.Value += incr;
    }
}
