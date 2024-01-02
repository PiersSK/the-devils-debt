using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private TextMeshProUGUI restartButtonText;
    [SerializeField] private TextMeshProUGUI gameOverMessage;

    private const string SUCCESS = "Success";
    private const string FAILURE = "Game Over";

    private const string HOSTBUTTON = "Play Again";
    private const string CLIENTBUTTON = "Waiting For Host...";

    private const string HOMESCENE = "HomeBase";

    private void Start()
    {
        restartButton.interactable = Player.LocalInstance.playerIsHost;
        restartButtonText.text = Player.LocalInstance.playerIsHost ? HOSTBUTTON : CLIENTBUTTON;

        restartButton.onClick.AddListener(ResetGameServerRpc);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetGameServerRpc()
    {
        Debug.Log("ResetGameServerRpc called");
        NetworkManager.Singleton.SceneManager.LoadScene(HOMESCENE, UnityEngine.SceneManagement.LoadSceneMode.Single);

        foreach (Player player in FindObjectsOfType<Player>())
        {
            player.GetComponent<CharacterController>().enabled = false;
            player.transform.position = Vector3.zero;
            player.GetComponent<CharacterController>().enabled = true;
        }

        ResetGameClientRpc();
    }

    [ClientRpc] //TODO Doesn't run on non host client????
    private void ResetGameClientRpc()
    {
        Debug.Log("Server resetting game through clientRPC");
        // Clear inventory
        Player.LocalInstance.playerInventory.ClearEquipiment();
        // Reset Objective UI
        UIManager.Instance.objectiveUI.SetActive(false);
        UIManager.Instance.objectiveBar.fillAmount = 0;
        // Reset player health and mana
        Player.LocalInstance.playerMana.currentMana.Value = Player.LocalInstance.playerMana.maxMana;
        Player.LocalInstance.playerHealth.currentHealth.Value = Player.LocalInstance.playerHealth.maxHealth;

        // Unpause game
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        gameObject.SetActive(false);
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
        gameObject.SetActive(true);
        gameOverMessage.text = success ? SUCCESS : FAILURE;
    }

}
