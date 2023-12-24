using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorButton : Interactable
{
    [SerializeField] private Door door;
    [SerializeField] private int manaCost;
    
    public Room.RoomType roomType;

    protected override void Interact()
    {
        if (manaCost <= Player.LocalInstance.playerMana.CurrentMana)
        {
            Player.LocalInstance.playerMana.CurrentMana -= manaCost;
            door.RemoveDoor();
            door.dungeon.AddRandomRoom(door.room, door.direction, roomType);
        }
    }
}
