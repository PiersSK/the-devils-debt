using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DoorButton : Interactable
{
    [SerializeField] private Door door;
    
    public Room.RoomType roomType;

    protected override void Interact()
    {
        //door.RemoveDoor();
        //TestSpawn();
        //Destroy(door.gameObject);
        door.dungeon.AddRandomRoom(door.room, door.direction, roomType);
    }
}
