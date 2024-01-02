using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using static Door;
using Unity.AI.Navigation;
using System;
using Random = UnityEngine.Random;

public class Dungeon : NetworkBehaviour
{
    public static Dungeon Instance;

    [Header("Starting Objects")]
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private Room startingRoom;

    [Header("Generation Settings")]
    [SerializeField] private int maxRoomRadius = 10;
    [SerializeField] private int maxFloors = 3;
    [SerializeField] private float roomWidth = 20f;
    [SerializeField] private float roomHeight = 6f;
    [SerializeField] private float stairSpawnChance = 10f;
    private Room[,,] dungeonGrid;

    [Header("Objective Spawn")]
    [SerializeField] private int objectiveMinRadius = 2;
    [SerializeField] private int objectiveMaxRadius = 3;
    [SerializeField] private bool objectiveCanSpawnOnDifferentFloor = false;
    private Vector3 objectiveCoords;

    private NetworkObject lastSpawnedRoom;

    private void Start()
    {
        Instance = this;
        InitiateGrid();
        dungeonGrid[maxRoomRadius, maxRoomRadius, maxFloors/2] = startingRoom;
        startingRoom.roomCoords = new Vector3(maxRoomRadius, maxRoomRadius, maxFloors / 2);
        Debug.Log("Starting Room is at " + startingRoom.roomCoords);
        AssignObjectiveRoomCoords();
        Debug.Log("Objective Room is at " + objectiveCoords);
    }

    //TODO: NEEDS TO BE SET BY SERVER
    private void AssignObjectiveRoomCoords()
    {
        int x = (int)startingRoom.roomCoords.x + (Random.Range(0, 2) * 2 - 1) * Random.Range(objectiveMinRadius, objectiveMaxRadius + 1);
        int y = (int)startingRoom.roomCoords.y + (Random.Range(0, 2) * 2 - 1) * Random.Range(objectiveMinRadius, objectiveMaxRadius + 1);
        int z = objectiveCanSpawnOnDifferentFloor ? Random.Range(0, maxFloors) : (int)startingRoom.roomCoords.z;

        objectiveCoords = new Vector3(x, y, z);
    }

    private Vector3 GetGridOfPlayer()
    {
        Vector3 playerPos = Player.LocalInstance.transform.position;
        int x = maxRoomRadius + (int)Mathf.Round(playerPos.x / roomWidth);
        int y = maxRoomRadius + (int)Mathf.Round(playerPos.z / roomWidth);
        int z = (maxFloors / 2) + (int)Mathf.Round(playerPos.y / roomHeight);

        return new Vector3(x,y,z);
    }

    public DoorDirection GetPathToObjective()
    {
        float minDistance = Mathf.Infinity;
        DoorDirection bestDirection = DoorDirection.None;
        Vector3 playerRoomCoords = GetGridOfPlayer();

        if (playerRoomCoords == objectiveCoords) return bestDirection;

        foreach (DoorDirection dir in cardinals)
        {
            float dist = Vector3.Distance(objectiveCoords, (playerRoomCoords + DirectionToGrid(dir)));
            if (dist < minDistance)
            {
                bestDirection = dir;
                minDistance = dist;
            }
        }

        return bestDirection;
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

        if (Random.Range(0, 100) < stairSpawnChance && roomType != Room.RoomType.Objective && trySpawnStairs)
        {
            int upDown = Random.Range(0, 2); //0=up, 1=down
            if (CanSpawnDownStairs(currentRoom.roomCoords, dir) && upDown == 0)
            {
                InitiateNewRoomServerRpc(currentRoomNO, "Stairwell Top", dir, roomType);
                InitiateNewRoomServerRpc(lastSpawnedRoom, "Stairwell Bottom", DoorDirection.Down, roomType);
            }
            else if (CanSpawnUpStairs(currentRoom.roomCoords, dir) && upDown == 1)
            {
                InitiateNewRoomServerRpc(currentRoomNO, "Stairwell Bottom", dir, roomType);
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

        //Open doors to other adjacent rooms
        List<Room> neighbours = GetRoomNeighbours(newRoom);
        foreach (Room room in neighbours)
        {
            if (room.roomCoords != currentCoords)
                room.DisableDoorToNeighbour(newRoom);
        }

        Vector3 objectiveRelative = objectiveCoords - newRoom.roomCoords;
        DoorDirection objectiveDoor = Door.GridToDirection(objectiveRelative);
        if (Array.IndexOf(cardinals, objectiveDoor) > -1)
        {
            newRoom.doors[(int)objectiveDoor].ConvertToObjective();
        }

        navMeshSurface.BuildNavMesh();
        //currentRoomNO.GetComponent<Room>().floor.GetComponent<NavMeshSurface>().BuildNavMesh();

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
