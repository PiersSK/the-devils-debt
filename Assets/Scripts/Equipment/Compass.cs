using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Compass : Equipment
{
    [SerializeField] private Transform compassNeedle;
    [SerializeField] private int manaCost;

    private enum CompassDestination
    {
        Objective,
        PuzzleRoom
    }

    private CompassDestination destination = CompassDestination.Objective;
    private List<string> destinationName = new() { "Objective", "Puzzle Room" };

    private void Update()
    {
        if (isPickedUp) // Point it north (ish)
            compassNeedle.localEulerAngles = -Player.LocalInstance.transform.eulerAngles;
    }

    public override void PerformAbility()
    {
        if (!onCooldown && Player.LocalInstance.playerMana.IncrementPlayerMana(-manaCost))
        {
            string compassResponse = "";
            if(destination == CompassDestination.Objective)
                compassResponse = Dungeon.Instance.GetPathToObjective().ToString();
            else if(destination == CompassDestination.PuzzleRoom)
                compassResponse = Dungeon.Instance.GetPathToPuzzle().ToString();

            if (compassResponse == "None") compassResponse = "Destination Reached";

            Color nofifcationColor = destination == CompassDestination.Objective ? new Color(1, 0.8f, 0.05f) : new Color(0.55f, 0.25f, 0.66f); 

            UIManager.Instance.notification.ShowNotification(compassResponse, nofifcationColor);
            UIManager.Instance.hotbarAccessory.PutOnCooldown(1f);
        }
    }

    public override void ResetAbility()
    {
    }

    public override void SetAnimations()
    {
    }

    public override void PerformAlt()
    {
        int objectiveOptions = Enum.GetValues(typeof(CompassDestination)).Cast<int>().Max();
        if ((int)destination < objectiveOptions)
            destination = (CompassDestination)((int)destination + 1);
        else
            destination = 0;

        UpdateAltUI();

    }

    public override void UpdateAltUI()
    {
        UIManager.Instance.equipmentPrompt.text = "[F] Toggle Destination";
        UIManager.Instance.equipmentDetails.text = "Destination: " + destinationName[(int)destination];

        UIManager.Instance.equipmentPrompt.gameObject.SetActive(true);
        UIManager.Instance.equipmentDetails.gameObject.SetActive(true);
    }
}
