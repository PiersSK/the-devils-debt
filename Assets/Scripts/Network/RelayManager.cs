using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;
using System;


public class RelayManager : MonoBehaviour
{

    public event EventHandler<RelayJoinCodeChanged> OnJoinedCodeChanged;
    public class RelayJoinCodeChanged : EventArgs
    {
        public string newJoinCode;
    }

    public event EventHandler<RelayJoinComplete> OnRelayJoinComplete;
    public class RelayJoinComplete : EventArgs
    {
    }

    public string joinCode;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        
    }


    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            Debug.Log("Should invoke OnJoinedCodeChanged");
            OnJoinedCodeChanged?.Invoke(this, new RelayJoinCodeChanged { newJoinCode = joinCode }); 
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinRelay(string code)
    {
        try
        {
            Debug.Log("Joining Relay...");
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();

            OnRelayJoinComplete?.Invoke(this, new RelayJoinComplete { });
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }
}
