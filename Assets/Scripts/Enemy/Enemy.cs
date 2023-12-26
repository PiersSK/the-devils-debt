using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class Enemy : NetworkBehaviour
{
    private EnemyStateMachine stateMachine;
    [SerializeField] private string currentState; //For debugging 

    private NavMeshAgent agent;
    public NavMeshAgent Agent { get => agent; }

    public Path path;

    public float sightDistance = 15f;
    public float sightFOV = 65f;


    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Image healthBar;
    private NetworkVariable<int> currentHealth = new(3);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsServer) currentHealth.Value = maxHealth;
        currentHealth.OnValueChanged += UpdateCurrentHealth;
    }

    private void Start()
    {
        stateMachine = GetComponent<EnemyStateMachine>();
        agent = GetComponent<NavMeshAgent>();
        stateMachine.Initialise();
    }

    private void Update()
    {
        CanSeePlayer();
        currentState = stateMachine.activeState.ToString();
    }

    private void UpdateCurrentHealth(int prevVal, int newVal) {
        currentHealth.Value = newVal;
        float percHealth = (float)newVal / maxHealth;
        healthBar.fillAmount = percHealth;
        healthBar.color = new Color(healthBar.color.r, percHealth, healthBar.color.b);
        if (currentHealth.Value <= 0) DeathServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DamageToEnemyServerRpc(int damage)
    {
        currentHealth.Value = Math.Clamp(currentHealth.Value - damage, 0, maxHealth);
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeathServerRpc()
    {
        if (ObjectiveController.Instance.objectiveSelected.Value == ObjectiveController.ObjectiveType.Monsters)
            ObjectiveController.Instance.ProgressObjective();

        Destroy(gameObject);
        NetworkObject.Despawn();
    }

    public bool CanSeePlayer()
    {
        //Find all players
        Player[] players = FindObjectsOfType<Player>();
        foreach(Player player in players)
        {
            //If close enough to be seen
            if(Vector3.Distance(transform.position, player.transform.position) < sightDistance)
            {
                //If in FOV
                Vector3 targetDirection = player.transform.position - transform.position;
                float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);
                if (angleToPlayer >= -sightFOV && angleToPlayer <= sightFOV)
                {
                    //If view is unobstructed
                    Ray ray = new(transform.position, targetDirection);
                    Debug.DrawRay(ray.origin, ray.direction * sightDistance);
                    RaycastHit hitInfo = new();
                    if(Physics.Raycast(ray, out hitInfo, sightDistance))
                    {
                        if(hitInfo.transform == player.transform)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
}
