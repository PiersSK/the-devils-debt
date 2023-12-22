using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkDestroyTest : NetworkInteractable
{
    protected override void Interact()
    {
        GameObject testObj = GameObject.Find("SphereObj");
        netSpawner.DestroyObject(testObj, PlayerInfo.GetPlayerInfo().IsServer);
    }
}
