using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in to unity services with playerId: " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay()
    {
        try
        {
            // Creates game with 3 extra slots (i.e. 4 player game)
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            UIManager.Instance.playerUI_joinCode.text = "Join Code: " + joinCode;

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }

    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining relay with code: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }

    }
}
