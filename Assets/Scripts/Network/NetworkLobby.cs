using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using QFSW.QC;

public class NetworkLobby : MonoBehaviour
{

    private Lobby hostLobby;
    private Lobby joinedLobby;

    private float heartbeatTimer = 15f;
    private float lobbyUpdateTimer = 1.1f;

    private string playerName;

    private async void Start()
    {
        playerName = "Piers#" + Random.Range(10, 100);
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log(playerName);
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
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer <= 0f)
            {
                lobbyUpdateTimer = 1.1f;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                joinedLobby = lobby;
            }

        }
    }

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
            joinedLobby = hostLobby;

            Debug.Log("Lobby started: " + lobby.Name + " max players=" + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    async private void ListLobbies()
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
                    new QueryFilter(QueryFilter.FieldOptions.S1, "StoryMode", QueryFilter.OpOptions.EQ),
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.AvailableSlots)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log("Lobbies Found : " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " | " + lobby.MaxPlayers + " players | " + lobby.Data["GameMode"].Value);
            };
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            Debug.Log("Joined lobby with code: " + lobbyCode);
            PrintPlayers(joinedLobby);
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
            joinedLobby = hostLobby;

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    private async void UpdatePlayerName(string newName)
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
            Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, updatePlayerOptions);

            joinedLobby = lobby;
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
        PrintPlayers(joinedLobby);
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log(lobby.Name + "(" + lobby.Players.Count + ") | " + lobby.Data["GameMode"].Value + " | " + lobby.Data["Level"].Value);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Data["PlayerName"].Value + " id=" + player.Id);
        }
    }

    [Command]
    private async void LeaveLobby()
    {
        try
        { 
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby = null;
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
                HostId = joinedLobby.Players[1].Id
            };
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, updateLobbyOptions);
            joinedLobby = hostLobby;

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
