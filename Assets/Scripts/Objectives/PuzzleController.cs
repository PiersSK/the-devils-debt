using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PuzzleController : MonoBehaviour
{
    public static PuzzleController Instance { get; private set; }

    public event EventHandler OnPuzzleComplete;

    [Range(1, 5)]
    [SerializeField] private int combinationLength = 3;

    private List<int> solution = new();
    private List<int> playerInput = new();

    private bool puzzleComplete = false;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < combinationLength; i++)
        {
            solution.Add(Random.Range(1, 6));
        }

        solution.Sort();
        string combinationString = "";
        foreach (int val in solution) combinationString += val.ToString()+ " ";
        Debug.Log("Solution to puzzle is " + combinationString);
    }

    private void Update()
    {
        if (!puzzleComplete)
        {
            playerInput.Sort();

            if (solution.SequenceEqual(playerInput))
            {
                Debug.Log("Puzzle completed!");
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
