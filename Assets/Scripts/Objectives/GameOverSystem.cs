using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameOverSystem : NetworkBehaviour
{
    public static GameOverSystem Instance { get; private set; }

    private const string HOMESCENE = "HomeBase";

    private const string SUCCESS = "Success";
    private const string FAILURE = "Game Over";

    private void Awake()
    {
        Instance = this;
    }

    //private void Start()
    //{
    //    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
    //}

    //private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    //{
    //    Debug.Log("Scene Loaded");
    //    if (sceneName == HOMESCENE)
    //        ResetSelf();
    //}

    [ServerRpc(RequireOwnership = false)]
    public void ResetGameServerRpc()
    {
        Debug.Log("ResetGameServerRpc called");
        NetworkManager.Singleton.SceneManager.LoadScene(HOMESCENE, UnityEngine.SceneManagement.LoadSceneMode.Single);

        foreach (Player player in FindObjectsOfType<Player>())
        {
            // Reset player position
            player.GetComponent<CharacterController>().enabled = false;
            player.transform.position = Vector3.zero;
            player.GetComponent<CharacterController>().enabled = true;

            // Reset player equipment
            player.playerInventory.DestroyEquipment();
            player.playerInventory.ClearEquipment();

            // Reset player health and mana
            player.playerMana.currentMana.Value = Player.LocalInstance.playerMana.maxMana;
            player.playerHealth.currentHealth.Value = Player.LocalInstance.playerHealth.maxHealth;

        }

        ResetSelfClientRpc();
    }

    [ClientRpc]
    private void ResetSelfClientRpc()
    {
        ResetSelf();
    }

    private void ResetSelf()
    {
        Debug.Log("Resetting self");
        // Clear inventory
        Player.LocalInstance.playerInventory.ClearEquipment();

        // Reset Objective UI
        UIManager.Instance.objectiveUI.SetActive(false);
        UIManager.Instance.timer.gameObject.SetActive(false);
        UIManager.Instance.objectiveBar.fillAmount = 0;

        // Unpause game
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        UIManager.Instance.gameOver.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void GameOverServerRpc(bool success = true)
    {
        GameOverClientRpc(success);
    }

    [ClientRpc]
    public void GameOverClientRpc(bool success = true)
    {
        GameOver(success);
    }

    public void GameOver(bool success)
    {
        Cursor.lockState = CursorLockMode.None;
        Player.LocalInstance.transform.position = Vector3.zero;
        Time.timeScale = 0f;
        UIManager.Instance.gameOver.gameObject.SetActive(true);
        UIManager.Instance.gameOver.gameOverMessage.text = success ? SUCCESS : FAILURE;
    }
}
