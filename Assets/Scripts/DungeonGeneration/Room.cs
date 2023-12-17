using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Door;

public class Room : MonoBehaviour
{
    public enum RoomType
    {
        Monster,
        Boon,
        Treasure,
        Random,
        Objective
    }

    public Door[] doors = new Door[4];
    public Vector3 roomCoords;
    public Light roomLight;
    
    public RoomType roomType;

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

    public void SetType(RoomType type)
    {
        roomType = type;

        if (roomType == RoomType.Random)
            type = (RoomType)Random.Range(0, 3);

        switch (type)
        {
            case RoomType.Treasure:
                roomLight.color = Color.blue;

                GameObject chest = GameObject.Instantiate(Resources.Load<GameObject>("TreasureChest"), transform);
                chest.transform.localPosition = new Vector3(0f, -1.7f, 0f);
                break;
            case RoomType.Boon:
                roomLight.color = Color.green;
                break;
            case RoomType.Monster:
                roomLight.color = Color.red;
                break;
            case RoomType.Objective:
                roomLight.color = Color.yellow;
                roomLight.intensity = 1;
                foreach (Door door in doors) door.BlockDoor();
                GameObject objectiveChest = GameObject.Instantiate(Resources.Load<GameObject>("ObjectiveChest"), transform);
                objectiveChest.transform.localPosition = new Vector3(0f, -1.7f, 0f);

                ObjectiveController.GetObjectiveController().ShowObjectiveUI();
                break;
        }
    }

}
