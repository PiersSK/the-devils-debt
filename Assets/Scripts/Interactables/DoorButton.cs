using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorButton : Interactable
{
    [SerializeField] private Door door;
    public int manaCost;
    
    public Room.RoomType roomType;

    protected override void Interact()
    {
        if (manaCost <= Player.LocalInstance.playerMana.currentMana.Value)
        {
            Player.LocalInstance.playerMana.IncrementPlayerMana(-manaCost);
            door.RemoveDoor();
            door.dungeon.AddRandomRoom(door.room, door.direction, roomType);
        }
    }

    public override bool CanInteract()
    {
        return manaCost <= Player.LocalInstance.playerMana.currentMana.Value;
    }
}
