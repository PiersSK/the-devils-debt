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

    private void Start()
    {
        restartButton.interactable = Player.LocalInstance.playerIsHost;
        restartButtonText.text = Player.LocalInstance.playerIsHost ? HOSTBUTTON : CLIENTBUTTON;

        restartButton.onClick.AddListener(ResetGameServerRpc);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetGameServerRpc()
    {
        foreach (Player player in FindObjectsOfType<Player>())
            player.transform.position = Vector3.zero;

        NetworkManager.Singleton.SceneManager.LoadScene("HomeBase", UnityEngine.SceneManagement.LoadSceneMode.Single);
        //TODO make this work. Network and persistent objects need to only spawn on load.

        gameObject.SetActive(false);
        Time.timeScale = 1f;
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
        Time.timeScale = 0f;
        gameObject.SetActive(true);
        gameOverMessage.text = success ? SUCCESS : FAILURE;
    }

}
