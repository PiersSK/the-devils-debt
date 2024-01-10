using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PuzzleRoom : Room
{
    public override Transform GetPrefab()
    {
        return Resources.Load<Transform>("Rooms/PuzzleRoom");
    }
}
