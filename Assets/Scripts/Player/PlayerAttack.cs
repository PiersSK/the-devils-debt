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
    [SerializeField] private AudioClip hitmarkerSound;

    private AudioClip queuedSound;


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

        //PlaySwingSoundServerRpc();
        queuedSound = swordSwing;
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        PlaySoundForAllServerRpc();

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
            if(hit.transform.TryGetComponent(out Enemy enemy))
            {
                HitTarget(true);
                UIManager.Instance.ShowHitmarker();
                enemy.DamageToEnemyServerRpc(attackDamage, NetworkObject);
            }
            else
                HitTarget(false);
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

    private void HitTarget(bool isEnemy)
    {
        if (isEnemy)
        {
            queuedSound = hitmarkerSound;
            audioSource.pitch = 1f;
            PlaySoundLocally();
        }
        else
        {
            queuedSound = hitSound;
            audioSource.pitch = 1f;
            PlaySoundForAllServerRpc();
        }
    }

    private void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.1f);
    }

    private void PlaySoundLocally()
    {
        audioSource.PlayOneShot(queuedSound);
    }

    [ServerRpc]
    private void PlaySoundForAllServerRpc()
    {
        PlaySoundForAllClientRpc();
    }

    [ClientRpc]
    private void PlaySoundForAllClientRpc()
    {
        audioSource.PlayOneShot(queuedSound);
    }
}
