using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    
    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            float move = 1.0f;
            OSCHandler.Instance.SendMessageToClient("SuperCollider", "/move", move);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            float food = 1.0f;
            OSCHandler.Instance.SendMessageToClient("SuperCollider", "/food", food);
            print("New synth created");
        }
    }
}
