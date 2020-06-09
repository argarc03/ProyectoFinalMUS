using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class RecordingController : MonoBehaviour
{
    public bool pianoActive = false;
    bool pianoPlaying = false;
    public string name = ""; //usar esto cuando se añadan varios instrumentos!!!!

    public GameObject timeBar;

    public GameObject pianoItem;
    bool instantiateItem = false;
    int itemY = 0;

    List<GameObject> itemsGO = new List<GameObject>();

    public List<Vector3> items = new List<Vector3>(); // time pos, freq, start/stop
    Vector3 item;
    int i = 0;

    List<List<GameObject>> itemsGOSets = new List<List<GameObject>>();

    public TimeController timeControl;

    GameObject soundObj;
    public SoundObjectManager soundObjManager;

    public List<float> values;

    // Data of each instrument:
    [Header("Instrument Data:")]
    public InstrumentData sinData; // SIN
    public InstrumentData squareData; // SQUARE
    public InstrumentData percussionData; // PERCUSSION
    public InstrumentData pianoData; // PIANO

    public Image icon;
    public Text text;

    public AudioClip cancelClip;
    public AudioClip undoClip;
    public AudioClip cleanClip;
    AudioSource audioUI;

    void Start()
    {
        audioUI = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if (pianoActive)
        {
            handleInput();
            createItem();
            playItems();
            updateTime();
        }
    }

    public void init(string insName, List<GameObject> instrumentPrefabs)
    {
        pianoActive = true;
        InstrumentData data = null;
        switch (insName)
        {
            case "/sin":
                data = sinData;
                text.text = "WAVE";
                soundObj = instrumentPrefabs[6];
                break;
            case "/square":
                data = squareData;
                text.text = "WAVE";
                soundObj = instrumentPrefabs[6];
                break;
            case "/drums":
                data = percussionData;
                text.text = "DRUMKIT";
                soundObj = instrumentPrefabs[2];
                break;
            case "/piano":
                data = pianoData;
                text.text = "PIANO";
                soundObj = instrumentPrefabs[1];
                break;
            case "/flute":
                data = sinData;
                data.instrumentName = "/flute";
                text.text = "FLUTE";
                soundObj = instrumentPrefabs[5];
                break;
            case "/guitar":
                data = sinData;
                data.instrumentName = "/guitar";
                text.text = "GUITAR";
                soundObj = instrumentPrefabs[4];
                break;
            case "/violin":
                data = sinData;
                data.instrumentName = "/violin";
                text.text = "VIOLIN";
                soundObj = instrumentPrefabs[3];
                break;
            case "/bell":
                data = sinData;
                data.instrumentName = "/bell";
                text.text = "BELLS";
                soundObj = instrumentPrefabs[0];
                break;
            default:
                break;
        }
        name = data.instrumentName;
        values = data.values;
    }

    public void acceptCreation()
    {
        Sound sound = soundObj.GetComponent<Sound>();
        sound.items = items;
        sound.name = name;
        sound.timeControl = timeControl;
        sound.isPreset = false;

        OSCHandler.Instance.SendSoundMessageToClient("SuperCollider", name, -1, items[i].y);

        soundObjManager.addSoundObject(name, soundObj);
    }

    public void undo()
    {
        if (items.Count != 0)
        {
            // DESTROY LAST SOUND ITEM
            // destroy the stop and start of the
            // last item added
            items.RemoveAt(items.Count - 1);
            items.RemoveAt(items.Count - 1);

            // DESTROY LAST GAMEOBJECTS SET
            // remove all the instances of the last item added
            if (itemsGO.Count != 0)
            {
                foreach (GameObject go in itemsGOSets[itemsGOSets.Count - 1])
                    Destroy(go);
                itemsGOSets.RemoveAt(itemsGOSets.Count - 1);
            }

            audioUI.PlayOneShot(undoClip, 0.8f);
        }
    }

    public void cleanAll()
    {
        if (items.Count != 0)
        {
            pianoPlaying = false;

            OSCHandler.Instance.SendMessageToClient("SuperCollider", name, -1, -1, -1);
            items.Clear();
            foreach (GameObject go in itemsGO)
                Destroy(go);
            itemsGO.Clear();

            if (pianoActive)
                audioUI.PlayOneShot(cleanClip, 0.7f);
        }
    }

    public void deactivatePiano()
    {
        pianoActive = false;
        cleanAll();
    }

    void playItems()
    {
        if (i < items.Count && (timeControl.time <= items[i].x + 0.01f && timeControl.time >= items[i].x - 0.01f))
        {
            OSCHandler.Instance.SendSoundMessageToClient("SuperCollider", name, items[i].z, items[i].y);
            i++;
        }
    }

    void createItem()
    {
        if (instantiateItem && (timeControl.time % 0.05f) < 0.05f)
        {
            Vector3 itemPos = timeBar.transform.position;
            itemPos.y = itemY * 20 + 36;
            GameObject go = Instantiate(pianoItem, itemPos, Quaternion.identity, transform.parent);
            itemsGO.Add(go);
            itemsGOSets[itemsGOSets.Count - 1].Add(go);
        }
    }

    void updateTime()
    {
        if (timeControl.time <= 0.01f)
            i = 0;

        timeBar.transform.localPosition = new Vector3((timeControl.time - 1) * 75, 0f, 0f);
    }

    void handleInput()
    {
        float DPADposX = Input.GetAxis("DPADHorizontal");
        float DPADposY = Input.GetAxis("DPADVertical");

        instantiateItem = true;

        item.x = timeControl.time;

        if (DPADposX > 0) // RIGHT
        {
            if (!pianoPlaying)
            {
                pianoPlaying = true;
                itemY = 3;
                item.y = values[0];
                item.z = 1;
                items.Add(item);
                itemsGOSets.Add(new List<GameObject>());
            }
        }
        else if (DPADposX < 0) // LEFT
        {
            if (!pianoPlaying)
            {
                pianoPlaying = true;
                itemY = 1;
                item.y = values[1];
                item.z = 1;
                items.Add(item);
                itemsGOSets.Add(new List<GameObject>());
            }
        }
        else if (DPADposY > 0) // UP
        {
            if (!pianoPlaying)
            {
                pianoPlaying = true;
                itemY = 0;
                item.y = values[2];
                item.z = 1;
                items.Add(item);
                itemsGOSets.Add(new List<GameObject>());
            }
        }
        else if (DPADposY < 0) // DOWN
        {
            if (!pianoPlaying)
            {
                pianoPlaying = true;
                itemY = 2;
                item.y = values[3];
                item.z = 1;
                items.Add(item);
                itemsGOSets.Add(new List<GameObject>());
            }
        }
        else if (Input.GetButton("Button Y")) // Y
        {
            if (!pianoPlaying)
            {
                pianoPlaying = true;
                itemY = 4;
                item.y = values[4];
                item.z = 1;
                items.Add(item);
                itemsGOSets.Add(new List<GameObject>());
            }
        }
        else if (Input.GetButton("Button X")) // X
        {
            if (!pianoPlaying)
            {
                pianoPlaying = true;
                itemY = 5;
                item.y = values[5];
                item.z = 1;
                items.Add(item);
                itemsGOSets.Add(new List<GameObject>());
            }
        }
        else if (Input.GetButton("Button A")) // A
        {
            if (!pianoPlaying)
            {
                pianoPlaying = true;
                itemY = 6;
                item.y = values[6];
                item.z = 1;
                items.Add(item);
                itemsGOSets.Add(new List<GameObject>());
            }
        }
        else if (Input.GetButton("Button B")) // B
        {
            if (!pianoPlaying)
            {
                pianoPlaying = true;
                itemY = 7;
                item.y = values[7];
                item.z = 1;
                items.Add(item);
                itemsGOSets.Add(new List<GameObject>());
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
