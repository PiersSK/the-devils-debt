using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    private float magnetDistance = 5f;

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log(other + " trigger pickup");
    }

    private void FindMagneticTarget()
    {
        // Search for closest player in range
    }
}
