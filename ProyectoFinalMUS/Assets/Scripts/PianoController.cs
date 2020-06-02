using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoController : MonoBehaviour
{
    bool pianoPlaying = false;

    public GameObject timeBar;
    float time = 0f;
    float maxTime = 2f;

    void Start()
    {
        
    }

    void Update()
    {
        handleInput();
        updateTime();
    }

    void updateTime()
    {
        time = (time + 0.01f) % maxTime;
        timeBar.transform.localPosition = new Vector3((time-1)*75, 0f, 0f);
    }

    void handleInput()
    {
        float DPADposX = Input.GetAxis("DPADHorizontal");
        float DPADposY = Input.GetAxis("DPADVertical");

        if (DPADposX > 0) // F
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, 349.23f);
                pianoPlaying = true;
            }
        }
        else if (DPADposX < 0) // D
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, 293.66f);
                pianoPlaying = true;
            }
        }
        else if (DPADposY > 0) // C
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, 261.63f);
                pianoPlaying = true;
            }
        }
        else if (DPADposY < 0) // E
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, 329.63f);
                pianoPlaying = true;
            }
        }
        else if (Input.GetButton("Button Y")) // G
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, 392f);
                pianoPlaying = true;
            }
        }
        else if (Input.GetButton("Button X")) // A
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, 440f);
                pianoPlaying = true;
            }
        }
        else if (Input.GetButton("Button A")) // B
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, 493.88f);
                pianoPlaying = true;
            }
        }
        else if (Input.GetButton("Button B")) // C2
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, 523.25f);
                pianoPlaying = true;
            }
        }
        else if (pianoPlaying)
        {
            OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", -1.0, 0);
            pianoPlaying = false;
        }
    }
}
