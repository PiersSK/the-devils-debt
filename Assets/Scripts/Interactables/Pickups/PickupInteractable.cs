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

    public Vector3 rootPosition;

    [SerializeField] private bool isPickedUp = false;

    private void Awake()
    {
        rootPosition = transform.position;
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

            Vector3 currentPos = new Vector3(rootPosition.x, rootPosition.y + Mathf.Sin(currentSpin) / 8, rootPosition.z);
            transform.position = currentPos;
        }
    }

    public void UpdateRootPosition(Vector3 newPos)
    {
        rootPosition = newPos;
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

        PlayerInventory inventory = Player.LocalInstance.GetComponent<PlayerInventory>();
        inventory.PickupItemServerRpc(gameObject);
    }

}
