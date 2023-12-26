using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using static Door;

public class Room : NetworkBehaviour
{    
    public enum RoomType
    {
        Monster,
        Boon,
        Treasure,
        Random,
        Objective
    }

    public Transform floor;

    public RoomType roomType;

    public Door[] doors = new Door[4];
    public Vector3 roomCoords;
    public Light roomLight;
       
    private void Awake()
    {
        OrderDoorArray();
    }

    private void OrderDoorArray()
    {
        List<Door> orderedDoors = new List<Door> ();

        for (int i = 0; i < 4; i++)
        {
            foreach (Door door in doors)
            {
                if (door.direction == (DoorDirection)i)
                {
                    orderedDoors.Add(door);
                    break;
                }
            }
            if (orderedDoors.Count <= i)
            {
                orderedDoors.Add(null);
            }
        }

        doors = orderedDoors.ToArray();
    }

    public void SpawnObjectiveDoor()
    {
        foreach (Door door in doors) //This has a directional bias, could make it random
        {
            if (door.portal.activeSelf)
            {
                door.ConvertToObjective();
                break;
            }
        }
    }
    public void DisableDoorToNeighbour(Room neighbour)
    {
        Vector2 diff = neighbour.roomCoords - roomCoords;
        Debug.Log("Disabling room to neighbor at: " + neighbour.roomCoords + "|| Direction: " + diff);

        if (diff.y == 1) // neighbour is north
        {
            if (roomType != RoomType.Objective) doors[0].RemoveDoor();
            neighbour.doors[2].RemoveDoor();
        }
        if (diff.y == -1) // neighbour is south
        {
            if (roomType != RoomType.Objective) doors[2].RemoveDoor();
            neighbour.doors[0].RemoveDoor();
        }
        if (diff.x == 1) // neighbour is east
        {
            if (roomType != RoomType.Objective) doors[1].RemoveDoor();
            neighbour.doors[3].RemoveDoor();
        }
        if (diff.x == -1) // neighbour is west
        {
            if (roomType != RoomType.Objective) doors[3].RemoveDoor();
            neighbour.doors[1].RemoveDoor();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnObjectInRoomServerRpc(string prefabName, Vector3 localOffset)
    {
        Debug.Log("Spawning chest in room");
        Transform obj = Instantiate(Resources.Load<Transform>(prefabName));
        NetworkObject objNO = obj.GetComponent<NetworkObject>();
        objNO.Spawn();
        objNO.transform.parent = transform;

        ChangeObjectLocalPositionClientRpc(objNO, localOffset);
    }

    [ClientRpc]
    private void ChangeObjectLocalPositionClientRpc(NetworkObjectReference objNOR, Vector3 localOffset)
    {
        objNOR.TryGet(out NetworkObject objNO);
        objNO.transform.localPosition = localOffset;
    }

    public virtual void SpawnRoomContents() { }
    public virtual Transform GetPrefab()
    {
        return Resources.Load<Transform>("Rooms/Room");
    }
}
