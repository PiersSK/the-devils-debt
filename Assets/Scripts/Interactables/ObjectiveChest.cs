using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ObjectiveChest : TreasureChest
{
    public override bool CanInteract()
    {
        return ObjectiveController.Instance.objectiveComplete && !isOpen.Value;
    }

    private void Update()
    {
        if (!ObjectiveController.Instance.objectiveComplete)
            SetPromptMessage("Locked!");
        else if (!isOpen.Value)
            SetPromptMessage("Loot objective!");
        else
            SetPromptMessage("");
    }

    protected override void Interact()
    {
        OpenObjectiveChestServerRpc();

    }

    [ServerRpc(RequireOwnership = false)]
    private void OpenObjectiveChestServerRpc()
    {
        isOpen.Value = true;
        OpenObjectiveChestClientRpc();
    }

    [ClientRpc]
    private void OpenObjectiveChestClientRpc()
    {
        GameOverSystem.Instance.GameOver(true);
    }
}
