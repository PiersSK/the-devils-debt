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

    private Vector3 lerpToPosition;
    private bool shakeLerpOut = false;
    private bool shakeLerpIn = false;

    private float cameraShakeLerpTimer = 0f;
    private float cameraShakeLerpTime = 0.1f;

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
