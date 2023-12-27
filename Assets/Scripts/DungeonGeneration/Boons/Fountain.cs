using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Fountain : NetworkBehaviour
{
    private float manaTimer = 0f;
    private float manaRegenSpeed = 1f;

    private NetworkVariable<int> fountainManaRemaining = new(10);

    private float requiredDistance = 7f;

    private const string IDLE = "Idle";
    private const string SPIN = "RegenSpin";
    private const string DEACTIVATE = "Deactivate";
    private string currentAnim = IDLE;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        fountainManaRemaining.OnValueChanged += SyncRemainingMana;
    }

    private void Update()
    {
        if (fountainManaRemaining.Value > 0)
        {
            //Check to see if regen effect can be given to local player
            float distanceToPlayer = Vector3.Magnitude(Player.LocalInstance.transform.position - transform.position);
            bool localPlayerInRange = distanceToPlayer <= requiredDistance;
            bool playerMaxMana = Player.LocalInstance.playerMana.currentMana.Value == Player.LocalInstance.playerMana.maxMana;

            if (localPlayerInRange && !playerMaxMana)
            {
                manaTimer += Time.deltaTime;
                if (manaTimer >= manaRegenSpeed)
                {
                    Player.LocalInstance.playerMana.IncrementPlayerManaServerRpc(1);
                    SpendFountainManaServerRpc();
                    manaTimer = 0f;
                }
            }
            else
            {
                manaTimer = 0f;
            }

            ChangeAnimation(AnyPlayerRegenValid() ? SPIN : IDLE);
        }
        else
        {
            ChangeAnimation(DEACTIVATE);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpendFountainManaServerRpc()
    {
        fountainManaRemaining.Value--;
    }

    private void SyncRemainingMana(int prevVal, int newVal)
    {
        fountainManaRemaining.Value = newVal;
    }

    private bool AnyPlayerRegenValid()
    {
        foreach (Player player in FindObjectsOfType<Player>())
        {
            float distanceToPlayer = Vector3.Magnitude(player.transform.position - transform.position);
            bool playerMaxMana = player.playerMana.currentMana.Value == player.playerMana.maxMana;

            if (distanceToPlayer <= requiredDistance && !playerMaxMana)
            {
                return true;
            }
        }

        return false;
    }

    private void ChangeAnimation(string newAnim)
    {
        if(newAnim!= currentAnim)
        {
            Debug.Log("Changing anim to :" + newAnim);
            currentAnim = newAnim;
            GetComponent<Animator>().CrossFadeInFixedTime(newAnim, 0.2f);

        }
    }
}
