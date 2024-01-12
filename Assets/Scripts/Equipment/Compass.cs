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
    private Transform currentObjectiveOrb;
    private Transform currentDoorTarget;

    private void Update()
    {
        if (isPickedUp) // Point it north (ish)
            compassNeedle.localEulerAngles = -Player.LocalInstance.transform.eulerAngles;

    }

    public override void PerformAbility()
    {
        if (!onCooldown && Player.LocalInstance.playerMana.IncrementPlayerMana(-manaCost))
        {
            Door.DoorDirection compassResponse = Door.DoorDirection.None;
            if (destination == CompassDestination.Objective)
                compassResponse = Dungeon.Instance.GetPathToObjective();
            else if (destination == CompassDestination.PuzzleRoom)
                compassResponse = Dungeon.Instance.GetPathToPuzzle();

            string responseString = compassResponse.ToString();

            if (compassResponse == Door.DoorDirection.None) responseString = "Destination Reached";
            else
            {
                currentDoorTarget = Dungeon.Instance.GetRoomOfPlayer().doors[(int)compassResponse].transform;
                SendProjectileServerRpc(Player.LocalInstance.GetComponent<NetworkObject>());
            }

            Color nofifcationColor = destination == CompassDestination.Objective ? new Color(1, 0.8f, 0.05f) : new Color(0.55f, 0.25f, 0.66f); 

            UIManager.Instance.notification.ShowNotification(responseString, nofifcationColor);
            UIManager.Instance.hotbarAccessory.PutOnCooldown(1f);
                

        }
    }

    public override void ResetAbility() { }
    public override void SetAnimations() { }

    [ServerRpc(RequireOwnership = false)]
    private void SendProjectileServerRpc(NetworkObjectReference playerNOR)
    {
        playerNOR.TryGet(out NetworkObject playerNO);
        Player player = playerNO.GetComponent<Player>();

        Transform orbPrefab = Resources.Load<Transform>("Projectiles/CompassProjectile");
        orbPrefab.position = player.playerLook.cam.transform.position + (player.playerLook.cam.transform.forward * 2);

        currentObjectiveOrb = Instantiate(orbPrefab);

        currentObjectiveOrb.GetComponent<CompassProjectile>().playerSourceNO = player.GetComponent<NetworkObject>();
        currentObjectiveOrb.GetComponent<CompassProjectile>().hasBeenfired = true;
        currentObjectiveOrb.GetComponent<CompassProjectile>().target = currentDoorTarget;

        currentObjectiveOrb.GetComponent<NetworkObject>().Spawn();

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
