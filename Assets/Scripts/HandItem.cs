using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandItem : MonoBehaviour
{
    public Transform player;
    public Transform cam;
    private Vector3 initPos;
    private Quaternion initRot;

    private void Start()
    {
        initPos = transform.position;
        initRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = player.position;
        transform.rotation = cam.rotation;
    }
}
