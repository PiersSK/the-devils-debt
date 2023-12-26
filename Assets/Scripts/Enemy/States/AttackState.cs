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

    public override void Enter()
    {
        enemy.Agent.stoppingDistance = 2f;
    }

    public override void Exit()
    {
    }

    public override void Perform()
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

        // If can see a player, move towards the closest
        if(enemy.CanSeePlayer())
        {
            losePlayerTimer = 0f;
            enemy.Agent.SetDestination(GetClosestPlayer().position);
        }
        else
        {
            losePlayerTimer += Time.deltaTime;
            if(losePlayerTimer >= 3f)
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
}
