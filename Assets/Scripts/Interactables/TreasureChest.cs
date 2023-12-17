using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : Interactable
{
    [SerializeField] private Animator anim;
    protected ObjectiveController objective;
    protected bool isOpen = false;

    private void Start()
    {
        objective = ObjectiveController.GetObjectiveController();
    }

    protected override void Interact()
    {
        if (!isOpen)
        {
            anim.SetTrigger("OpenChest");
            if (objective.objectiveSelected == ObjectiveController.ObjectiveType.Keys)
                objective.ProgressObjective();
            isOpen = true;
            promptMessage = string.Empty;
        }
    }
}
