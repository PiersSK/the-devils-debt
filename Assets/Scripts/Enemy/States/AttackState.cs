using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackState : BaseState
{
    private float losePlayerTimer;

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void Perform()
    {
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
