using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoController : MonoBehaviour
{
    public bool pianoActive = false;

    bool pianoPlaying = false;

    public GameObject timeBar;
    float time = 0f;
    float maxTime = 2f;

    public GameObject pianoItem;
    bool instantiateItem = false;
    int itemY = 0;

    void Start()
    {

    }

    void FixedUpdate()
    {
        if (pianoActive)
        {
            handleInput();
            createItem();
            updateTime();
        }
    }

    void createItem()
    {
        if (instantiateItem && (time % 0.1f) < 0.05f)
        {
            Vector3 itemPos = timeBar.transform.position;
            itemPos.y = itemY * 20 + 36;
            Instantiate(pianoItem, itemPos, Quaternion.identity, transform.parent);
        }
    }

    void updateTime()
    {
        time = (time + 0.01f) % maxTime;
        timeBar.transform.localPosition = new Vector3((time - 1) * 75, 0f, 0f);
    }

    void handleInput()
    {
        float DPADposX = Input.GetAxis("DPADHorizontal");
        float DPADposY = Input.GetAxis("DPADVertical");

        instantiateItem = true;

        if (DPADposX > 0) // F
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 349.23f);
                pianoPlaying = true;
                itemY = 3;
            }
        }
        else if (DPADposX < 0) // D
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 293.66f);
                pianoPlaying = true;
                itemY = 1;
            }
        }
        else if (DPADposY > 0) // C
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 261.63f);
                pianoPlaying = true;
                itemY = 0;
            }
        }
        else if (DPADposY < 0) // E
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 329.63f);
                pianoPlaying = true;
                itemY = 2;
            }
        }
        else if (Input.GetButton("Button Y")) // G
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 392f);
                pianoPlaying = true;
                itemY = 4;
            }
        }
        else if (Input.GetButton("Button X")) // A
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 440f);
                pianoPlaying = true;
                itemY = 5;
            }
        }
        else if (Input.GetButton("Button A")) // B
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 493.88f);
                pianoPlaying = true;
                itemY = 6;
            }
        }
        else if (Input.GetButton("Button B")) // C2
        {
            if (!pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 523.25f);
                pianoPlaying = true;
                itemY = 7;
            }
        }
        else
        {
            if (pianoPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", -1.0, -1, 0);
                pianoPlaying = false;
            }

            instantiateItem = false;
        }
    }
}
