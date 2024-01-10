using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PuzzleController : NetworkBehaviour
{
    public static PuzzleController Instance { get; private set; }

    public event EventHandler OnPuzzleComplete;

    private int combinationLength = 3;

    private List<int> solution = new();
    private List<int> playerInput = new();

    private List<int> runeCluesSpawned = new();

    private bool puzzleComplete = false;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (IsServer)
        {
            Dungeon.Instance.OnRoomSpawn += Instance_OnRoomSpawn;

            do
            {
                int val = Random.Range(1, 6);
                if (!solution.Contains(val)) solution.Add(val);
            }
            while (solution.Count < combinationLength);

            solution.Sort();
            SetupSolutionClientRpc(solution[0], solution[1], solution[2]);
        }
    }

    private void Update()
    {
        if (!puzzleComplete && IsServer) // Only server tracks progress
        {
            playerInput.Sort();

            if (solution.SequenceEqual(playerInput))
            {
                CompletePuzzleClientRpc();
                ObjectiveController.Instance.ProgressObjective();
            }
        }
    }

    // NOTE: This hardcodes the puzzle to being a solution of 3 runes.
    [ClientRpc]
    private void SetupSolutionClientRpc(int sol1, int sol2, int sol3)
    {
        solution = new() { sol1, sol2, sol3 };
    }

    [ClientRpc]
    private void CompletePuzzleClientRpc()
    {
        UIManager.Instance.notification.ShowNotification("Puzzle Complete", Color.blue);
        puzzleComplete = true;
        OnPuzzleComplete?.Invoke(this, new EventArgs());
    }

    private void Instance_OnRoomSpawn(object sender, Dungeon.OnRoomSpawnArgs e)
    {
        Debug.Log("Checking rune spawn on room spawn");
        List<int> cluesToSpawn = new(solution);
        foreach (int rune in runeCluesSpawned) cluesToSpawn.Remove(rune);

        if (cluesToSpawn.Count > 0 && Random.Range(0, 100) < 50)
        {

            int runeToSpawn = cluesToSpawn[Random.Range(0, cluesToSpawn.Count)];

            Transform wallRune = Instantiate(Resources.Load<Transform>("RuneSymbols/WallRune"));
            wallRune.GetComponent<NetworkObject>().Spawn();
            SyncWallRuneImgClientRpc(wallRune.GetComponent<NetworkObject>(), runeToSpawn);

            bool isEastWest = Random.Range(0, 2) == 0;
            Vector3 runePos = Vector3.zero;
            Vector3 runeRot = Vector3.zero;

            runePos.y = Random.Range(-1, 3);

            if (isEastWest)
            {
                runePos.x = (Random.Range(0, 2) * 2 - 1) * 9.8f;
                runePos.z = (Random.Range(0, 2) * 2 - 1) * Random.Range(3, 9);
                runeRot.y = 90f;
            }
            else {
                runePos.x = (Random.Range(0, 2) * 2 - 1) * Random.Range(3, 9);
                runePos.z = (Random.Range(0, 2) * 2 - 1) * 9.8f;
            }

            wallRune.localPosition = e.RoomTransform.position + runePos;
            wallRune.localEulerAngles = runeRot;

            runeCluesSpawned.Add(runeToSpawn);
        }
        
    }

    [ClientRpc]
    private void SyncWallRuneImgClientRpc(NetworkObjectReference runeNOR, int index)
    {
        runeNOR.TryGet(out NetworkObject runeNO);
        runeNO.GetComponent<WallRune>().SetRuneImage(index - 1);

    }



    public void AddToPlayerInput(int input)
    {
        playerInput.Add(input);
    }

    public void RemoveFromPlayerInput(int input)
    {
        playerInput.Remove(input);
    }
}
