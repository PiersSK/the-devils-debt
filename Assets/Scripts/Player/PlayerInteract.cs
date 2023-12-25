using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInteract : NetworkBehaviour
{
    private Camera cam;
    [SerializeField]
    private float distance = 3f;
    [SerializeField]
    private LayerMask mask;
    private InputManager inputManager;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<PlayerLook>().cam;
        inputManager = GetComponent<InputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * distance);

        RaycastHit hitInfo;
        if(Physics.Raycast(ray, out hitInfo, distance, mask))
        {
            if(hitInfo.collider.GetComponent<IInteractable>() != null)
            {
                IInteractable interactable = hitInfo.collider.GetComponent<IInteractable>();
                UpdateText(interactable.GetPromptMessage(), interactable.CanInteract());
                if (inputManager.onFoot.Interact.triggered)
                {
                    interactable.BaseInteract();
                }
            }
        }
        else
        {
            UpdateText(string.Empty, true);
        }
    }

    private void UpdateText(string promptMessage, bool canInteract)
    {
        if (promptMessage != string.Empty)
        {
            UIManager.Instance.playerUI_promptText.text = "[E] " + promptMessage;
            UIManager.Instance.playerUI_promptText.color = canInteract ? Color.white : Color.red;
        }
        else
        {
            UIManager.Instance.playerUI_promptText.text = string.Empty;
        }
    }
}
