using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentParent : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
