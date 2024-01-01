using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StartGame : NetworkInteractable
{
    
    public override bool CanInteract()
    {
        return Player.LocalInstance.playerIsHost;
    }

    protected override void Interact()
    {
        foreach(Player player in FindObjectsOfType<Player>())
        {
            player.playerInventory.SpawnStartingGearServerRpc();
        }

        NetworkManager.SceneManager.LoadScene("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
