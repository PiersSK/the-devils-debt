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
        ,Monsters
        ,Puzzle
    }

    private List<string> objectiveMessages = new List<string>
    {
        "Find Keys" //Keys
        ,"Slay Enemies" //Monsters
        ,"Solve The Puzzle Room" //Puzzle
    };

    public NetworkVariable<ObjectiveType> objectiveSelected;
    public bool objectiveComplete = false;
    private NetworkVariable<float> objectiveGoal = new(3f);
    private NetworkVariable<float> objectiveProgress = new NetworkVariable<float>(0f);

    [Range(10,3600)]
    public float timeLimit = 60f;

    private NetworkVariable<float> timeRemaining = new(0f);
    private bool timeLimitReached = false;
    private float lowTimeStart = 10f;

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
                timeRemaining.Value = 0;
                timeLimitReached = true;
                GameOverSystem.Instance.GameOverServerRpc(false);
            } 
        }
    }

    public override void OnNetworkSpawn()
    {
        objectiveProgress.OnValueChanged += ProgressChanged;
        objectiveGoal.OnValueChanged += GoalSync;
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
        return string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
    }

    private void GoalSync(float prevVal, float newVal)
    {
        objectiveGoal.Value = newVal;
    }

    private void TimeSync(float prevVal, float newVal)
    {
        timeRemaining.Value = newVal;
        if (timeRemaining.Value > 0)
        {
            // Update UI
            UIManager.Instance.timer.text = TimeRemainingAsString();
            UIManager.Instance.timer.color = timeRemaining.Value > lowTimeStart ? Color.white : Color.red;
            // Update Camera shake
            Player.LocalInstance.playerLook.cameraShake = timeRemaining.Value <= lowTimeStart;
            Player.LocalInstance.playerLook.cameraShakeMagnitude = (lowTimeStart - timeRemaining.Value) / (lowTimeStart * 2);
        } else
        {
            // Update UI
            UIManager.Instance.timer.text = "00:00";
            UIManager.Instance.timer.color = timeRemaining.Value > lowTimeStart ? Color.white : Color.red;
            // Update Camera shake
            Player.LocalInstance.playerLook.cameraShake = false;
            Player.LocalInstance.playerLook.cameraShakeMagnitude = 0f;
        }
    }

    private void ProgressChanged(float prevVal, float newVal)
    {
        objectiveProgress.Value = newVal;

        UIManager.Instance.objectiveBar.fillAmount = objectiveProgress.Value / objectiveGoal.Value;

        if (objectiveProgress.Value == objectiveGoal.Value)
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

        if (objectiveSelected.Value == ObjectiveType.Puzzle) objectiveGoal.Value = 1f;

    }

    public static ObjectiveController GetObjectiveController()
    {
        return GameObject.Find("ObjectiveSystem").GetComponent<ObjectiveController>();
    }
}
