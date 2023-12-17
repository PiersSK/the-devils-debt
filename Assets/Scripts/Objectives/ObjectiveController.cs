using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveController : MonoBehaviour
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
    private float objectiveProgress = 0f;

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
        if (!objectiveComplete)
        {
            objectiveProgress++;
            objectiveBar.fillAmount =  objectiveProgress / objectiveGoal;
            Debug.Log("progress now " +  objectiveProgress + "/" + objectiveGoal);
        }
        
        if (objectiveProgress == objectiveGoal)
            objectiveComplete = true;
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
