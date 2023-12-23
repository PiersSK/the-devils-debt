using System;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveController : NetworkBehaviour
{
    public enum ObjectiveType
    {
        Keys
        //,Monsters
        //,Puzzle
    }

    [SerializeField] GameObject objectiveUI;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private Image objectiveBar;

    public ObjectiveType objectiveSelected;
    public bool objectiveComplete = false;
    public float objectiveGoal = 3f;
    private NetworkVariable<float> objectiveProgress = new NetworkVariable<float>(0f);

    public override void OnNetworkSpawn()
    {
        objectiveProgress.OnValueChanged += ProgressChanged;
        base.OnNetworkSpawn();
    }

    private void ProgressChanged(float prevVal, float newVal)
    {
        Debug.Log("objectiveProgress changed from " + prevVal + " to " + newVal);
        objectiveProgress.Value = newVal;

        objectiveBar.fillAmount = objectiveProgress.Value / objectiveGoal;
        Debug.Log("progress now " + objectiveProgress.Value + "/" + objectiveGoal);

        if (objectiveProgress.Value == objectiveGoal)
            objectiveComplete = true;
    }

    private void Start()
    {
        SetObjective();
    }

    public void ShowObjectiveUI()
    {
        objectiveUI.SetActive(true);
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
        objectiveSelected = (ObjectiveType)UnityEngine.Random.Range(0, objectiveOptions+1);

        switch(objectiveSelected)
        {
            case ObjectiveType.Keys:
                objectiveText.text = "Objective: Find Keys";
                break;
        }
    } 

    public static ObjectiveController GetObjectiveController()
    {
        return GameObject.Find("ObjectiveSystem").GetComponent<ObjectiveController>();
    }
}
