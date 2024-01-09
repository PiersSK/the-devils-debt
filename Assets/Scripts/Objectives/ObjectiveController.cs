using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveController : NetworkBehaviour
{
    public static ObjectiveController Instance { get; private set; }

    public enum ObjectiveType
    {
        Keys
        , Monsters
        //,Puzzle
    }

    private List<string> objectiveMessages = new List<string>
    {
        "Find Keys" //Keys
        ,"Slay Enemies" //Monsters
    };

    public NetworkVariable<ObjectiveType> objectiveSelected;
    public bool objectiveComplete = false;
    public float objectiveGoal = 3f;
    private NetworkVariable<float> objectiveProgress = new NetworkVariable<float>(0f);

    public float timeLimit = 60f;
    private NetworkVariable<float> timeRemaining = new(60f);
    private bool timeLimitReached = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer) return;

        if (!timeLimitReached)
        {
            timeRemaining.Value -= Time.deltaTime;
            if (timeRemaining.Value <= 0)
            {
                timeLimitReached = true;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        objectiveProgress.OnValueChanged += ProgressChanged;
        timeRemaining.OnValueChanged += TimeSync;

        UIManager.Instance.timer.gameObject.SetActive(true);

        if (IsServer)
        {
            SetObjective();
            timeRemaining.Value = timeLimit;
        }
        else
        {
            UIManager.Instance.objectiveText.text = objectiveMessages[(int)objectiveSelected.Value]; //Assumes host joins first and has already set NVs
            UIManager.Instance.timer.text = TimeRemainingAsString();

        }

    }

    private string TimeRemainingAsString()
    {
        TimeSpan ts = TimeSpan.FromSeconds(timeRemaining.Value);
        return string.Format("{0:00}:{1:00}", ts.TotalMinutes, ts.Seconds);
    }

    private void TimeSync(float prevVal, float newVal)
    {
        timeRemaining.Value = newVal;
        UIManager.Instance.timer.text = TimeRemainingAsString();
        UIManager.Instance.timer.color = (timeRemaining.Value / timeLimit) > 0.2 ? Color.white : Color.red;
    }

    private void ProgressChanged(float prevVal, float newVal)
    {
        objectiveProgress.Value = newVal;

        UIManager.Instance.objectiveBar.fillAmount = objectiveProgress.Value / objectiveGoal;

        if (objectiveProgress.Value == objectiveGoal)
            objectiveComplete = true;
    }

    public void ShowObjectiveUI()
    {
        UIManager.Instance.objectiveUI.SetActive(true);
    }

    public void ProgressObjective()
    {
        ProgressObjectiveServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ProgressObjectiveServerRpc()
    {
        if (!objectiveComplete)
            objectiveProgress.Value++;
    }


    private void SetObjective()
    {
        int objectiveOptions = Enum.GetValues(typeof(ObjectiveType)).Cast<int>().Max();
        objectiveSelected.Value = (ObjectiveType)UnityEngine.Random.Range(0, objectiveOptions+1);
        UIManager.Instance.objectiveText.text = objectiveMessages[(int)objectiveSelected.Value];

    }

    public static ObjectiveController GetObjectiveController()
    {
        return GameObject.Find("ObjectiveSystem").GetComponent<ObjectiveController>();
    }
}
