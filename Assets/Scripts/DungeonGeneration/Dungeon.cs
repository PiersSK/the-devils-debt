using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using static Door;
using Unity.AI.Navigation;

public class Dungeon : NetworkBehaviour
{
    public float objectiveSpawnChance = 0f;
    public float objectiveMaxRooms = 10f;
    private bool objectiveSpawned = false;

    [SerializeField] private Room startingRoom;
    [SerializeField] private int maxRoomRadius = 10;
    [SerializeField] private int maxFloors = 3;
    [SerializeField] private float roomWidth = 20f;
    [SerializeField] private float roomHeight = 6f;
    [SerializeField] private float stairSpawnChance = 10f;
    private Room[,,] dungeonGrid;

    private NetworkObject lastSpawnedRoom;

    private void Start()
    {
        InitiateGrid();
        dungeonGrid[maxRoomRadius, maxRoomRadius, (int)maxFloors/2] = startingRoom;
        startingRoom.roomCoords = new Vector3(maxRoomRadius, maxRoomRadius, (int)maxFloors / 2);
    }

    private void InitiateGrid()
    {
        dungeonGrid = new Room[(maxRoomRadius * 2)+1, (maxRoomRadius * 2)+1, maxFloors];
        for (int i = 0; i < dungeonGrid.GetLength(0); i++)
        {
            for (int j = 0; j < dungeonGrid.GetLength(1); j++)
            {
                for (int k = 0; k < dungeonGrid.GetLength(2); k++)
                {
                    dungeonGrid[i, j, k] = null;
                }
            }
        }
    }

    private Room GetRoomFromVector3(Vector3 pos)
    {
        if(pos.x < 0 || pos.x > maxRoomRadius*2 || pos.y < 0 || pos.y > maxRoomRadius*2 || pos.z < 0 || pos.z > maxFloors)
            return null;
        return dungeonGrid[(int)pos.x, (int)pos.y, (int)pos.z];
    }

    private bool RoomExistsAtCoords(Vector3 pos)
    {
        return GetRoomFromVector3(pos) != null; 
    }

    private bool CanSpawnUpStairs(Vector3 pos, DoorDirection dir)
    {
        if ((int)pos.z == maxFloors - 1)
            return false;

        Vector3 spawnAttemptLoc = Door.DirectionToGrid(dir) + pos + new Vector3(0, 0, 1);

        return !RoomExistsAtCoords(spawnAttemptLoc);
    }

    private bool CanSpawnDownStairs(Vector3 pos, DoorDirection dir)
    {
        if ((int)pos.z == 0)
            return false;

        Vector3 spawnAttemptLoc = Door.DirectionToGrid(dir) + pos + new Vector3(0, 0, -1);

        return !RoomExistsAtCoords(spawnAttemptLoc);
    }

