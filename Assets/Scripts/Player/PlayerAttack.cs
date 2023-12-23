using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAttack : NetworkBehaviour
{
    [SerializeField] private float swingLength = 0.5f;
    [SerializeField] private Animator animator;

    private bool isSwinging = false;
    private float swingTimer = 0f;

    private void Update()
    {
        if (isSwinging)
        {
            swingTimer += Time.deltaTime;
            if (swingTimer > swingLength)
            {
                isSwinging = false;
                swingTimer = 0f;
                Debug.Log("Stopping Swinging Sword");
                animator.SetBool("Swinging", false);
            }
        }
    }

    public void SwingSword()
    {
        if (!IsOwner) return;
        if (isSwinging) return;

        isSwinging = true;
        Debug.Log("Swinging Sword");
        animator.SetBool("Swinging", true);
    }
}
