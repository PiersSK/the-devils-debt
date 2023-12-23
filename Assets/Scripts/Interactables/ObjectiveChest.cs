using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveChest : TreasureChest
{

    private void Update()
    {
        if (!ObjectiveController.Instance.objectiveComplete)
            SetPromptMessage(string.Empty);
        else
            SetPromptMessage("Loot objective!");
    }

    protected override void Interact()
    {
        if (ObjectiveController.Instance.objectiveComplete)
            base.Interact();
    }
}
