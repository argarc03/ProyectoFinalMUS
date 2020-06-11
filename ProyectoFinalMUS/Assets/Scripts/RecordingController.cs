using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class RecordingController : MonoBehaviour
{
    public bool pianoActive = false;
    public bool pianoPlaying = false;
    public string name = ""; //usar esto cuando se añadan varios instrumentos!!!!
    public int currentObjectIndex;

    public GameObject timeBar;

    public GameObject pianoItem;
    bool instantiateItem = false;
    int itemY = 0;
    public float verticalSpacing = 1.0f;
    public float verticalOffset = 200.0f;

    List<GameObject> itemsGO = new List<GameObject>();
    Color currentColor = Color.grey;

    public List<Vector3> items = new List<Vector3>(); // time pos, freq, start/stop
    Vector3 item;
    public int i = 0;

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

    bool inputAfterEnd = false;
    public Stack<int> additionOrder = new Stack<int>();
    int oldI;
    bool soundPlaying;
    float offset = 0.02f;

    static int SortByTimePos(Vector3 p1, Vector3 p2)
    {
        return p1.x.CompareTo(p2.x);
    }

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

    public void init(string insName, List<GameObject> instrumentPrefabs, int index)
    {
        currentObjectIndex = index;
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
        if (!pianoPlaying)
        {
            Sound sound = soundObj.GetComponent<Sound>();
            sound.items = items;
            sound.name = name;
            sound.objectIndex = currentObjectIndex;
            sound.timeControl = timeControl;
            sound.isPreset = false;

            if (i - 1 >= 0 && items[i - 1].z == 1)
                OSCHandler.Instance.SendSoundMessageToClient("SuperCollider", name, -1, items[i - 1].y, currentObjectIndex);

            soundObjManager.addSoundObject(name, soundObj);
        }
    }

    public void undo()
    {
        if (!pianoPlaying && items.Count != 0)
        {
            // DESTROY LAST SOUND ITEM
            // destroy the stop and start of the
            // last item added

            // en la nota
            if (i - 1 == additionOrder.Peek() - 1)
            {
                OSCHandler.Instance.SendSoundMessageToClient("SuperCollider", name, -1, items[i - 1].y, currentObjectIndex);

                soundPlaying = false;

                i--;
                if (i < 0) i = 0;
            }
            // detras de la nota
            else if (i > additionOrder.Peek())
            {
                i -= 2;
                if (i < 0) i = 0;
            }


            items.RemoveAt(additionOrder.Pop());
            items.RemoveAt(additionOrder.Pop());

            // DESTROY LAST GAMEOBJECTS SET
            // remove all the instances of the last item added
            if (itemsGO.Count != 0)
            {
                foreach (GameObject go in itemsGOSets[itemsGOSets.Count - 1])
                    Destroy(go);
                itemsGOSets.RemoveAt(itemsGOSets.Count - 1);
            }

            

            audioUI.PlayOneShot(undoClip, 0.8f);

            items.Sort(SortByTimePos);
        }
    }

    public void cleanAll()
    {
        if (!pianoPlaying && items.Count != 0)
        {
            pianoPlaying = false;
            soundPlaying = false;

            OSCHandler.Instance.SendSoundMessageToClient("SuperCollider", name, -1, -1, currentObjectIndex);
            items.Clear();
            foreach (GameObject go in itemsGO)
                Destroy(go);
            itemsGO.Clear();

            i = 0;

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
        if (i < items.Count && (timeControl.time >= items[i].x - offset))
        {
            if (items[i].z == 1)
                soundPlaying = true;
            else if (items[i].z == -1 && ((i + 1 < items.Count && (timeControl.time < items[i+1].x - offset) || i + 1 >= items.Count)))
                soundPlaying = false;
        }

        if (i < items.Count && (timeControl.time <= items[i].x + 0.01f && timeControl.time >= items[i].x - 0.01f))
        {
            OSCHandler.Instance.SendSoundMessageToClient("SuperCollider", name, items[i].z, items[i].y, currentObjectIndex);
            i++;
        }
    }

    void createItem()
    {
        if (pianoPlaying && instantiateItem && (timeControl.time % 0.05f) < 0.05f)
        {
            Vector3 itemPos = timeBar.transform.position;
            itemPos.y = (itemY * verticalSpacing) + verticalOffset;
            GameObject go = Instantiate(pianoItem, itemPos, Quaternion.identity, transform.parent);
            go.GetComponent<Image>().color = currentColor;
            itemsGO.Add(go);
            itemsGOSets[itemsGOSets.Count - 1].Add(go);

            //sort
            items.Sort(SortByTimePos);
        }
    }

    void updateTime()
    {
        if (timeControl.time <= 0.01f)
            i = 0;

        //print(soundPlaying);

        timeBar.transform.localPosition = new Vector3((timeControl.time - 1) * 75, 0f, 0f);
    }

    void addStartItem(int y, int value, Color color)
    {
        if (items.Count == 0 || i == 0 || (items.Count != 0 && i - 1 >= 0 && i - 1 < items.Count && items[i - 1].z == -1))
        {
            if (!pianoPlaying && !soundPlaying)
            {
                pianoPlaying = true;
                itemY = y;
                item.y = values[value];
                item.z = 1;
                items.Add(item);
                additionOrder.Push(i);
                itemsGOSets.Add(new List<GameObject>());
                currentColor = color;
                oldI = i;
            }
        }
    }

    void handleInput()
    {
        float DPADposX = Input.GetAxis("DPADHorizontal");
        float DPADposY = Input.GetAxis("DPADVertical");

        instantiateItem = true;
        item.x = timeControl.time;

        // starts pressing a button
        if (inputAfterEnd)
        {
            if (!Input.anyKey)
            {
                inputAfterEnd = false;
                //print("inputafterend = false");
            }
            return;
        }

        // reach end or start of a note
        if (pianoPlaying && ((items[items.Count - 1].z == 1 && item.x >= timeControl.maxTime - 0.02f) ||
            (oldI + 1 < items.Count && timeControl.time >= items[oldI + 1].x - offset)))
        {
            pianoPlaying = false;
            inputAfterEnd = true;
            item.z = -1;
            items.Add(item);
            additionOrder.Push(i);

            items.Sort(SortByTimePos);

            return;
        }

        if (DPADposX > 0) // RIGHT
            addStartItem(3, 0, Color.red);
        else if (DPADposX < 0) // LEFT
            addStartItem(1, 1, Color.blue);
        else if (DPADposY > 0) // UP
            addStartItem(0, 2, Color.yellow);
        else if (DPADposY < 0) // DOWN
            addStartItem(2, 3, Color.green);
        else if (Input.GetButton("Button Y")) // Y
            addStartItem(4, 4, Color.yellow);
        else if (Input.GetButton("Button X")) // X
            addStartItem(5, 5, Color.blue);
        else if (Input.GetButton("Button A")) // A
            addStartItem(6, 6, Color.green);
        else if (Input.GetButton("Button B")) // B
            addStartItem(7, 7, Color.red);
        else
        {
            if (pianoPlaying)
            {
                pianoPlaying = false;

                item.z = -1;
                items.Add(item);
                additionOrder.Push(i);
                items.Sort(SortByTimePos);
            }

            instantiateItem = false;
        }
    }
}
