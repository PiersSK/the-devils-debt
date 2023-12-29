using UnityEngine;
using UnityEngine.UI;

public class PickupInteractable : NetworkInteractable
{
    // TODO: Combine with Equipment class????


    public PlayerInventory.InventorySlot inventorySlot;
    [SerializeField] private GameObject spriteObject;
    [SerializeField] private GameObject equipmentObject;
    private Collider interactCollider;

    public Image objectSprite;
    private float spinSpeed = 20f;
    private float currentSpin = 0f;
    private Vector3 startPos;

    [SerializeField] private bool isPickedUp = false;

    private void Awake()
    {
        startPos = transform.position;
        interactCollider = GetComponent<Collider>();

        spriteObject.SetActive(!isPickedUp);
        equipmentObject.SetActive(isPickedUp);
        interactCollider.enabled = !isPickedUp;
    }


    private void Update()
    {
        if (!isPickedUp)
        {
            currentSpin += Time.deltaTime;
            if (currentSpin > Mathf.PI * 2) currentSpin = 0;

            transform.Rotate(Vector3.up * (Time.deltaTime * spinSpeed));

            Vector3 currentPos = new Vector3(startPos.x, startPos.y + Mathf.Sin(currentSpin) / 8, startPos.z);
            transform.position = currentPos;
        }
    }

    public void UpdateRootPosition(Vector3 newPos)
    {
        startPos = newPos;
        transform.position = newPos;
    }

    public void ToggleIsPickedUp()
    {
        Debug.Log(IsSpawned);
        isPickedUp = !isPickedUp;

        if (!isPickedUp) gameObject.SetActive(true);

        spriteObject.SetActive(!isPickedUp);
        equipmentObject.SetActive(isPickedUp);
        interactCollider.enabled = !isPickedUp;
    }

    protected override void Interact()
    {
        if (isPickedUp) return;
        //Spawn actual item
        //Put in player hand
        //Equip
        PlayerInventory inventory = Player.LocalInstance.GetComponent<PlayerInventory>();
        inventory.PickupItem(gameObject);
        //Drop current equipped
        //Sawn old equipped interactable
    }

}
