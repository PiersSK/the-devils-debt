using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : Equipment
{
    [SerializeField] private Transform compassNeedle;
    [SerializeField] private int manaCost;

    private void Update()
    {
        if (isPickedUp) // Point it north (ish)
            compassNeedle.localEulerAngles = -Player.LocalInstance.transform.eulerAngles;
    }

    public override void PerformAbility()
    {
        if (!onCooldown && Player.LocalInstance.playerMana.IncrementPlayerMana(-manaCost))
        {
            string compassResponse = Dungeon.Instance.GetPathToObjective().ToString();
            if (compassResponse == "None") compassResponse = "Destination Reached";

            UIManager.Instance.notification.ShowNotification(compassResponse, Color.cyan);
            UIManager.Instance.hotbarAccessory.PutOnCooldown(1f);
        }
    }

    public override void ResetAbility()
    {
    }

    public override void SetAnimations()
    {
    }
}
