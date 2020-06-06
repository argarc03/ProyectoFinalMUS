using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    public List<Vector3> items = new List<Vector3>(); // time pos, freq, start/stop
    public TimeController timeControl;
    public string name = "";
    int i = 0;

    void Start()
    {
        
    }

    void Update()
    {
        playItems();

        if (timeControl.time <= 0.01f)
            i = 0;
    }

    void playItems()
    {
        if (i < items.Count && (timeControl.time <= items[i].x + 0.01f && timeControl.time >= items[i].x - 0.01f))
        {
            OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", items[i].z, -1, items[i].y);
            i++;
        }
    }
}
