using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSysManager : MonoBehaviour
{
    public Transform networkManagerObj;

    public bool relayConnected = false;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);

    }

    public void SpawnPlayer()
    {
        
    }
}
