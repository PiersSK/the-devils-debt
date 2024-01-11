using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using static Door;
using Unity.AI.Navigation;
using System;
using Random = UnityEngine.Random;

public class Dungeon : NetworkBehaviour
{
    public static Dungeon Instance { get; private set; }
    public event EventHandler<OnRoomSpawnArgs> OnRoomSpawn;
    public class OnRoomSpawnArgs : EventArgs
    {
        public Transform RoomTransform;
    }


    [Header("Starting Objects")]
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private Room startingRoom;

    [Header("Generation Settings")]
    [SerializeField] private int maxRoomRadius = 10;
    [SerializeField] private int maxFloors = 3;
    [SerializeField] private float roomWidth = 20f;
    [SerializeField] private float roomHeight = 6f;
    [SerializeField] private float stairSpawnChance = 10f;
    [SerializeField] private float roomExpandChance = 20f;

    //private NetworkVariable<float> roomExpandCurrentChance = new(20f);

    private Room[,,] dungeonGrid;

    [Header("Objective Spawn")]
    [SerializeField] private int objectiveMinRadius = 2;
    [SerializeField] private int objectiveMaxRadius = 3;
    [SerializeField] private bool objectiveCanSpawnOnDifferentFloor = false;
    private Vector3 objectiveCoords;

    [Header("Puzzle Spawn")]
    [SerializeField] private int puzzleMinRadius = 2;
    [SerializeField] private int puzzleMaxRadius = 2;
    [SerializeField] private bool puzzleCanSpawnOnDifferentFloor = false;
    private Vector3 puzzleCoords;

    private NetworkObject lastSpawnedRoom;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitiateGrid();
        dungeonGrid[maxRoomRadius, maxRoomRadius, maxFloors/2] = startingRoom;
        startingRoom.roomCoords = new Vector3(maxRoomRadius, maxRoomRadius, maxFloors / 2);

