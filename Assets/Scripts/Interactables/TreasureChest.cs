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

    public override bool CanInteract()
    {
        return !isOpen.Value;
    }

    protected override void Interact()
    {
        OpenChestServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OpenChestServerRpc()
    {
        if (!isOpen.Value) {
            isOpen.Value = true;

            if (ObjectiveController.Instance.objectiveSelected.Value == ObjectiveController.ObjectiveType.Keys
                && !ObjectiveController.Instance.objectiveComplete) {
                Transform keyObj = Instantiate(Resources.Load<Transform>("Pickups/ObjectiveKey"));
                keyObj.GetComponent<PickupInteractable>().UpdateRootPosition(transform.position + lootOffset);
                keyObj.GetComponent<NetworkObject>().Spawn(true);
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
            lootObj.GetComponent<NetworkObject>().Spawn(true);
        }
    }

    private void SpawnPickup(string prefabName, int spawnChance)
    {
        //Health Orb spawn
        if (Random.Range(0, 10) < spawnChance)
        {
            Transform healthOrb = Resources.Load<Transform>(prefabName);
            healthOrb.position = new Vector3(
                transform.position.x + Random.Range(-1f, 1f)
                , -1.6f
                , transform.position.z + Random.Range(-1f, 1f)
            );
            Transform healthOrbObj = Instantiate(healthOrb);
            healthOrbObj.GetComponent<NetworkObject>().Spawn(true);
        }
}
}
