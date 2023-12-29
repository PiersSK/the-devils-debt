using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    public enum InventorySlot
    {
        MainHand,
        OffHand,
        Accessory
    }

    public InventorySlot currentEquipped = InventorySlot.MainHand;

    // Assumes index of equipment always set by InventorySlot enum
    [SerializeField] private List<Equipment> equipment = new List<Equipment>(3) { null, null, null };
    private List<HotbarIcon> hotbarIcons = new List<HotbarIcon>(3) { null, null, null };

    [SerializeField] private Vector3 handPos;

    private PlayerInput.OnFootActions input;
    private UIManager ui;

    private void Start()
    {
        // Get other class references
        ui = UIManager.Instance;
        input = GetComponent<InputManager>().onFoot;

        hotbarIcons[(int)InventorySlot.MainHand] = ui.hotbarMain;
        hotbarIcons[(int)InventorySlot.OffHand] = ui.hotbarOff;
        hotbarIcons[(int)InventorySlot.Accessory] = ui.hotbarAccessory;

        // Initial setup of equipment
        UpdateHotbarSprites();
        SetEquipmentToHands();
        FollowCameraRotation();
        EquipItemServerRpc(currentEquipped);
    }

    private void Update()
    {
        if (!IsOwner) return;

        FollowCameraRotation();

        // Deal with switching equipment
        if (input.EquipMain.IsPressed()) EquipItemServerRpc(InventorySlot.MainHand);
        if (input.EquipOff.IsPressed()) EquipItemServerRpc(InventorySlot.OffHand);
        if (input.EquipAccessory.IsPressed()) EquipItemServerRpc(InventorySlot.Accessory);

        // Check for equipment use
        if (input.Attack.IsPressed())
            equipment[(int)currentEquipped].PerformAbility();
        else
            equipment[(int)currentEquipped].ResetAbility();

        equipment[(int)currentEquipped].SetAnimations();
    }

    private void UpdateHotbarSprites()
    {
        foreach (Equipment item in equipment)
        {
            if (item.objectSprite != null)
            {
                hotbarIcons[(int)item.inventorySlot].UpdateSprite(item.objectSprite.sprite);
            }
        }
    }

    private void SetEquipmentToHands()
    {
        foreach (Equipment item in equipment)
            item.transform.localPosition = handPos;
    }

    private void FollowCameraRotation()
    {
        foreach (Equipment item in equipment)
            item.transform.rotation = GetComponent<PlayerLook>().cam.transform.rotation;
    }

    public void PickupItem(GameObject itemObj)
    {
        Equipment pickup = itemObj.GetComponent<Equipment>();
        Transform itemToDrop = equipment[(int)pickup.inventorySlot].transform;

        // Pick up new item
        equipment[(int)pickup.inventorySlot] = pickup.GetComponent<Equipment>();
        pickup.ToggleIsPickedUp();

        // Drop item in that slot
        if (itemToDrop != null)
        {
            itemToDrop.parent = null;
            itemToDrop.GetComponent<Equipment>().UpdateRootPosition(itemObj.transform.position);
            itemToDrop.GetComponent<Equipment>().ToggleIsPickedUp();
        }

        // Put new item in hand
        itemObj.transform.parent = transform;

        // Update equipment positions
        SetEquipmentToHands();
        FollowCameraRotation();
        UpdateHotbarSprites();

        //Refresh equipment
        EquipItemClientRpc(currentEquipped);
    }

    [ServerRpc(RequireOwnership = false)]
    private void EquipItemServerRpc(InventorySlot slot)
    {
        EquipItemClientRpc(slot);
    }

    [ClientRpc]
    private void EquipItemClientRpc(InventorySlot slot)
    {
        ResetEquipmentActiveStates();

        currentEquipped = slot;
        hotbarIcons[(int)slot].SetEquipped(true);
        hotbarIcons[(int)slot].itemInSlot = equipment[(int)slot];
        equipment[(int)slot].equippedPlayer = Player.LocalInstance;

        equipment[(int)slot].gameObject.SetActive(true);
    }

    private void ResetEquipmentActiveStates()
    {
        foreach (Equipment item in equipment)
        {
            item.gameObject.SetActive(false);
            item.equippedPlayer = Player.LocalInstance;
            hotbarIcons[(int)item.inventorySlot].SetEquipped(false);
        }
    }
}
