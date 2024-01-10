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
    public TextMeshProUGUI gameOverMessage;

    private const string HOSTBUTTON = "Play Again";
    private const string CLIENTBUTTON = "Waiting For Host...";

    private void Start()
    {
        restartButton.interactable = Player.LocalInstance.playerIsHost;
        restartButtonText.text = Player.LocalInstance.playerIsHost ? HOSTBUTTON : CLIENTBUTTON;

        restartButton.onClick.AddListener(GameOverSystem.Instance.ResetGameServerRpc);

    }
}
