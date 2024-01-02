using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ObjectiveChest : TreasureChest
{
    public override bool CanInteract()
    {
        return ObjectiveController.Instance.objectiveComplete;
    }

    private void Update()
    {
        if (!ObjectiveController.Instance.objectiveComplete)
            SetPromptMessage(string.Empty);
        else
            SetPromptMessage("Loot objective!");
    }

    protected override void Interact()
    {
        OpenChestServerRpc();
        UIManager.Instance.gameOver.GameOverServerRpc(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OpenChestServerRpc()
    {
        if (!isOpen.Value)
        {
            isOpen.Value = true;
        }
    }
}
