using Unity.Netcode;
using UnityEngine;

public class TreasureChest : NetworkInteractable
{
    [SerializeField] private Animator anim;
    private NetworkVariable<bool> isOpen = new(false);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isOpen.OnValueChanged += ToggleOpen;
    }

    private void ToggleOpen(bool prevVal, bool newVal)
    {
        Debug.Log("toggling chest state");
        isOpen.Value = newVal;
        if(isOpen.Value)
        {
            anim.SetTrigger("OpenChest");
            SetPromptMessage(string.Empty);
        }
    }

    protected override void Interact()
    {
        Debug.Log("Chest interact happening");
        OpenChestServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OpenChestServerRpc()
    {
        Debug.Log("Chest serverRPC happening");
        if (!isOpen.Value)
            isOpen.Value = true;
            if (ObjectiveController.Instance.objectiveSelected == ObjectiveController.ObjectiveType.Keys)
                ObjectiveController.Instance.ProgressObjective();
    }
}
