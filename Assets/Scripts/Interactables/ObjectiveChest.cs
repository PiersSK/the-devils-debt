using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveChest : TreasureChest
{

    private void Update()
    {
        if (!objective.objectiveComplete)
            promptMessage = string.Empty;
        else
            promptMessage = "Loot objective!";
    }

    protected override void Interact()
    {
        if (objective.objectiveComplete)
            base.Interact();
    }
}
