using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerIndex;
    [SerializeField] private TextMeshProUGUI playerName;

    public void UpdatePlayerItem(int index, string name)
    {
        playerIndex.text = "#" + index;
        playerName.text = name;
    }
}