    public void AddRandomRoom(Room currentRoom, DoorDirection dir, Room.RoomType roomType)
    {
        Vector3 currentPos = currentRoom.gameObject.transform.position;
        Vector3 currentCoords = currentRoom.roomCoords;
        NetworkObject currentRoomNO = currentRoom.GetComponent<NetworkObject>();
        string prefabName = "Rooms/";
        bool trySpawnStairs = false;

        if (roomType == Room.RoomType.Random)
        {
            roomType = (Room.RoomType)Random.Range(0, 3);
            trySpawnStairs = true;
        }

        switch (roomType)
        {
            case Room.RoomType.Boon:
                prefabName += "BoonRoom";
                break;
            case Room.RoomType.Treasure:
                prefabName += "TreasureRoom";
                break;
            case Room.RoomType.Monster:
                prefabName += "MonsterRoom";
                break;
            case Room.RoomType.Objective:
                prefabName += "ObjectiveRoom";
                break;
            default:
                prefabName += "Room";
                break;
        }

        //InitiateNewRoomServerRpc("Room", currentPos, currentCoords, dir, roomType);

        if (Random.Range(0, 100) < stairSpawnChance && roomType != Room.RoomType.Objective && trySpawnStairs)
        {
            int upDown = Random.Range(0, 2); //0=up, 1=down
            if (CanSpawnDownStairs(currentRoom.roomCoords, dir) && upDown == 0)
            {
                Debug.Log("Spawning stairs down");

                InitiateNewRoomServerRpc(currentRoomNO, "Stairwell Top", dir, roomType);

                Vector3 stairTopPos = currentPos + (DirectionToWorldSpace(dir) * roomWidth);
                Vector3 stairTopGrid = currentCoords + DirectionToGrid(dir);

                InitiateNewRoomServerRpc(lastSpawnedRoom, "Stairwell Bottom", DoorDirection.Down, roomType);
            }
            else if (CanSpawnUpStairs(currentRoom.roomCoords, dir) && upDown == 1)
            {
                Debug.Log("Spawning stairs up");

                InitiateNewRoomServerRpc(currentRoomNO, "Stairwell Bottom", dir, roomType);

                Vector3 stairBottomPos = currentPos + (DirectionToWorldSpace(dir) * roomWidth);
                Vector3 stairBottomGrid = currentCoords + DirectionToGrid(dir);

                InitiateNewRoomServerRpc(lastSpawnedRoom, "Stairwell Top", DoorDirection.Up, roomType);
            }
            else
            {
                InitiateNewRoomServerRpc(currentRoomNO, prefabName, dir, roomType);

            }
        }
        else
        {
            InitiateNewRoomServerRpc(currentRoomNO, prefabName, dir, roomType);

        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InitiateNewRoomServerRpc(NetworkObjectReference currentRoomNOR, string prefabName, DoorDirection dir, Room.RoomType roomType)
    {
        

        // Server instantiates and spawns the room
        GameObject newRoomObj = Instantiate(Resources.Load<GameObject>(prefabName));
        newRoomObj.GetComponent<NetworkObject>().Spawn();
        newRoomObj.transform.parent = transform;

        // Server sets room up and sends to clients
        InitiateNewRoomClientRpc(newRoomObj.GetComponent<NetworkObject>(), currentRoomNOR, dir, roomType);

        // Server spawns room contents
        newRoomObj.GetComponent<Room>().SpawnRoomContents();
    }

    [ClientRpc]
    private void InitiateNewRoomClientRpc(NetworkObjectReference roomNOR, NetworkObjectReference currentRoomNOR, DoorDirection dir, Room.RoomType roomType)
    {
        currentRoomNOR.TryGet(out NetworkObject currentRoomNO);
        Vector3 currentPos = currentRoomNO.transform.position;
        Vector3 currentCoords = currentRoomNO.GetComponent<Room>().roomCoords;

        if (roomType == Room.RoomType.Objective)
            ObjectiveController.GetObjectiveController().ShowObjectiveUI();

        roomNOR.TryGet(out NetworkObject roomNetworkObject);
        Room newRoom = roomNetworkObject.GetComponent<Room>();

        //Move room to correct position
        Vector3 newRoomPos = currentPos;
        float spaceShift = dir == DoorDirection.Up || dir == DoorDirection.Down ? roomHeight : roomWidth;
        newRoomPos += (DirectionToWorldSpace(dir) * spaceShift);
        newRoom.transform.position = newRoomPos;

        //Add room to dungeon grid
        Vector3 newGridIndex = currentCoords;
        newGridIndex += DirectionToGrid(dir);
        dungeonGrid[(int)newGridIndex.x, (int)newGridIndex.y, (int)newGridIndex.z] = newRoom;
        newRoom.roomCoords = newGridIndex; // Let room object know what co-ords it's at

        // Block doors to OOB
        foreach (Door door in newRoom.doors)
        {
            door.dungeon = this;
            if ((newRoom.roomCoords.x + DirectionToGrid(door.direction).x) < 0
                || (newRoom.roomCoords.x + DirectionToGrid(door.direction).x) > maxRoomRadius * 2
                || (newRoom.roomCoords.y + DirectionToGrid(door.direction).y) < 0
                || (newRoom.roomCoords.y + DirectionToGrid(door.direction).y) > maxRoomRadius * 2)
            {
                door.BlockDoor();
            }
        }
        
        if (dir != DoorDirection.Up && dir != DoorDirection.Down)
        {
            newRoom.doors[((int)dir + 2) % 4].RemoveDoor(); //Remove door joining to entrance
        }

        //Open doors to other adjacent rooms (leave Objective rooms as dead ends)
        if (roomType != Room.RoomType.Objective)
        {
            List<Room> neighbours = GetRoomNeighbours(newRoom);
            foreach (Room room in neighbours)
            {
                if (room.roomCoords != currentCoords)
                    room.DisableDoorToNeighbour(newRoom);
            }
        }

        //If the objective room hasn't spawned, give a chance for the door to be here
        if (!objectiveSpawned)
        {
            float randSpawn = Random.Range(0, 100);
            Debug.Log("Rolled " + randSpawn + " to spawn objective (currently needs " + objectiveSpawnChance + ")");
            if (randSpawn < objectiveSpawnChance)
            {
                Debug.Log("Spawning Objective!");
                newRoom.SpawnObjectiveDoor();
                objectiveSpawned = true;
            }
            else
            {
                objectiveSpawnChance += 100 / objectiveMaxRooms;
            }
        }

        currentRoomNO.GetComponent<Room>().floor.GetComponent<NavMeshSurface>().BuildNavMesh();

        lastSpawnedRoom = roomNetworkObject;
    }

    private List<Room> GetRoomNeighbours(Room room)
    {
        List<Room> neighbours = new List<Room>();
        foreach(DoorDirection dir in Door.cardinals)
        {
            neighbours.Add(GetRoomFromVector3(room.roomCoords + Door.DirectionToGrid(dir)));
        }
        neighbours.RemoveAll(item => item == null);

        return neighbours;
    }
}
