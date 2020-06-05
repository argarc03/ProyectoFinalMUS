using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public float time = 0f;
    public float maxTime = 2f;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        time = (time + 0.01f) % maxTime;
    }
}
