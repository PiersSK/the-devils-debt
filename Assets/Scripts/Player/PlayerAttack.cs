using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAttack : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    private AudioSource audioSource;
    private Camera cam;

    [SerializeField] private float attackDistance = 3f;
    [SerializeField] private float attackDelay = 0.4f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask attackLayer;

    [SerializeField] private AudioClip swordSwing;
    [SerializeField] private AudioClip hitSound;

    private bool attacking = false;
    private bool readyToAttack = true;
    private int attackCount;

    private string currentAnimationState;
    private const string IDLE = "Idle";
    private const string WALK = "Walk";
    private const string ATTACK1 = "Attack 1";
    private const string ATTACK2 = "Attack 2";

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        cam = GetComponent<PlayerLook>().cam;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (GetComponent<InputManager>().onFoot.Attack.IsPressed()) Attack();
        else attackCount = 0;

        SetAnimations();
    }

    public void Attack()
    {
        if (!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        //Invokes will perform methods after set delay
        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

        PlaySwingSoundServerRpc();

        if (attackCount == 0)
        {
            ChangeAnimationState(ATTACK1);
            attackCount++;
        }
        else
        {
            ChangeAnimationState(ATTACK2);
            attackCount = 0;
        }
    }

    private void ResetAttack()
    {
        readyToAttack = true;
        attacking = false;
    }

    private void AttackRaycast()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            HitTarget(hit.point);
            if(hit.transform.TryGetComponent(out Enemy enemy))
            {
                enemy.DamageToEnemyServerRpc(attackDamage, NetworkObject);
            }
        }
    }

    private void SetAnimations()
    {
        if(!attacking)
        {
            if (GetComponent<PlayerMotor>().isMoving)
                ChangeAnimationState(WALK);
            else
                ChangeAnimationState(IDLE);
        }
    }

    private void HitTarget(Vector3 pos)
    {
        PlayHitSoundServerRpc();
    }

    private void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }

    [ServerRpc]
    private void PlaySwingSoundServerRpc()
    {
        PlaySwingSoundClientRpc();
    }

    [ClientRpc]
    private void PlaySwingSoundClientRpc()
    {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(swordSwing);
    }

    [ServerRpc]
    private void PlayHitSoundServerRpc()
    {
        PlayHitSoundClientRpc();
    }

    [ClientRpc]
    private void PlayHitSoundClientRpc()
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(hitSound);
    }

    //[SerializeField] private float swingLength = 0.5f;

    //private bool isSwinging = false;
    //private float swingTimer = 0f;

    //private void Update()
    //{
    //    if (isSwinging)
    //    {
    //        swingTimer += Time.deltaTime;
    //        if (swingTimer > swingLength)
    //        {
    //            isSwinging = false;
    //            swingTimer = 0f;
    //            Debug.Log("Stopping Swinging Sword");
    //            animator.SetBool("Swinging", false);
    //        }
    //    }
    //}

    //public void SwingSword()
    //{
    //    if (!IsOwner) return;
    //    if (isSwinging) return;

    //    isSwinging = true;
    //    Debug.Log("Swinging Sword");
    //    animator.SetBool("Swinging", true);
    //}
}
