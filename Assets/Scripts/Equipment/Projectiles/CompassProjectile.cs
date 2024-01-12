using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassProjectile : BaseProjectile
{
    public Transform target;
    private float orbSpeed = 8f;

    private void Update()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.GetComponent<Rigidbody>().velocity = direction * orbSpeed;

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
            Destroy(transform.gameObject);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.transform.GetComponentInParent<Door>() != null)
            DestroyProjectileServerRpc();
    }
}
