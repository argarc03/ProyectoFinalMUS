using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSCCall : MonoBehaviour
{
    void Start()
    {
        OSCHandler.Instance.Init();
    }

    void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        OSCHandler.Instance.Stop();
        OSCHandler.Instance.Destroy();
    }
}
