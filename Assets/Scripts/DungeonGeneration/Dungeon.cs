using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using static Door;
using Mono.CSharp;

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

    private PlayerInfo player;

    private Transform roomToSpawn;
    private Room spawningRoom;
    private Room lastSpawned;



    private void Start()
    {
        //Debug.Log("Dungeon start?");
        //Debug.Log("Creating Player");
        //Transform player = Instantiate(Resources.Load<Transform>("Player"));
        //player.GetComponent<NetworkObject>().Spawn();
        //if(!IsSpawned)
        //    NetworkObject.Spawn();
        //SpawnPlayerServerRpc(player);
        player = PlayerInfo.GetPlayerInfo();
        InitiateGrid();
        dungeonGrid[maxRoomRadius, maxRoomRadius, (int)maxFloors/2] = startingRoom;
        startingRoom.roomCoords = new Vector3(maxRoomRadius, maxRoomRadius, (int)maxFloors / 2);
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void SpawnPlayerServerRpc(Transform player)
    //{
    //    Debug.Log("Spawning player...");
    //}

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
        Debug.Log("AddRandomRoom");
        spawningRoom = currentRoom;

        if (Random.Range(0, 100) < stairSpawnChance && roomType != Room.RoomType.Objective)
        {
            int upDown = Random.Range(0, 2); //0=up, 1=down
            if (CanSpawnDownStairs(currentRoom.roomCoords, dir) && upDown == 0)
            {
                InitiateNewRoom(dir, roomType, "Stairwell Top");
                spawningRoom = lastSpawned;
                InitiateNewRoom(DoorDirection.Down, roomType, "Stairwell Bottom");
            }
            else if (CanSpawnUpStairs(currentRoom.roomCoords, dir) && upDown == 1)
            {
                InitiateNewRoom(dir, roomType, "Stairwell Bottom");
                spawningRoom = lastSpawned;
                InitiateNewRoom(DoorDirection.Up, roomType, "Stairwell Top");
            }
            else
            {
                InitiateNewRoom(dir, roomType, "Room");
            }
        }
        else
        {
            InitiateNewRoom(dir, roomType, "Room");    
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnRoomServerRpc(DoorDirection dir, string prefabName)
    {
        Debug.Log("SpawnRoomServerRpc");

        SpawnRoom(dir, prefabName);

    }

    private void SpawnRoom(DoorDirection dir, string prefabName)
    {
        Debug.Log("SpawnRoom");

        Transform roomObj = Resources.Load<Transform>(prefabName);
        roomObj = Instantiate(roomObj, transform);

        Vector3 newRoomPos = spawningRoom.gameObject.transform.position;
        float spaceShift = dir == DoorDirection.Up || dir == DoorDirection.Down ? roomHeight : roomWidth;
        newRoomPos += (DirectionToWorldSpace(dir) * spaceShift);
        roomObj.position = newRoomPos;

        roomObj.GetComponent<NetworkObject>().Spawn();
        roomToSpawn = roomObj;
    }


    public void InitiateNewRoom(DoorDirection dir, Room.RoomType roomType, string prefabName)
    {
        Debug.Log("InitiateNewRoom player.IsServer="+ player.IsServer);

        if (player.IsServer)
            SpawnRoom(dir, prefabName);
        else
            SpawnRoomServerRpc(dir, prefabName);

        Transform roomObj = roomToSpawn;

        //Establish new grid index and location of room
        Vector3 newGridIndex = spawningRoom.roomCoords;

        newGridIndex = newGridIndex + DirectionToGrid(dir);

        //Setup new room object
        Room newRoom = roomObj.GetComponent<Room>();

        dungeonGrid[(int)newGridIndex.x, (int)newGridIndex.y, (int)newGridIndex.z] = newRoom;

        newRoom.SetType(roomType);
        newRoom.roomCoords = newGridIndex;

        // Block doors to OOB
        foreach (Door door in newRoom.doors)
        {
            door.dungeon = this;
            if ((newRoom.roomCoords.x + DirectionToGrid(door.direction).x) < 0
                || (newRoom.roomCoords.x + DirectionToGrid(door.direction).x) > maxRoomRadius*2
                || (newRoom.roomCoords.y + DirectionToGrid(door.direction).y) < 0
                || (newRoom.roomCoords.y + DirectionToGrid(door.direction).y) > maxRoomRadius*2)
            {
                Debug.Log("Blocking door in room at: " + newRoom.roomCoords + " in direction " + Door.DirectionToGrid(door.direction));
                door.BlockDoor();
            }
        }

        //Remove door joining to entrance
        if (dir != DoorDirection.Up && dir != DoorDirection.Down)
        {
            //newRoom.doors[((int)dir + 2) % 4].RemoveDoor();
            Destroy(spawningRoom.doors[(int)dir].gameObject);
            spawningRoom.doors[(int)dir].GetComponent<NetworkObject>().Despawn();


            Destroy(newRoom.doors[((int)dir + 2) % 4].gameObject);
            newRoom.doors[((int)dir + 2) % 4].GetComponent<NetworkObject>().Despawn();
        }

        //Open doors to other adjacent rooms (leave Objective rooms as dead ends)
        if (roomType != Room.RoomType.Objective)
        {
            List<Room> neighbours = GetRoomNeighbours(newRoom);
            foreach (Room room in neighbours)
            {
                if (room != spawningRoom)
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

        lastSpawned = roomObj.GetComponent<Room>();

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

    //[ServerRpc(RequireOwnership = false)]
    //private GameObject SpawnRoomServerRpc(string prefabName)
    //{
    //    GameObject roomObj = Resources.Load<GameObject>(prefabName);

    //    GameObject newRoomObj = Instantiate(roomObj, gameObject.transform);
    //    newRoomObj.GetComponent<NetworkObject>().Spawn(true);
    //    return newRoomObj;
    //}
}
