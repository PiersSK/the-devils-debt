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

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        cam.gameObject.transform.parent.gameObject.SetActive(true);
        base.OnNetworkSpawn();

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
    }
}
