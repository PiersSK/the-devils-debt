using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : Interactable
{
    public Light roomLight;
    private bool lightIsOn = true;
    [SerializeField] private Transform switchPart;
    protected override void Interact()
    {
        lightIsOn = !lightIsOn;
        roomLight.intensity = lightIsOn ? 10f : 0;

        Quaternion switchPartRotation = switchPart.rotation;
        switchPartRotation.z = lightIsOn ? -45f : -135f;
        switchPart.rotation = switchPartRotation;
    }
}
