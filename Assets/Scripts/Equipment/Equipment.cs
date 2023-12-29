using System.Collections;
using System.Collections.Generic;
using Mono.CSharp;
using Unity.Netcode;
using UnityEngine;

public abstract class Equipment : PickupInteractable
{
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary,
        Epic
    }

    private List<Color> rarityColours = new List<Color>()
    {
        Color.white,
        Color.green,
        Color.blue,
        Color.magenta,
        Color.yellow
    };
    
    [Header("Equipment General")]
    //public PlayerInventory.InventorySlot inventorySlot;
    public bool onCooldown = false;
    public Player equippedPlayer;
    public ItemRarity rarity = ItemRarity.Common;
    [SerializeField] protected Animator animator;

    public abstract void PerformAbility();
    public abstract void SetAnimations();
    public abstract void ResetAbility();

    protected virtual void Start()
    {
        SetSpriteColour();
    }

    public void SetSpriteColour()
    {
        objectSprite.color = rarityColours[(int)rarity];
    }
}
