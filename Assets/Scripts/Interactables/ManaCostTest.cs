using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaCostTest : Interactable
{

    public int manaCost = 15;

    protected override void Interact()
    {
        Player.LocalInstance.playerMana.IncrementPlayerMana(-manaCost);
    }

    //public override bool CanInteract()
    //{
    //    return manaCost <= Player.LocalInstance.playerMana.currentMana.Value;
    //}
}
