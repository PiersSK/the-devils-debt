using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackState : BaseState
{
    private Transform closestPlayer;

    private float alertRadius = 5f;
    private float lookSpeed = 3f;

    private float trackPlayerTimer;
    private float losePlayerTimer;

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void Perform()
    {
        closestPlayer = GetClosestPlayer();
        Debug.Log("Closest player is " + Vector3.Distance(enemy.transform.position, closestPlayer.position) + " away");

        // If closest player is very close it will constantly rotate to look at them
        if (Vector3.Distance(enemy.transform.position, closestPlayer.position) <= alertRadius)
            trackPlayerTimer = 3f;
        else
            trackPlayerTimer -= Time.deltaTime;

        if (trackPlayerTimer >= 0f)
        {
            Debug.Log("turning to look at player");
            //Vector3 direction = closestPlayer.position - enemy.transform.position;

            enemy.transform.LookAt(closestPlayer);

            //Quaternion toRotation = Quaternion.LookRotation(enemy.transform.forward, direction);
            //enemy.transform.rotation = Quaternion.Lerp(enemy.transform.rotation, toRotation, lookSpeed * Time.deltaTime);
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
