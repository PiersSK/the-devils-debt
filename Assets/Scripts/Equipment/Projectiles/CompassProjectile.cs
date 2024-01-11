using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassProjectile : BaseProjectile
{
    public Compass sourceCompass;

    protected override void OnTriggerEnter(Collider other)
    {
        if (hasBeenfired)
        {
            if (other.transform.GetComponentInParent<Door>() != null)
            {
                sourceCompass.orbIsActive = false;
                DestroyProjectileServerRpc();
            } else
            {
                Debug.Log("compass orb hit something else: " + other.name);
            }
        }
    }
}
