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

    List<GameObject> itemsGO = new List<GameObject>();

    List<Vector3> items = new List<Vector3>(); // time pos, freq, start/stop
    Vector3 item;
    int i = 0;

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
            playItems();
        }
    }

    public void undo()
    {
        if (items.Count != 0)
        {
            // destroy the start and stop of the
            // last item added
            items.RemoveAt(items.Count - 1);
            items.RemoveAt(items.Count - 1);

            // remove all the instances of the last item added
            if (itemsGO.Count != 0)
            {
                int k = itemsGO.Count - 1;
                float posY = itemsGO[k].transform.position.y;

                while (k >= 0 && itemsGO.Count != 0 && itemsGO[k].transform.position.y == posY)
                {
                    Destroy(itemsGO[k]);
                    itemsGO.RemoveAt(k);
                    k--;
                }
            }

        }
    }

    public void cleanAll()
    {
        if (items.Count != 0)
        {
            pianoPlaying = false;

            OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", -1, -1, -1);
            items.Clear();
            foreach (GameObject go in itemsGO)
                Destroy(go);
            itemsGO.Clear();
        }
    }

    public void deactivatePiano()
    {
        pianoActive = false;
        cleanAll();
    }

    void playItems()
    {
        if (i < items.Count && (time <= items[i].x + 0.01f && time >= items[i].x - 0.01f))
        {
            OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", items[i].z, -1, items[i].y);
            i++;
        }
        //if (i < items.Count) { print(items[i].x); print(time); }
    }

    void createItem()
    {
        if (instantiateItem && (time % 0.1f) < 0.05f)
        {
            Vector3 itemPos = timeBar.transform.position;
            itemPos.y = itemY * 20 + 36;
            itemsGO.Add(Instantiate(pianoItem, itemPos, Quaternion.identity, transform.parent));
        }
    }

    void updateTime()
    {
        time = (time + 0.01f) % maxTime;
        if (time <= 0.01f)
        {
            i = 0;
            print("time == 0");
        }
        timeBar.transform.localPosition = new Vector3((time - 1) * 75, 0f, 0f);
    }

    void handleInput()
    {
        float DPADposX = Input.GetAxis("DPADHorizontal");
        float DPADposY = Input.GetAxis("DPADVertical");

        instantiateItem = true;

        item.x = time;

        if (DPADposX > 0) // F
        {
            if (!pianoPlaying)
            {
                //OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 349.23f);
                pianoPlaying = true;
                itemY = 3;
                item.y = 349.23f;
                item.z = 1;
                items.Add(item);
            }
        }
        else if (DPADposX < 0) // D
        {
            if (!pianoPlaying)
            {
                //OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 293.66f);
                pianoPlaying = true;
                itemY = 1;
                item.y = 293.66f;
                item.z = 1;
                items.Add(item);
            }
        }
        else if (DPADposY > 0) // C
        {
            if (!pianoPlaying)
            {
                //OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 261.63f);
                pianoPlaying = true;
                itemY = 0;
                item.y = 261.63f;
                item.z = 1;
                items.Add(item);
            }
        }
        else if (DPADposY < 0) // E
        {
            if (!pianoPlaying)
            {
                //OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 329.63f);
                pianoPlaying = true;
                itemY = 2;
                item.y = 329.63f;
                item.z = 1;
                items.Add(item);
            }
        }
        else if (Input.GetButton("Button Y")) // G
        {
            if (!pianoPlaying)
            {
                //OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 392f);
                pianoPlaying = true;
                itemY = 4;
                item.y = 392f;
                item.z = 1;
                items.Add(item);
            }
        }
        else if (Input.GetButton("Button X")) // A
        {
            if (!pianoPlaying)
            {
                //OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 440f);
                pianoPlaying = true;
                itemY = 5;
                item.y = 440f;
                item.z = 1;
                items.Add(item);
            }
        }
        else if (Input.GetButton("Button A")) // B
        {
            if (!pianoPlaying)
            {
                //OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 493.88f);
                pianoPlaying = true;
                itemY = 6;
                item.y = 493.88f;
                item.z = 1;
                items.Add(item);
            }
        }
        else if (Input.GetButton("Button B")) // C2
        {
            if (!pianoPlaying)
            {
                //OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", 1.0, -1, 523.25f);
                pianoPlaying = true;
                itemY = 7;
                item.y = 523.25f;
                item.z = 1;
                items.Add(item);
            }
        }
        else
        {
            if (pianoPlaying)
            {
                //OSCHandler.Instance.SendMessageToClient("SuperCollider", "/piano", -1.0, -1, 0);
                pianoPlaying = false;

                item.z = -1;
                items.Add(item);
            }

            instantiateItem = false;
        }
    }
}
