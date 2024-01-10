using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyStateMachine : NetworkBehaviour
{
    public BaseState activeState;
    public PatrolState patrolState;
    public AttackState attackState;

    public void Initialise()
    {
        patrolState = new();
        attackState = new();
        ChangeState(patrolState); 
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("ESM IsOwner: " + IsOwner);
        if(activeState != null)
        {
            activeState.Perform();
        }
    }

    public void ChangeState(BaseState newState)
    {
        //check activestate != null
        if (activeState != null)
        {
            //run activestate cleanup
            activeState.Exit();
        }

        activeState = newState;

        if (activeState != null)
        {
            activeState.stateMachine = this;
            activeState.enemy = GetComponent<Enemy>();
            activeState.Enter();
        }

    }
}
