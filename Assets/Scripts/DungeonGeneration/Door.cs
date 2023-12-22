using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Door : MonoBehaviour
{
    public enum DoorDirection
    {
        North,
        East,
        South,
        West,
        Down,
        Up
    }

    public static DoorDirection[] cardinals = new DoorDirection[4]
    {
        DoorDirection.North,
        DoorDirection.East,
        DoorDirection.South,
        DoorDirection.West
    };

    public DoorDirection direction;
    public Room room;
    public Dungeon dungeon;

    [SerializeField] private DoorButton randomButton;
    [SerializeField] private DoorButton[] chooseButtons = new DoorButton[3];

    public GameObject portal;
    public GameObject block;

    //[ServerRpc(RequireOwnership = false)]
    //private void DespawnPortalServerRpc()
    //{
    //    Debug.Log("DespawnPortalServerRpc IsServer=" + IsServer + "IsHost=" + IsHost + "IsClient=" + IsClient);

    //    portal.GetComponent<NetworkObject>().Despawn();
    //}

    //[ServerRpc(RequireOwnership = false)]
    //private void DespawnBlockServerRpc()
    //{
    //    block.GetComponent<NetworkObject>().Despawn();
    //}


    public void RemoveDoor()
    {
        // Destroy locally
        Destroy(portal);
        Destroy(block);
        //Debug.Log("DespawnPortalServerRpc IsServer=" + IsServer + "IsHost=" + IsHost + "IsClient=" + IsClient);

        //// Despawn on server
        //DespawnPortalServerRpc();
        //DespawnBlockServerRpc();

    }

    public void BlockDoor()
    {
        // Set locally
        block.SetActive(true);
        Destroy(portal);

        // Despawn on server
        //DespawnPortalServerRpc();
    }

    public void ConvertToObjective()
    {
        foreach (DoorButton button in chooseButtons)
        {
            button.gameObject.SetActive(false);
        }

        randomButton.roomType = Room.RoomType.Objective;
        randomButton.promptMessage = "Objective! [0]";

    }
    
    public static Vector3 DirectionToGrid(DoorDirection dir)
    {
        switch (dir)
        {
            case DoorDirection.North:
                return new Vector3(0, 1, 0);
            case DoorDirection.South:
                return new Vector3(0, -1, 0);
            case DoorDirection.East:
                return new Vector3(1, 0, 0);
            case DoorDirection.West:
                return new Vector3(-1, 0, 0);
            case DoorDirection.Up:
                return new Vector3(0, 0, 1);
            case DoorDirection.Down:
                return new Vector3(0, 0, -1);
        }

        return Vector3.zero;
    }

    public static Vector3 DirectionToWorldSpace(DoorDirection dir)
    {
        switch (dir)
        {
            case DoorDirection.North:
                return new Vector3(0, 0, 1);
            case DoorDirection.South:
                return new Vector3(0, 0, -1);
            case DoorDirection.East:
                return new Vector3(1, 0, 0);
            case DoorDirection.West:
                return new Vector3(-1, 0, 0);
            case DoorDirection.Up:
                return new Vector3(0, 1, 0);
            case DoorDirection.Down:
                return new Vector3(0, -1, 0);
        }

        return Vector3.zero;
    }

}
