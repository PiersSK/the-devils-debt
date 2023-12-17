using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float swingLength = 0.25f;
    [SerializeField] private Animator animator;

    private bool isSwinging = false;
    private float swingTimer = 0f;

    private void Update()
    {
        swingTimer += Time.deltaTime;
        if(swingTimer > swingLength)
        {
            isSwinging = false;
            animator.SetBool("Swinging", false);
        }
    }
    public void SwingSword()
    {
        if(isSwinging) return;
        isSwinging = true;
        animator.SetTrigger("Swing");
    }
}