        if (Player.LocalInstance.playerIsHost)
        {
            AssignSpecialRoomCoords();
            SetupSpecialClientRpc(objectiveCoords, puzzleCoords);
        }
    }

    [ClientRpc]
    private void SetupSpecialClientRpc(Vector3 objCoords, Vector3 puzCoords)
    {
        Debug.Log("Setting objective coordinates to: " + objCoords);
        Debug.Log("Setting puzzle coordinates to: " + puzCoords);
        objectiveCoords = objCoords;
        puzzleCoords = puzCoords;
    }

    private void AssignSpecialRoomCoords()
    {
        objectiveCoords = GetRandomCoordsAtRange(startingRoom.roomCoords, objectiveMinRadius, objectiveMaxRadius, objectiveCanSpawnOnDifferentFloor);

        do puzzleCoords = GetRandomCoordsAtRange(startingRoom.roomCoords, puzzleMinRadius, puzzleMaxRadius, puzzleCanSpawnOnDifferentFloor);
        while (puzzleCoords == objectiveCoords);
    }

    private Vector3 GetRandomCoordsAtRange(Vector3 coords, int minRange, int maxRange, bool canSpawnOnDifferentFloor)
    {
        int x = (int)coords.x + (Random.Range(0, 2) * 2 - 1) * Random.Range(minRange, maxRange + 1);
        int y = (int)coords.y + (Random.Range(0, 2) * 2 - 1) * Random.Range(minRange, maxRange + 1);
        int z = canSpawnOnDifferentFloor ? Random.Range(0, maxFloors) : (int)coords.z;

        return new Vector3(x, y, z);
    }

    private Vector3 GetGridOfPlayer()
    {
        Vector3 playerPos = Player.LocalInstance.transform.position;
        int x = maxRoomRadius + (int)Mathf.Round(playerPos.x / roomWidth);
        int y = maxRoomRadius + (int)Mathf.Round(playerPos.z / roomWidth);
        int z = (maxFloors / 2) + (int)Mathf.Round(playerPos.y / roomHeight);

        return new Vector3(x,y,z);
    }

    public DoorDirection GetPathToPuzzle()
    {
        float minDistance = Mathf.Infinity;
        DoorDirection bestDirection = DoorDirection.None;
        Vector3 playerRoomCoords = GetGridOfPlayer();

        if (playerRoomCoords == puzzleCoords) return bestDirection;

        foreach (DoorDirection dir in cardinals)
        {
            float dist = Vector3.Distance(puzzleCoords, (playerRoomCoords + DirectionToGrid(dir)));
            if (dist < minDistance)
            {
                bestDirection = dir;
                minDistance = dist;
            }
        }

        return bestDirection;
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

    [ServerRpc(RequireOwnership = false)]
    public void AddRandomRoomServerRpc(NetworkObjectReference currentRoomNOR, DoorDirection dir, Room.RoomType roomType)
    {
        currentRoomNOR.TryGet(out NetworkObject currentRoomNO);
        Room currentRoom = currentRoomNO.GetComponent<Room>();
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
            case Room.RoomType.Puzzle:
                prefabName += "PuzzleRoom";
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
                InitiateNewRoom(currentRoomNO, "Stairwell Top", dir, Room.RoomType.Stairs);
                InitiateNewRoom(lastSpawnedRoom, "Stairwell Bottom", DoorDirection.Down, Room.RoomType.Stairs);
            }
            else if (CanSpawnUpStairs(currentRoom.roomCoords, dir) && upDown == 1)
            {
                InitiateNewRoom(currentRoomNO, "Stairwell Bottom", dir, Room.RoomType.Stairs);
                InitiateNewRoom(lastSpawnedRoom, "Stairwell Top", DoorDirection.Up, Room.RoomType.Stairs);
            }
            else
            {
                InitiateNewRoom(currentRoomNO, prefabName, dir, roomType);
            }
        }
        else
        {
            InitiateNewRoom(currentRoomNO, prefabName, dir, roomType);
        }
    }

    private void InitiateNewRoom(
        NetworkObjectReference currentRoomNOR,
        string prefabName,
        DoorDirection dir,
        Room.RoomType roomType,
        bool tryExpand = true)
    {
        // Server instantiates and spawns the room
        GameObject newRoomObj = Instantiate(Resources.Load<GameObject>(prefabName));
        newRoomObj.GetComponent<NetworkObject>().Spawn();
        newRoomObj.transform.parent = transform;

        // Server sets room up and sends to clients
        InitiateNewRoomClientRpc(newRoomObj.GetComponent<NetworkObject>(), currentRoomNOR, dir, roomType);

        Room newRoom = newRoomObj.GetComponent<Room>();

        //Chance for room to expand
        List<Room> neighbours = GetRoomNeighbours(newRoom);
        List<Vector3> neighbourCoords = new();
        foreach (Room room in neighbours) neighbourCoords.Add(room.roomCoords);

        List<Room.RoomType> invalidExpandTypes = new() { Room.RoomType.Objective, Room.RoomType.Stairs, Room.RoomType.Puzzle };
        if (!invalidExpandTypes.Contains(roomType) && tryExpand)
        {
            foreach (DoorDirection possibleDir in cardinals)
            {
                Vector3 possibleLoc = newRoom.roomCoords + DirectionToGrid(possibleDir);
                if (Random.Range(0, 100) > roomExpandChance) continue;

                if (!neighbourCoords.Contains(possibleLoc)
                    && possibleLoc != objectiveCoords
                    && possibleLoc != puzzleCoords
                    && !IsOutOfBounds(newRoom.roomCoords, possibleDir))
                {
                    newRoom.RemoveWallClientRpc((int)possibleDir); // Remove room to new room
                    InitiateNewRoom(newRoomObj.GetComponent<NetworkObject>(), prefabName, possibleDir, roomType, false); //Spawn new room
                    lastSpawnedRoom.GetComponent<Room>().RemoveWallClientRpc(((int)possibleDir + 2) % 4); //Remove wall of new room to this one
                    navMeshSurface.BuildNavMesh();
                }

            }
        }

        // Server spawns room contents
        newRoomObj.GetComponent<Room>().SpawnRoomContents();
    }

    [ClientRpc]
    private void InitiateNewRoomClientRpc(
        NetworkObjectReference roomNOR,
        NetworkObjectReference currentRoomNOR,
        DoorDirection dir,
        Room.RoomType roomType)
    {
        currentRoomNOR.TryGet(out NetworkObject currentRoomNO);
        Vector3 currentPos = currentRoomNO.transform.position;
        Vector3 currentCoords = currentRoomNO.GetComponent<Room>().roomCoords;

        if (roomType == Room.RoomType.Objective)
            ObjectiveController.GetObjectiveController().ShowObjectiveUI();

        roomNOR.TryGet(out NetworkObject roomNetworkObject);
        Room newRoom = roomNetworkObject.GetComponent<Room>();

        lastSpawnedRoom = roomNetworkObject;

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
            if(IsOutOfBounds(newRoom.roomCoords, door.direction)) door.BlockDoor();
        }
        
        if (dir != DoorDirection.Up && dir != DoorDirection.Down)
        {
            newRoom.doors[((int)dir + 2) % 4].RemoveDoor(); //Remove door joining to entrance
        }

        //Open doors to other adjacent rooms
        List<Room> neighbours = GetRoomNeighbours(newRoom);
        List<Vector3> neighbourCoords = new();
        foreach (Room room in neighbours)
        {
            neighbourCoords.Add(room.roomCoords);
            if (room.roomCoords != currentCoords)
                room.DisableDoorToNeighbour(newRoom);
        }

        DoorDirection objectiveDoor = Door.GridToDirection(objectiveCoords - newRoom.roomCoords);
        DoorDirection puzzleDoor = Door.GridToDirection(puzzleCoords - newRoom.roomCoords);

        if (Array.IndexOf(cardinals, objectiveDoor) > -1)
            newRoom.doors[(int)objectiveDoor].ConvertToObjective();

        if (Array.IndexOf(cardinals, puzzleDoor) > -1)
            newRoom.doors[(int)puzzleDoor].ConvertToPuzzle();


        navMeshSurface.BuildNavMesh();
        Debug.Log("Invoking Room Spawn");
        OnRoomSpawn?.Invoke(this, new OnRoomSpawnArgs { RoomTransform = roomNetworkObject.transform });
    }

    private bool IsOutOfBounds(Vector3 coords, DoorDirection dir)
    {
        return (coords.x + DirectionToGrid(dir).x) < 0
        || (coords.x + DirectionToGrid(dir).x) > maxRoomRadius * 2
        || (coords.y + DirectionToGrid(dir).y) < 0
        || (coords.y + DirectionToGrid(dir).y) > maxRoomRadius * 2;
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
