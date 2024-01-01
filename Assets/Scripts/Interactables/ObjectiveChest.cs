using System.Collections;
using System.Collections.Generic;
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
        if (!isOpen.Value)
            isOpen.Value = true;
            UIManager.Instance.gameOver.GameOver(true);
    }
}
