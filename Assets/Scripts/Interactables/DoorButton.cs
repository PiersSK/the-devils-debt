using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorButton : Interactable
{
    [SerializeField] private Door door;
    
    public Room.RoomType roomType;

    protected override void Interact()
    {
        door.RemoveDoor();
        door.dungeon.AddRandomRoom(door.room, door.direction, roomType);
    }
}
