using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum DrumID
{
    CRASH_CYMBAL, FLOOR_TOM, HIGH_TOM, HI_HAT, KICK, MEDIUM_TOM, RIDE_CYMBAL, SNARE
}

public class DrumController : MonoBehaviour
{
    bool drumPlaying = false;

    public GameObject timeBar;
    float time = 0f;
    float maxTime = 2f;

    public GameObject drumItem;
    bool instantiateItem = false;
    int itemY = 0;

    void Start()
    {
        print("CONTROLLING DRUMS");
    }

    void FixedUpdate()
    {
        handleInput();
        createItem();
        updateTime();
    }

    void createItem()
    {
        if (instantiateItem && (time % 0.1f) < 0.05f)
        {
            Vector3 itemPos = timeBar.transform.position;
            itemPos.y = itemY * 20 + 36;
            Instantiate(drumItem, itemPos, Quaternion.identity, transform.parent);
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

        if (DPADposX > 0) // HI HAT
        {
            if (!drumPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/drumPrev", 1.0, (int)DrumID.HI_HAT);
                drumPlaying = true;
                itemY = 3;
            }
        }
        else if (DPADposX < 0) // CRASH CYMBAL
        {
            if (!drumPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/drumPrev", 1.0, (int)DrumID.CRASH_CYMBAL);
                drumPlaying = true;
                itemY = 1;
            }
        }
        else if (DPADposY > 0) // HIGH TOM
        {
            if (!drumPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/drumPrev", 1.0, (int)DrumID.HIGH_TOM);
                drumPlaying = true;
                itemY = 0;
            }
        }
        else if (DPADposY < 0) // SNARE DRUM
        {
            if (!drumPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/drumPrev", 1.0, (int)DrumID.SNARE);
                drumPlaying = true;
                itemY = 2;
            }
        }
        else if (Input.GetButton("Button Y")) // MEDIUM TOM
        {
            if (!drumPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/drumPrev", 1.0, (int)DrumID.MEDIUM_TOM);
                drumPlaying = true;
                itemY = 4;
            }
        }
        else if (Input.GetButton("Button X")) // BASS DRUM (KICK)
        {
            if (!drumPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/drumPrev", 1.0, (int)DrumID.KICK);
                drumPlaying = true;
                itemY = 5;
            }
        }
        else if (Input.GetButton("Button A")) // FLOOR TOM
        {
            if (!drumPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/drumPrev", 1.0, (int)DrumID.FLOOR_TOM);
                drumPlaying = true;
                itemY = 6;
            }
        }
        else if (Input.GetButton("Button B")) // RIDE CYMBAL
        {
            if (!drumPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/drumPrev", 1.0, (int)DrumID.RIDE_CYMBAL);
                drumPlaying = true;
                itemY = 7;
            }
        }
        else
        {
            if (drumPlaying)
            {
                OSCHandler.Instance.SendMessageToClient("SuperCollider", "/drum", -1.0, 0);
                drumPlaying = false;
            }

            instantiateItem = false;
        }
    }
}
