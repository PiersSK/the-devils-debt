using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Door : NetworkBehaviour
{
    public enum DoorDirection
    {
        North,
        East,
        South,
        West,
        Down,
        Up,
        None
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
    private GameObject block;
    [SerializeField] private Material objectiveMaterial;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    private void Awake()
    {
        portal = gameObject.transform.Find("Portal").gameObject;
        block = gameObject.transform.Find("Block").gameObject;
    }

    public void RemoveDoor()
    {
        if (IsClient)
        {
            RemoveDoorServerRpc();
        }
        else
        {
            portal.SetActive(false);
            block.SetActive(false);
        }

    }

    public void SetNavMeshLink(Transform linkedFloor)
    {
        GetComponent<OffMeshLink>().endTransform = linkedFloor;
    }

    [ClientRpc]
    private void RemoveDoorClientRpc()
    {
        portal.SetActive(false);
        block.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveDoorServerRpc()
    {
        RemoveDoorClientRpc();
    }

    public void BlockDoor()
    {
        block.SetActive(true);
        portal.SetActive(false);
    }

    public void ConvertToObjective()
    {
        foreach (DoorButton button in chooseButtons)
        {
            button.gameObject.SetActive(false);
        }

        randomButton.roomType = Room.RoomType.Objective;
        randomButton.SetPromptMessage("Objective! [0]");
        portal.GetComponent<MeshRenderer>().material = objectiveMaterial;

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

    public static DoorDirection GridToDirection(Vector3 gridVector)
    {
        if (gridVector == new Vector3(0, 1, 0)) return DoorDirection.North;
        else if (gridVector == new Vector3(0, -1, 0)) return DoorDirection.South;
        else if (gridVector == new Vector3(1, 0, 0)) return DoorDirection.East;
        else if (gridVector == new Vector3(-1, 0, 0)) return DoorDirection.West;
        else if (gridVector == new Vector3(0, 0, 1)) return DoorDirection.Up;
        else if (gridVector == new Vector3(0, 0, -1)) return DoorDirection.Down;

        return DoorDirection.None;
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
