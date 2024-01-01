using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("UI Settings")]
    [SerializeField] private Sprite defaultMainSprite;
    [SerializeField] private Sprite defaultOffSprite;
    [SerializeField] private Sprite defaultAccessorySprite;

    [Header("Player Settings")]
    [SerializeField] private Vector3 handPos;

    [Header("Starting Equipment")]
    [SerializeField] private string startingMain;
    [SerializeField] private string startingOff;
    [SerializeField] private string startingAccessory;

    [Header("Current Equipment")]
    public InventorySlot currentEquipped = InventorySlot.MainHand;

    // Assumes index of equipment always set by InventorySlot enum
    private List<Equipment> equipment;
    private List<HotbarIcon> hotbarIcons;


    private PlayerInput.OnFootActions input;
    private UIManager ui;

    private void Start()
    {
        equipment = new List<Equipment>() { null, null, null };
        hotbarIcons = new List<HotbarIcon>() { null, null, null };

        // Get other class references
        ui = UIManager.Instance;
        input = GetComponent<InputManager>().onFoot;

        hotbarIcons[(int)InventorySlot.MainHand] = ui.hotbarMain;
        hotbarIcons[(int)InventorySlot.OffHand] = ui.hotbarOff;
        hotbarIcons[(int)InventorySlot.Accessory] = ui.hotbarAccessory;

        // Initial setup of equipment
        RefreshEquipmentState();
    }

    private void Update()
    {
        FollowCameraRotation();

        if (!IsOwner || equipment[(int)currentEquipped] == null) return;

        // Check for equipment use
        if (input.Attack.IsPressed())
            equipment[(int)currentEquipped].PerformAbility();
        else
            equipment[(int)currentEquipped].ResetAbility();

        equipment[(int)currentEquipped].SetAnimations();
    }

    private void RefreshEquipmentState()
    {
        // Initial setup of equipment
        Debug.Log("RefestEquipmentState invoked");
        if(IsOwner) UpdateHotbarSprites();
        SetEquipmentToHands();
        FollowCameraRotation();
        EquipItemServerRpc(currentEquipped);
    }

    private void UpdateHotbarSprites()
    {
        foreach (Equipment item in equipment)
        {
            if (item!= null && item.objectSprite != null)
            {
                hotbarIcons[(int)item.inventorySlot].UpdateSprite(item.objectSprite.sprite);
            }
        }
    }

    private void SetEquipmentToHands()
    {
        foreach (Equipment item in equipment)
        {
            if (item != null)
            {
                item.transform.localPosition = handPos;
            }
        }
    }

    private void FollowCameraRotation()
    {
        foreach (Equipment item in equipment)
        {
            if (item != null)
            {
                item.transform.rotation = GetComponent<PlayerLook>().cam.transform.rotation;
            }
        }
    }

    private string GetSlotPrefab(InventorySlot slot)
    {
        switch(slot)
        {
            case InventorySlot.MainHand:
                return startingMain;
            case InventorySlot.OffHand:
                return startingOff;
            case InventorySlot.Accessory:
                return startingAccessory;
            default:
                return string.Empty;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnStartingGearServerRpc()
    {
        int maxSlot = (int)Enum.GetValues(typeof(InventorySlot)).Cast<InventorySlot>().Max();
        for (int i = 0; i <= maxSlot; i++)
        {
            string prefabName = GetSlotPrefab((InventorySlot)i);
            if (equipment[i] == null && prefabName != string.Empty)
            {
                Debug.Log("spawning item " + prefabName);
                GameObject itemPrefabObj = Resources.Load<GameObject>(prefabName);
                GameObject itemObj = Instantiate(itemPrefabObj.gameObject);
                NetworkObject itemNO = itemObj.GetComponent<NetworkObject>();
                itemNO.Spawn();
                PickupItemClientRpc(itemNO);

            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PickupItemServerRpc(NetworkObjectReference itemNOR)
    {
        PickupItemClientRpc(itemNOR);
    }

    [ClientRpc]
    private void PickupItemClientRpc(NetworkObjectReference itemNOR)
    {
        itemNOR.TryGet(out NetworkObject itemNO);
        PickupItem(itemNO.gameObject);
    }


    private void PickupItem(GameObject itemObj)
    {
        Equipment pickup = itemObj.GetComponent<Equipment>();

        // Drop item in that slot
        if (equipment[(int)pickup.inventorySlot] != null)
        {
            Transform itemToDrop = equipment[(int)pickup.inventorySlot].transform;
            itemToDrop.GetComponent<NetworkObject>().TryRemoveParent();
            itemToDrop.GetComponent<Equipment>().UpdateRootPosition(itemObj.GetComponent<Equipment>().rootPosition);
            itemToDrop.GetComponent<Equipment>().ToggleIsPickedUp();
        }

        // Pick up new item
        equipment[(int)pickup.inventorySlot] = pickup;
        pickup.ToggleIsPickedUp();

        // Put new item in hand
        itemObj.GetComponent<NetworkObject>().TrySetParent(transform);
        Debug.Log("Item is child of player?: " + (itemObj.transform.parent == transform));

        //TODO: Find a better solution than this for parent delay on client lol
        if (itemObj.transform.parent != transform)
            Invoke(nameof(RefreshEquipmentState), 0.1f);
        else
            RefreshEquipmentState();
    }

    [ServerRpc(RequireOwnership = false)]
    public void EquipItemServerRpc(InventorySlot slot)
    {
        EquipItemClientRpc(slot);
    }

    [ClientRpc]
    private void EquipItemClientRpc(InventorySlot slot)
    {
        ResetEquipmentActiveStates();

        currentEquipped = slot;

        if (IsOwner)
        {
            hotbarIcons[(int)slot].SetEquipped(true);
            hotbarIcons[(int)slot].itemInSlot = equipment[(int)slot];
        }

        if (equipment[(int)slot] != null)
        {
            equipment[(int)slot].equippedPlayer = Player.LocalInstance;
            equipment[(int)slot].gameObject.SetActive(true);
        }
    }

    private void ResetEquipmentActiveStates()
    {
        if(IsOwner)
            foreach (HotbarIcon icon in hotbarIcons) icon.SetEquipped(false);

        foreach (Equipment item in equipment)
        {
            if (item != null)
            {
                item.gameObject.SetActive(false);
                item.equippedPlayer = Player.LocalInstance;
            }
        }
    }
}
