using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using TMPro;
using UnityEngine.SceneManagement;


public class LobbyUI : MonoBehaviour
{
    [Header("Network Classes")]
    [SerializeField] private NetworkLobby netLobby;
    [SerializeField] private RelayManager relay;

    [Header("Persistent UI")]
    [SerializeField] private TextMeshProUGUI playerNameDisplay;
    [SerializeField] private TMP_InputField updatePlayerNameInput;
    [SerializeField] private Button updateNameBtn;

    [Header("UI Containers")]
    [SerializeField] private GameObject lobbyList;
    [SerializeField] private GameObject lobbySetup;
    [SerializeField] private GameObject lobbyDetails;

    [Header("Lobby List")]
    [SerializeField] private Button newLobbyBtn;
    [SerializeField] private Button refreshListBtn;
    [SerializeField] private Transform lobbyListParent;

    [Header("Lobby Setup")]
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private Button createLobbyBtn;

    [Header("Lobby Details")]
    [SerializeField] private TextMeshProUGUI joinedLobbyName;
    [SerializeField] private Button leaveLobbyBtn;
    [SerializeField] private Button startGameBtn;
    [SerializeField] private Transform playerListParent;

    private void Start()
    {
        DontDestroyOnLoad(transform.parent.gameObject);
        // Add lobby listeners
        netLobby.onLobbyListRefresh += RefreshLobbiesList;
        netLobby.onJoinedLobbyChanged += UpdateLobbyDetails;

        // Add playername update listener
        netLobby.playerName = "Player#" + UnityEngine.Random.Range(10, 100);
        playerNameDisplay.text = "Playing as: " + netLobby.playerName;
        updateNameBtn.onClick.AddListener(UpdatePlayerName);

        // Add lobby list buttons
        newLobbyBtn.onClick.AddListener(OpenLobbySetup);
        refreshListBtn.onClick.AddListener(RefreshLobbies);
        // Add lobby create buttons
        createLobbyBtn.onClick.AddListener(CreateNewLobby);
        createLobbyBtn.onClick.AddListener(OpenLobbyDetails);
        // Add lobby detail options
        leaveLobbyBtn.onClick.AddListener(LeaveLobby);
        startGameBtn.onClick.AddListener(StartRelay);
    }

    private void StartRelay()
    {
        relay.CreateRelay();
    }

    private void Update()
    {
        if (lobbyDetails.activeSelf && netLobby.JoinedLobby != null) RefreshPlayerList();
    }

    private void OpenLobbySetup()
    {
        lobbyList.SetActive(false);
        lobbySetup.SetActive(true);
        lobbyDetails.SetActive(false);
    }

    private void OpenLobbyDetails()
    {
        lobbyList.SetActive(false);
        lobbySetup.SetActive(false);
        lobbyDetails.SetActive(true);
    }

    private void UpdateLobbyDetails(object caller, NetworkLobby.LobbyChangedArgs e)
    {
        if (e.lobby != null && startGameBtn != null)
        {
            joinedLobbyName.text = e.lobby.Name;
            startGameBtn.gameObject.SetActive(netLobby.IsLobbyHost());
        }
    }

    private void LeaveLobby()
    {
        lobbyList.SetActive(true);
        lobbySetup.SetActive(false);
        lobbyDetails.SetActive(false);
        netLobby.LeaveLobby();
    }

    private void RefreshLobbiesList(object caller, NetworkLobby.LobbyListChangedArgs e)
    {
        
        foreach (Transform child in lobbyListParent) Destroy(child.gameObject);
        for (int i = 0; i < e.lobbies.Count; i++)
        {
            Lobby lobby = e.lobbies[i];
            Transform lobbyItem = Instantiate(Resources.Load<Transform>("UI/LobbyItem"), lobbyListParent);
            lobbyItem.localPosition = new Vector3(0, 115 + (-40 * i) - 20, 0); //fudged this with the +115 - make it smarter

            LobbyItemUI lobbyItemUI = lobbyItem.GetComponent<LobbyItemUI>();
            lobbyItemUI.UpdateServerName(lobby.Name);
            lobbyItemUI.UpdatePlayerCount(lobby.Players.Count, lobby.MaxPlayers);
            lobbyItemUI.lobby = lobby;

            lobbyItemUI.joinLobbyBtn.onClick.AddListener(OpenLobbyDetails);
        }
    }

    private void RefreshPlayerList()
    {
        foreach (Transform child in playerListParent) Destroy(child.gameObject);
        for (int i = 0; i < netLobby.JoinedLobby.Players.Count; i++)
        {
            Player player = netLobby.JoinedLobby.Players[i];
            Transform playerItem = Instantiate(Resources.Load<Transform>("UI/PlayerItem"), playerListParent);
            playerItem.localPosition = new Vector3(0, 115 + (-40 * i) - 20, 0); //fudged this with the +115 - make it smarter

            PlayerItemUI playerItemUI = playerItem.GetComponent<PlayerItemUI>();
            playerItemUI.UpdatePlayerItem(i, player.Data["PlayerName"].Value);
        }
    }

    private void RefreshLobbies()
    {
        netLobby.UpdateLobbyList();
        
    }

    private void CreateNewLobby()
    {
        netLobby.CreateLobby(lobbyNameInput.text);
    }

    private void UpdatePlayerName()
    {
        if (netLobby.JoinedLobby != null)
            netLobby.UpdatePlayerName(updatePlayerNameInput.text);
        else
            netLobby.playerName = updatePlayerNameInput.text;

        playerNameDisplay.text = "Playing as: " + netLobby.playerName;
    }
}
