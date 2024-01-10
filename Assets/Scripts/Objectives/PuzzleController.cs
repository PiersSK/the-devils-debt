using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PuzzleController : MonoBehaviour
{
    public static PuzzleController Instance { get; private set; }

    public event EventHandler OnPuzzleComplete;

    [Range(1, 5)]
    [SerializeField] private int combinationLength = 3;

    private List<int> solution = new(); // Sync
    private List<int> playerInput = new(); // Sync

    private List<int> runeCluesSpawned = new();

    private bool puzzleComplete = false; // Sync

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Dungeon.Instance.OnRoomSpawn += Instance_OnRoomSpawn;

        do
        {
            int val = Random.Range(1, 6);
            if(!solution.Contains(val)) solution.Add(val);
        }
        while (solution.Count < combinationLength);

        solution.Sort();
        string combinationString = "";
        foreach (int val in solution) combinationString += val.ToString()+ " ";
        Debug.Log("Solution to puzzle is " + combinationString);
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
            wallRune.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("RuneSymbols/Rune" + runeToSpawn);

            wallRune.parent = e.RoomTransform;
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

            wallRune.localPosition = runePos;
            wallRune.localEulerAngles = runeRot;

            runeCluesSpawned.Add(runeToSpawn);
        }
        
    }

    private void Update()
    {
        if (!puzzleComplete)
        {
            playerInput.Sort();

            if (solution.SequenceEqual(playerInput))
            {
                UIManager.Instance.notification.ShowNotification("Puzzle Complete", Color.blue);
                puzzleComplete = true;
                OnPuzzleComplete?.Invoke(this, new EventArgs());
            }
        }
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
