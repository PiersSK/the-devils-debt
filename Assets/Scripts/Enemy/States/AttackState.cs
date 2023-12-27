using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackState : BaseState
{
    private Transform closestPlayer;

    private float alertRadius = 5f;
    //private float lookSpeed = 3f;

    private float trackPlayerTimer;
    private float losePlayerTimer;

    private float attackRange = 3f;
    private int attackDamage = 2;

    private float attackTriggerRange = 2.5f;

    private float chargeTimeMax = 1f;
    private float chargeTimeMin = 0.4f;
    private float chargeTime = 0.5f;

    private float attackTime = 0.5f;
    private float chargeTimer = 0f;
    private bool isAttacking = false;
    private bool chargeComplete = false;
    private bool attackComplete = false;
    private Transform attackTarget;

    public override void Enter()
    {
        enemy.Agent.stoppingDistance = 2f;
    }

    public override void Exit()
    {
    }

    public override void Perform()
    {
        SetAnimations();

        if (!isAttacking)
        {
            LookAtClosestPlayerInRange();
            MoveToClosestPlayer();
            ShouldStartAttack();
        }
        else
        {
            Attack();
        }


    }

    private void LookAtClosestPlayerInRange()
    {
        closestPlayer = GetClosestPlayer();
        // If closest player is very close it will constantly rotate to look at them
        if (Vector3.Distance(enemy.transform.position, closestPlayer.position) <= alertRadius)
            trackPlayerTimer = 3f;
        else
            trackPlayerTimer -= Time.deltaTime;

        if (trackPlayerTimer >= 0f)
        {
            enemy.transform.LookAt(closestPlayer); // Should change to be gradual
        }

    }

    private void MoveToClosestPlayer()
    {
        // If can see a player, move towards the closest
        if (enemy.CanSeePlayer())
        {
            losePlayerTimer = 0f;
            enemy.Agent.SetDestination(GetClosestPlayer().position);
        }
        else
        {
            losePlayerTimer += Time.deltaTime;
            if (losePlayerTimer >= 3f)
            {
                //Change to search state
                stateMachine.ChangeState(new PatrolState());
            }
        }
    }

    private Transform GetClosestPlayer()
    {
        List<float> distances = new();
        Player[] players = GameObject.FindObjectsOfType<Player>();
        foreach (Player player in players)
        {
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, player.transform.position);
            distances.Add(distanceToPlayer);
        }

        return players[distances.IndexOf(distances.Min())].transform;
    }

    private void ShouldStartAttack()
    {
        Transform currentClosestPlayer = GetClosestPlayer();
        if (Vector3.Distance(enemy.transform.position, currentClosestPlayer.position) <= attackTriggerRange) {
            isAttacking = true;
            attackComplete = false;
            chargeComplete = false;
            chargeTimer = 0f;
            chargeTime = Random.Range(chargeTimeMin, chargeTimeMax);
            attackTarget = currentClosestPlayer;
            enemy.ChangeAnimationState(Enemy.ATTACKPREP);
        }
    }

    private void Attack()
    {
        chargeTimer += Time.deltaTime;
        enemy.transform.LookAt(attackTarget);
        if (chargeTimer >= chargeTime) chargeComplete = true;

        if(chargeComplete && !attackComplete)
        {
            Debug.Log("Starting attack");
            enemy.ChangeAnimationState(Enemy.ATTACK);

            Ray ray = new(enemy.transform.position, enemy.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, attackRange))
            {
                Debug.Log(hit.transform);
                if (hit.transform.TryGetComponent(out Player player))
                {
                    Debug.Log("hit player");
                    player.GetComponent<PlayerHealth>().IncrementPlayerHealthServerRpc(-attackDamage);
                }
            }
            attackComplete = true;
        }

        if (chargeTimer >= (chargeTime + attackTime))
            isAttacking = false;
    }

    private void SetAnimations()
    {
        if (!isAttacking)
        {
            if (enemy.Agent.velocity != Vector3.zero)
                enemy.ChangeAnimationState(Enemy.WALK);
            else
                enemy.ChangeAnimationState(Enemy.IDLE);
        }
    }
}
