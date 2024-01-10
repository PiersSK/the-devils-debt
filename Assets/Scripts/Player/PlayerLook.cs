using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerLook : NetworkBehaviour
{
    public Camera cam;
    private float xRotation = 0f;

    public float xSensitivity = 0.3f;
    public float ySensitivity = 0.3f;

    private Vector3 rootPosition;

    public bool cameraShake = false;
    public float cameraShakeMagnitude = 0f;


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        cam.transform.parent.gameObject.SetActive(true);
        rootPosition = cam.transform.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ProcessLook(Vector2 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * (mouseX) * xSensitivity);

        if(cameraShake)
        {
            Vector3 offset = Random.insideUnitSphere * cameraShakeMagnitude;
            cam.transform.localPosition = rootPosition + offset;
            Player.LocalInstance.playerInventory.OffsetEquipmentInHand(offset * 0.9f);
        } else
        {
            cam.transform.localPosition = rootPosition;
        }
    }

    public Vector3 GetLookDirection()
    {
        return cam.transform.forward;
    }
}
