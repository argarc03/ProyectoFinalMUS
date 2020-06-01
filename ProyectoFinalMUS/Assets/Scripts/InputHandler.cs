using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    // Samplers menu & buttons
    public GameObject panelSamplers;
    public List<GameObject> buttonsSampler = new List<GameObject>();
    //int currentSelected = 0;

    // FX menu & buttons
    public GameObject panelFX;
    public List<GameObject> buttonsFX = new List<GameObject>();

    // Button selection
    GameObject selectedSample;

    // Intruments prefabs
    public GameObject sinPrefab;
    public GameObject squarePrefab;

    // Sound Objects Manager
    public SoundObjectManager soundObjManager;

    int x = 0;
    int y = 0;

    // Sound Object Navigation
    public GameObject objectCursor;

    int cursorX = 0;
    int cursorY = 0;
    public float cursorHeight = 1;

    bool objectNavigationMode = false;

    GameObject selectedSoundObject = null;

    // Use this for initialization
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(null, null);
        panelSamplers.SetActive(false);
        panelFX.SetActive(false);
        objectCursor.SetActive(false);
        // Calculate 
    }

    void Update()
    {
        if (!objectNavigationMode && Input.GetButtonDown("Button Y"))
            setCursorNavigationMode(true);
        else if (objectNavigationMode && Input.GetButtonDown("Button B"))
            setCursorNavigationMode(false);

        if(objectNavigationMode)
        {
            updateCursorNavigationMode();
            return;
        }

        // UPDATE SAMPLERS PANEL-------------
        // Show/hide the samplers panel
        if (Input.GetAxis("RightTrigger") != 0)
            panelSamplers.SetActive(true);
        else
        {

            // Call the onClick event if the selected gameObject is not null
            if (selectedSample != null && panelSamplers.activeSelf)
            {
                EventSystem.current.SetSelectedGameObject(selectedSample, null);
                EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
            }

            panelSamplers.SetActive(false);
        }

        // Select button with right joystick
        float posX = Input.GetAxis("RightHorizontal");
        float posY = Input.GetAxis("RightVertical");
        float angle = Mathf.Atan2(posX, posY) * Mathf.Rad2Deg;
        //print(angle);

        if (angle == 0)
        {
            EventSystem.current.SetSelectedGameObject(null, null);
            selectedSample = null;
        }
        else if (angle < 0)
        {
            EventSystem.current.SetSelectedGameObject(buttonsSampler[0], null);
            selectedSample = buttonsSampler[0];
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(buttonsSampler[1], null);
            selectedSample = buttonsSampler[1];
        }

        // UPDATE FX PANEL-----------
        // Show/hide the samplers panel
        if (Input.GetAxis("LeftTrigger") != 0)
            panelFX.SetActive(true);
        else
        {
            // Call the onClick event if the selected gameObject is not null
            if (EventSystem.current.currentSelectedGameObject != null && panelFX.activeSelf)
                EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();

            panelFX.SetActive(false);
        }

        // Select button with left joystick
        float FXposX = Input.GetAxis("LeftHorizontal");
        float FXposY = Input.GetAxis("LeftVertical");
        float FXangle = Mathf.Atan2(FXposX, FXposY) * Mathf.Rad2Deg;
        //print(angle);

        if (FXangle == 0)
            EventSystem.current.SetSelectedGameObject(null, null);
        else if (FXangle < 0)
            EventSystem.current.SetSelectedGameObject(buttonsFX[0], null);
        else
            EventSystem.current.SetSelectedGameObject(buttonsFX[1], null);
    }

    void updateCursorNavigationMode()
    {
        GameObject[,] soundObjects = soundObjManager.getSoundObjects();
        // Select an object
        if (selectedSoundObject == null && Input.GetButtonDown("Button A"))
        {
            // TODO: Abrir un menú con las opciones (Silenciar, Mover, Eliminar...)
            // de momento: borrar el objeto
            if (soundObjects[cursorY, cursorX] != null) soundObjManager.removeSoundObject(cursorX, cursorY);
        }

        if (selectedSoundObject != null) return;
        // Navigate between objects
        int dirX = 0;
        int dirY = 0;

        if (Input.GetAxis("LeftHorizontal") != 0)
            dirX = Input.GetAxis("LeftHorizontal") < 0 ? -1 : 1;
        /*else */if (Input.GetAxis("LeftVertical") != 0)
            dirY = Input.GetAxis("LeftVertical") < 0 ? -1 : 1;

        if (dirX != 0 || dirY != 0)
        {
            int newCursorX = cursorX + dirX, newCursorY = cursorY + dirY;
            // TODO: Buscar el primer objeto no null en la dirección [newCursorX, newCursorY]
            if (newCursorX > 0 && newCursorX < soundObjManager.width && newCursorY >= 0 && newCursorY < soundObjManager.height 
                && soundObjects[newCursorY, newCursorX] != null)
            {
                cursorX = newCursorX; cursorY = newCursorY;
                objectCursor.transform.position = new Vector3(cursorX, cursorHeight, -cursorY);
            }
        }
    }

    void setCursorNavigationMode(bool active)
    {
        if (!active)
        {
            objectCursor.SetActive(false);
            objectNavigationMode = false;
        }
        else
        {
            objectCursor.transform.position = new Vector3(1, cursorHeight, 0);
            cursorX = 1;
            cursorY = 0;
            objectCursor.SetActive(true);
            objectNavigationMode = true;
        }
    }


    // Add a synth to the scene
    public void AddSynth(string name)
    {
        OSCHandler.Instance.SendMessageToClient("SuperCollider", name, 1.0);
        print("New synth added");

        x++;
        if (x > soundObjManager.width)
        {
            x = 1;
            y++;

            if (y > soundObjManager.height) return; // Cannot add more instruments, scenario full
        }

        // Instantiate instrument in the scenario
        GameObject soundObj = null;
        switch (name)
        {
            case "/sin":
                soundObj = Instantiate(sinPrefab, new Vector3(x, 0, -y), Quaternion.identity);
                break;
            case "/square":
                soundObj = Instantiate(squarePrefab, new Vector3(x, 0, -y), Quaternion.identity);
                break;
            default:
                break;
        }
        soundObjManager.addSoundObject(x, y, soundObj);

    }
}
