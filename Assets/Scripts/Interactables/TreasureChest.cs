using Unity.Netcode;
using UnityEngine;

public class TreasureChest : NetworkInteractable
{
    [SerializeField] private Animator anim;
    protected NetworkVariable<bool> isOpen = new(false);
    private Vector3 lootOffset = new(0, 0.8f, 0.6f); // Do this better (make chest mid point actual position)

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isOpen.OnValueChanged += ToggleOpen;
    }

    private void ToggleOpen(bool prevVal, bool newVal)
    {
        Debug.Log("toggling chest state");
        isOpen.Value = newVal;
        if(isOpen.Value)
        {
            anim.SetTrigger("OpenChest");
            SetPromptMessage(string.Empty);
        }
    }

    protected override void Interact()
    {
        Debug.Log("Chest interact happening");
        OpenChestServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OpenChestServerRpc()
    {
        Debug.Log("Chest serverRPC happening");
        if (!isOpen.Value) {
            isOpen.Value = true;

            if (ObjectiveController.Instance.objectiveSelected.Value == ObjectiveController.ObjectiveType.Keys
                && !ObjectiveController.Instance.objectiveComplete) {
                //ObjectiveController.Instance.ProgressObjective();
                Transform keyObj = Instantiate(Resources.Load<Transform>("Pickups/ObjectiveKey"));
                keyObj.GetComponent<PickupInteractable>().UpdateRootPosition(transform.position + lootOffset);
                keyObj.GetComponent<NetworkObject>().Spawn();
            }
            else
            {
                SpawnLoot("Equipment/Sword2", 10);
            }


            SpawnPickup("Pickups/HealthOrb", 7);
            SpawnPickup("Pickups/ManaOrb", 3);
        }
    }


    private void SpawnLoot(string prefabName, int spawnChance)
    {
        if (Random.Range(0, 10) < spawnChance)
        {
            Transform loot = Resources.Load<Transform>(prefabName);
            loot.position = transform.position + lootOffset;
            Transform lootObj = Instantiate(loot);
            lootObj.GetComponent<NetworkObject>().Spawn();
        }
    }

    private void SpawnPickup(string prefabName, int spawnChance)
    {
        //Health Orb spawn
        if (UnityEngine.Random.Range(0, 10) < spawnChance)
        {
            Transform healthOrb = Resources.Load<Transform>(prefabName);
            healthOrb.position = new Vector3(
                transform.position.x + UnityEngine.Random.Range(-1f, 1f)
                , -1.6f
                , transform.position.z + UnityEngine.Random.Range(-1f, 1f)
            );
            Transform healthOrbObj = Instantiate(healthOrb);
            healthOrbObj.GetComponent<NetworkObject>().Spawn();
        }
}
}
