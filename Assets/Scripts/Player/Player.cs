using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player LocalInstance { get; private set; }

    public PlayerMotor playerMotor;
    public PlayerLook playerLook;
    public PlayerInteract playerInteract;
    public PlayerAttack playerAttack;
    public PlayerMana playerMana;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner) LocalInstance = this;
    }

    private void Start()
    {
        playerMotor = GetComponent<PlayerMotor>();
        playerLook = GetComponent<PlayerLook>();
        playerInteract = GetComponent<PlayerInteract>();
        playerAttack = GetComponent<PlayerAttack>();
        playerMana = GetComponent<PlayerMana>();
    }
}
