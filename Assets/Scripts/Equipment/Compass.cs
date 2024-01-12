using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Compass : Equipment
{
    [SerializeField] private Transform compassNeedle;
    [SerializeField] private int manaCost;

    private enum CompassDestination
    {
        Objective,
        PuzzleRoom
    }

    private CompassDestination destination = CompassDestination.Objective;
    private List<string> destinationName = new() { "Objective", "Puzzle Room" };

    public bool orbIsActive = false;

    private void Update()
    {
        if (isPickedUp) // Point it north (ish)
            compassNeedle.localEulerAngles = -Player.LocalInstance.transform.eulerAngles;

    }

    public override void PerformAbility()
    {
        Door.DoorDirection compassResponse = Door.DoorDirection.None;
        if (destination == CompassDestination.Objective)
            compassResponse = Dungeon.Instance.GetPathToObjective();
        else if (destination == CompassDestination.PuzzleRoom)
            compassResponse = Dungeon.Instance.GetPathToPuzzle();

        bool atDestination = compassResponse == Door.DoorDirection.None;

        if (!atDestination && !onCooldown && Player.LocalInstance.playerMana.IncrementPlayerMana(-manaCost))
        {
            SendProjectileServerRpc(Player.LocalInstance.GetComponent<NetworkObject>(), compassResponse);
            UIManager.Instance.hotbarAccessory.PutOnCooldown(1f);
        }
    }

    public override void ResetAbility() { }
    public override void SetAnimations() { }

    [ServerRpc(RequireOwnership = false)]
    private void SendProjectileServerRpc(NetworkObjectReference playerNOR, Door.DoorDirection dir)
    {
        playerNOR.TryGet(out NetworkObject playerNO);
        Player player = playerNO.GetComponent<Player>();

        Transform orbPrefab = Resources.Load<Transform>("Projectiles/CompassProjectile");
        orbPrefab.position = player.playerLook.cam.transform.position + (player.playerLook.cam.transform.forward * 2);

        Transform orbObj = Instantiate(orbPrefab);
        orbObj.GetComponent<NetworkObject>().Spawn();

        SendProjectileClientRpc(orbObj.GetComponent<NetworkObject>(), playerNOR, dir);

    }

    [ClientRpc]
    private void SendProjectileClientRpc(NetworkObjectReference orbNOR, NetworkObjectReference playerNOR, Door.DoorDirection dir)
    {
        orbNOR.TryGet(out NetworkObject orbNO);
        playerNOR.TryGet(out NetworkObject playerNO);

        Transform doorTarget = Dungeon.Instance.GetRoomOfPlayer(playerNO.GetComponent<Player>()).doors[(int)dir].transform;
        Debug.Log("Setting orb target to : " + doorTarget.name);
        orbNO.GetComponent<CompassProjectile>().target = doorTarget;
    }

    public override void PerformAlt()
    {
        int objectiveOptions = Enum.GetValues(typeof(CompassDestination)).Cast<int>().Max();
        if ((int)destination < objectiveOptions)
            destination = (CompassDestination)((int)destination + 1);
        else
            destination = 0;

        UpdateAltUI();

    }

    public override void UpdateAltUI()
    {
        UIManager.Instance.equipmentPrompt.text = "[F] Toggle Destination";
        UIManager.Instance.equipmentDetails.text = "Destination: " + destinationName[(int)destination];

        UIManager.Instance.equipmentPrompt.gameObject.SetActive(true);
        UIManager.Instance.equipmentDetails.gameObject.SetActive(true);
    }
}
