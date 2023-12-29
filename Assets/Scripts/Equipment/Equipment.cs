using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Equipment : PickupInteractable
{
    
    [Header("Equipment General")]
    //public PlayerInventory.InventorySlot inventorySlot;
    public bool onCooldown = false;
    public Player equippedPlayer;
    [SerializeField] protected Animator animator;

    public abstract void PerformAbility();
    public abstract void SetAnimations();
    public abstract void ResetAbility();
}
