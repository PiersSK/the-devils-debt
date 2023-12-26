using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : BaseState
{
    public int waypointIndex;
    private float waitTimer = 0f;
    private float timeToWait = 3f;

    public override void Enter()
    {
        enemy.Agent.stoppingDistance = 0f;
    }

    public override void Exit()
    {
    }

    public override void Perform()
    {
        PatrolCycle();
        if (enemy.CanSeePlayer()) stateMachine.ChangeState(new AttackState());
    }

    public void PatrolCycle()
    {
        if(enemy.Agent.remainingDistance < 0.2f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer > timeToWait)
            {
                if (waypointIndex < enemy.path.waypoints.Count - 1)
                    waypointIndex++;
                else
                    waypointIndex = 0;

                enemy.Agent.SetDestination(enemy.path.waypoints[waypointIndex].position);
                waitTimer = 0f;
            }

        }
    }
}
