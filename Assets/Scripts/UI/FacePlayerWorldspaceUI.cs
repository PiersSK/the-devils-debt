using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacePlayerWorldspaceUI : MonoBehaviour
{
    
    void Update()
    {
        if (PlayerMotor.LocalInstance != null)
        {
            Vector3 playerPos = PlayerMotor.LocalInstance.transform.position;
            Vector3 player2D = new(playerPos.x, transform.position.y, playerPos.z);
            transform.LookAt(player2D);
            transform.RotateAround(transform.position, transform.up, 180f);
        }
    }
}
