using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Sword : Equipment
{

    [Header("Sword Stats")]
    //Sword stats
    [SerializeField] private float staminaCost = 0.5f;
    [SerializeField] private float attackDelay = 0.4f; // Time between clicking and actual attack
    [SerializeField] private float attackSpeed = 1f; // Time until you can attack again
    [SerializeField] private float attackDistance = 3f; // Length of raycast
    [SerializeField] private int attackDamage = 1; // Damage dealt on hit


    //Hittable layers
    [SerializeField] private LayerMask attackLayer;

    //Animation fields
    //[Header("Animations")]
    //[SerializeField] private Animator animator;
    private string currentAnimationState; // To ensure we don't reset to the same state
    private const string IDLE = "Idle";
    private const string WALK = "Walk";
    private const string ATTACK1 = "Attack 1";
    private const string ATTACK2 = "Attack 2";

    [Header("Audio Clips")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swordSwing;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip hitmarkerSound;
    private AudioClip queuedSound; //Sound that should be played if triggered

    //Attack status trackers
    private bool readyToAttack = true;
    private bool attacking = false;
    private int attackCount = 0; //Tracks if it's a foreswing or backswing

    protected override void Start()
    {
        base.Start();
        readyToAttack = true;
        gameObject.SetActive(true);
    }

    public override void PerformAbility()
    {
        if (!readyToAttack || attacking || Player.LocalInstance.playerMotor.currentStamina < staminaCost) return;

        Player.LocalInstance.playerMotor.currentStamina -= staminaCost;

        readyToAttack = false;
        attacking = true;

        //Invokes will perform methods after set delay
        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

        //PlaySwingSoundServerRpc();
        queuedSound = swordSwing;
        PlaySoundForAllServerRpc(Random.Range(0.9f, 1.1f));

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

    public override void SetAnimations()
    {
        if (!attacking)
        {
            if (Player.LocalInstance.playerMotor.isMoving)
                ChangeAnimationState(WALK);
            else
                ChangeAnimationState(IDLE);
        }
    }

    public override void ResetAbility()
    {
        attackCount = 0;
    }

    private void ResetAttack()
    {
        readyToAttack = true;
        attacking = false;
    }

    private void AttackRaycast()
    {
        Transform playerEyes = Player.LocalInstance.playerLook.cam.transform;
        if (Physics.Raycast(playerEyes.position, playerEyes.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            if (hit.transform.TryGetComponent(out Enemy enemy))
            {
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(hitmarkerSound);
                UIManager.Instance.ShowHitmarker();
                enemy.DamageToEnemyServerRpc(attackDamage, Player.LocalInstance.GetComponent<NetworkObject>());
            }
            else
            {
                Debug.Log("Not enemy, was : " + hit.transform.name);
                queuedSound = hitSound;
                PlaySoundForAllServerRpc(1f);
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void PlaySoundForAllServerRpc(float pitch)
    {
        PlaySoundForAllClientRpc(pitch);
    }

    [ClientRpc]
    private void PlaySoundForAllClientRpc(float pitch)
    {
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(queuedSound);
    }



    private void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.1f);
    }
}
