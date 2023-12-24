using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fountain : MonoBehaviour
{
    private float manaTimer = 0f;
    private float manaRegenSpeed = 1f;

    private float requiredDistance = 7f;

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Magnitude(Player.LocalInstance.transform.position - transform.position);
        if(distanceToPlayer <= requiredDistance)
        {
            manaTimer += Time.deltaTime;
            if(manaTimer >= manaRegenSpeed)
            {
                Player.LocalInstance.playerMana.CurrentMana += 1;
                manaTimer = 0f;
            }
        }else
        {
            manaTimer = 0f;
        }
    }
}
