using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class Enemy : NetworkBehaviour
{
    public event EventHandler<OnAttackedArgs> OnAttacked;
    public class OnAttackedArgs : EventArgs
    {
        public Player attacker;

        public OnAttackedArgs(Player p)
        {
            attacker = p;
        }
    }

    private EnemyStateMachine stateMachine;
    [SerializeField] private string currentState; //For debugging 

    private NavMeshAgent agent;
    public NavMeshAgent Agent { get => agent; }

    public Path path;

    public float sightDistance = 15f;
    public float sightFOV = 65f;

    public LayerMask sightLayer;

    public bool isMoving = false;

    private string currentAnimationState;
    public const string WALK = "EnemyWalk";
    public const string IDLE = "Idle";
    public const string ATTACKPREP = "EnemySwordBack";
    public const string ATTACK = "EnemySwordSwing";

    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Image healthBar;
    private NetworkVariable<int> currentHealth = new(3);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsServer) currentHealth.Value = maxHealth;
        currentHealth.OnValueChanged += UpdateCurrentHealth;
        OnAttacked += ChangeToAttackState;
    }

    private void ChangeToAttackState(object caller, OnAttackedArgs e)
    {
        transform.LookAt(e.attacker.transform);
        ChangeToAttackStateServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeToAttackStateServerRpc()
    {
        stateMachine.ChangeState(stateMachine.attackState);
    }

    private void Start()
    {
        stateMachine = GetComponent<EnemyStateMachine>();
        agent = GetComponent<NavMeshAgent>();
        stateMachine.Initialise();
    }

    private void Update()
    {
        //if (!IsOwner) return;

        CanSeePlayer();
        currentState = stateMachine.activeState.ToString();
        isMoving = Agent.velocity != Vector3.zero;
    }

    private void UpdateCurrentHealth(int prevVal, int newVal) {
        currentHealth.Value = newVal;
        float percHealth = (float)newVal / maxHealth;
        healthBar.fillAmount = percHealth;
        healthBar.color = new Color(healthBar.color.r, percHealth, healthBar.color.b);
        if (currentHealth.Value <= 0) DeathServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DamageToEnemyServerRpc(int damage, NetworkObjectReference damageSourceNOR)
    {
        currentHealth.Value = Math.Clamp(currentHealth.Value - damage, 0, maxHealth);
        damageSourceNOR.TryGet(out NetworkObject damageSourceNO);

        //Vector3 knockbackForce = (damageSourceNO.transform.position - transform.position).normalized * 20f;
        //GetComponent<Rigidbody>().AddForce(knockbackForce);


        if (damageSourceNO.GetComponent<Player>() != null)
            OnAttacked?.Invoke(this, new OnAttackedArgs(damageSourceNO.GetComponent<Player>()));
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeathServerRpc()
    {
        if (ObjectiveController.Instance.objectiveSelected.Value == ObjectiveController.ObjectiveType.Monsters)
            ObjectiveController.Instance.ProgressObjective();

        //Health Orb spawn
        SpawnPickup("Pickups/HealthOrb", 7);

        //Mana Orb spawn
        SpawnPickup("Pickups/ManaOrb", 3);

        Destroy(gameObject);
        NetworkObject.Despawn();
    }

    private void SpawnPickup(string prefabName, int spawnChance)
    {
        //Health Orb spawn
        if (UnityEngine.Random.Range(0, 10) < spawnChance)
        {
            Transform healthOrb = Resources.Load<Transform>(prefabName);
            healthOrb.position = new Vector3(
                transform.position.x + UnityEngine.Random.Range(-1f, 1f)
                , -1.6f
                , transform.position.z + UnityEngine.Random.Range(-1f, 1f)
            );
            Transform healthOrbObj = Instantiate(healthOrb);
            healthOrbObj.GetComponent<NetworkObject>().Spawn(true);
        }
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
                    if(Physics.Raycast(ray.origin, ray.direction, out hitInfo, sightDistance, sightLayer))
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

    public void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;
        currentAnimationState = newState;
        GetComponent<Animator>().CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }
}
