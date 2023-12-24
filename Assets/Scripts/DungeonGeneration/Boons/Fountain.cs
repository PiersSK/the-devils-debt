using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fountain : MonoBehaviour
{
    private float manaTimer = 0f;
    private float manaRegenSpeed = 1f;

    private float requiredDistance = 7f;

    private const string IDLE = "Idle";
    private const string SPIN = "RegenSpin";
    private string currentAnim = IDLE;

    // Update is called once per frame
    void Update()
    {
        //If local player is nearby, give regen effect
        float distanceToPlayer = Vector3.Magnitude(Player.LocalInstance.transform.position - transform.position);
        bool playerMaxMana = Player.LocalInstance.playerMana.CurrentMana == Player.LocalInstance.playerMana.maxMana;

        if (distanceToPlayer <= requiredDistance && !playerMaxMana)
        {
            UIManager.Instance.playerUI_manaOverlay.gameObject.SetActive(true);
            manaTimer += Time.deltaTime;
            if(manaTimer >= manaRegenSpeed)
            {
                Player.LocalInstance.playerMana.CurrentMana += 1;
                manaTimer = 0f;
            }
        }
        else
        {
            UIManager.Instance.playerUI_manaOverlay.gameObject.SetActive(false);
            manaTimer = 0f;
        }

        ChangeAnimation(AnyPlayerRegenValid() ? SPIN : IDLE);
    }

    private bool AnyPlayerRegenValid()
    {
        foreach (Player player in FindObjectsOfType<Player>())
        {
            float distanceToPlayer = Vector3.Magnitude(player.transform.position - transform.position);
            bool playerMaxMana = player.playerMana.CurrentMana == player.playerMana.maxMana;

            return distanceToPlayer <= requiredDistance && !playerMaxMana;
        }

        return false;
    }

    private void ChangeAnimation(string newAnim)
    {
        if(newAnim!= currentAnim)
        {
            currentAnim = newAnim;
            GetComponent<Animator>().CrossFadeInFixedTime(newAnim, 0.2f);

        }
    }
}
