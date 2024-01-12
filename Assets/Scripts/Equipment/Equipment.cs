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
    
    public bool onCooldown = false;

    [Header("Equipment General")]
    public Player equippedPlayer;
    public ItemRarity rarity = ItemRarity.Common;

    [SerializeField] protected Animator animator;

    public abstract void PerformAbility();
    public abstract void SetAnimations();
    public abstract void ResetAbility();

    public virtual void PerformAlt() { }

    public virtual void UpdateAltUI()
    {
        UIManager.Instance.equipmentPrompt.gameObject.SetActive(false);
        UIManager.Instance.equipmentDetails.gameObject.SetActive(false);
    }

    protected virtual void Start()
    {
        SetSpriteColour();
    }

    public void SetSpriteColour()
    {
        objectSprite.color = rarityColours[(int)rarity];
    }
}
