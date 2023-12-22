using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using QFSW.QC;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class NetworkLobby : MonoBehaviour
{
    public event EventHandler<LobbyChangedArgs> onJoinedLobbyChanged;
    public class LobbyChangedArgs : EventArgs
    {
        public Lobby lobby;
    }


    public event EventHandler<LobbyListChangedArgs> onLobbyListRefresh;
    public class LobbyListChangedArgs : EventArgs
    {
        public List<Lobby> lobbies;
    }

    public RelayManager relayManager;
    public LobbyUI lobbyUI;

    private Lobby hostLobby;
    private Lobby joinedLobby;
    public Lobby JoinedLobby {
        get {
            return joinedLobby;
        }
        set {
            joinedLobby = value;
            onJoinedLobbyChanged?.Invoke(this, new LobbyChangedArgs { lobby = joinedLobby });
        }
    }

    private float heartbeatTimer = 15f;
    private float lobbyUpdateTimer = 1.1f;

    public string playerName;

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log(playerName);

        relayManager.OnJoinedCodeChanged += StartGame;
        relayManager.OnRelayJoinComplete += JoinGame;
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyCallForUpdates();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f)
            {
                heartbeatTimer = 15f;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }

        }
    }

    private async void HandleLobbyCallForUpdates() // rate limit 1/s
    {
        if (JoinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer <= 0f)
            {
                lobbyUpdateTimer = 1.1f;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);

                joinedLobby = lobby;
                // Would add check to see if you've been kicked here

                if (JoinedLobby.Data["RELAY_JOIN_KEY"].Value != "0")
                {
                    if (!IsLobbyHost())
                    {
                        Debug.Log("Joining key: " + JoinedLobby.Data["RELAY_JOIN_KEY"].Value);
                        relayManager.JoinRelay(JoinedLobby.Data["RELAY_JOIN_KEY"].Value);
                    }

                    //JoinedLobby = null;
                }
            }

        }
    }

    async public void UpdateLobbyList()
    {
        try
        {
            // Options to alter search
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 4,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            onLobbyListRefresh?.Invoke(this, new LobbyListChangedArgs { lobbies = queryResponse.Results });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    async public void CreateLobby(string lobbyName)
    {
        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.Player = GetPlayer();
            lobbyOptions.Data = new Dictionary<string, DataObject>
            {
                { "RELAY_JOIN_KEY", new DataObject(DataObject.VisibilityOptions.Member, "0")}
            };

            hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4, lobbyOptions);
            JoinedLobby = hostLobby;

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions();
            joinLobbyByIdOptions.Player = GetPlayer();

            JoinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id, joinLobbyByIdOptions);

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public bool IsLobbyHost()
    {
        return AuthenticationService.Instance.PlayerId == JoinedLobby.HostId;
    }

    public void JoinGame(object caller, RelayManager.RelayJoinComplete ep)
    {
        Debug.Log("Relay joined! Joining game...");
        //SceneManager.LoadSceneAsync("Sandbox");
        //NetworkManager.Singleton.SceneManager.LoadScene("Dungeon", LoadSceneMode.Single);
        while (true)
        {
            try
            {
                PlayerInfo.GetPlayerInfo().PlayerName = new NetworkVariable<FixedString32Bytes>(playerName);
                break;
            }
            catch
            {
                Debug.Log("Couldn't find player: retrying");
            }
        }

        //cleanup lobby components once this process is done
        lobbyUI.gameObject.SetActive(false);
        lobbyUI.transform.parent.Find("PlayerUI").gameObject.SetActive(true);
        Destroy(relayManager.gameObject);
        Destroy(gameObject);
    }


    public async void StartGame(object caller, RelayManager.RelayJoinCodeChanged ep)
    {
        //SceneManager.LoadSceneAsync("Sandbox");
        NetworkManager.Singleton.SceneManager.LoadScene("Sandbox", LoadSceneMode.Single);

        GameObject.FindGameObjectWithTag("Self").GetComponent<PlayerInfo>().PlayerName = new NetworkVariable<FixedString32Bytes>(playerName);


        try
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RELAY_JOIN_KEY", new DataObject(DataObject.VisibilityOptions.Member, ep.newJoinCode) }
                }
            };
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, updateLobbyOptions);
            JoinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }

        //cleanup lobby components once this process is done
        lobbyUI.gameObject.SetActive(false);
        lobbyUI.transform.parent.Find("PlayerUI").gameObject.SetActive(true);
        Destroy(relayManager.gameObject);
        Destroy(gameObject);

        //Transform player = Instantiate(Resources.Load<Transform>("Player"));
        //player.GetComponent<NetworkObject>().Spawn();
    }

    // ============================================================= //
    // ======================== QC COMMANDS ======================== //
    // ============================================================= //
    [Command]
    async private void CreateLobby(bool isPrivate = false)
    {
        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject> // Custom options about the lobby
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "StoryMode", DataObject.IndexOptions.S1) },
                    { "Level", new DataObject(DataObject.VisibilityOptions.Public, "1: WizardsTower", DataObject.IndexOptions.S2) },
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("DDLobby", 4, lobbyOptions);

            hostLobby = lobby;
            JoinedLobby = hostLobby;


            Debug.Log("Lobby started: " + lobby.Name + " max players=" + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    

    [Command]
    async public void ListLobbies()
    {
        try
        {
            // Options to alter search
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    // e.g this one shows only lobbies with at least one slot still free
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    // new QueryFilter(QueryFilter.FieldOptions.S1, "StoryMode", QueryFilter.OpOptions.EQ),
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log("Lobbies Found : " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " | " + lobby.MaxPlayers + " players" + " | " + lobby.LobbyCode);
            };
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions();
            joinLobbyByCodeOptions.Player = GetPlayer();

            JoinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            Debug.Log("Joined lobby with code: " + lobbyCode);
            PrintPlayers(JoinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    private async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode, DataObject.IndexOptions.S1) }
                }
            };
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, updateLobbyOptions);
            JoinedLobby = hostLobby;

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    public async void UpdatePlayerName(string newName)
    {
        playerName = newName;
        try
        {
            UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                }
            };
            Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId, updatePlayerOptions);

            JoinedLobby = lobby;
            PrintPlayers();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    private void PrintPlayers()
    {
        PrintPlayers(JoinedLobby);
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log(lobby.Name + "(" + lobby.Players.Count + ")");
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Data["PlayerName"].Value + " id=" + player.Id);
        }
    }

    [Command]
    public async void LeaveLobby()
    {
        try
        { 
            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);
            JoinedLobby = null;
            hostLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    private async void KickPlayer() // kicks 2nd player as example
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, hostLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    private async void MigrateLobbyHost() // migrates to 2nd player as example
    {
        try
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
            {
                HostId = JoinedLobby.Players[1].Id
            };
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, updateLobbyOptions);
            JoinedLobby = hostLobby;

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private Player GetPlayer()
    {
        // You could pass things like chosen class in here
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }

            }
        };
    }
}
