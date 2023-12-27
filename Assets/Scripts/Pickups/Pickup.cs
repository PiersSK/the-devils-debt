using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    private float magnetDistance = 5f;

    private void Update()
    {
        Transform target = FindMagneticTarget();
        if(target != null && CanPickUp(target))
        {
            Vector3 direction = (target.position - transform.position).normalized;
            float speed = magnetDistance - Vector3.Distance(target.position, transform.position);

            GetComponent<Rigidbody>().velocity = direction * speed;
        } else
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log(other + " trigger pickup");
    }

    protected virtual bool CanPickUp(Transform player)
    {
        return true;
    }

    private Transform FindMagneticTarget()
    {
        List<float> distances = new();
        List<Player> validPlayers = new();
        Player[] players = GameObject.FindObjectsOfType<Player>();
        foreach (Player player in players)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= magnetDistance)
            {
                distances.Add(distanceToPlayer);
                validPlayers.Add(player);
            }
        }

        if (distances.Count > 0)
            return validPlayers[distances.IndexOf(distances.Min())].transform;
        else
            return null;
    }
}
