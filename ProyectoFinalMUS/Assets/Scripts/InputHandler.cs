﻿using System.Collections;
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

    // Sound Object Navigation
    public GameObject objectCursor;
    public GameObject navigationInstructions;

    int cursorX = 0;
    int cursorY = 0;
    int prevDirX = 0;
    int prevDirY = 0;
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
        if (!objectNavigationMode && (Input.GetAxis("LeftHorizontal") != 0 || Input.GetAxis("LeftVertical") != 0) && soundObjManager.getNumOfSounds() > 0)
            setCursorNavigationMode(true);
        else if (objectNavigationMode && (Input.GetButtonDown("Button B") || soundObjManager.getNumOfSounds() == 0))
            setCursorNavigationMode(false);

        if(objectNavigationMode)
        {
            updateCursorNavigationMode();
        }

        // UPDATE SAMPLERS PANEL-------------
        // Show/hide the samplers panel
        if (Input.GetAxis("RightTrigger") != 0)
        {
            panelSamplers.SetActive(true);
            if(objectNavigationMode)
                setCursorNavigationMode(false);
        }
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
        else if (Input.GetAxis("LeftVertical") != 0)
            dirY = Input.GetAxis("LeftVertical") < 0 ? 1 : -1;

        // Limit movement of the cursor
        if (dirX == prevDirX)
            dirX = 0;
        else prevDirX = dirX;

        if (dirY == prevDirY)
            dirY = 0;
        else prevDirX = dirY;

        int newCursorX = cursorX, newCursorY = cursorY;
        GameObject selectedGO = null;
        bool cursorUpdated = false;
        // Horizontal movement of the cursor
        if (dirX != 0)
        {
            newCursorX += dirX;

            while (newCursorX >= 0 && newCursorX < soundObjManager.width && selectedGO == null)
            {
                selectedGO = soundObjects[newCursorY, newCursorX];
                if(selectedGO == null) newCursorX += dirX;
            }

            cursorUpdated = true;
        }

        // Vertical movement of the cursor
        if (dirY != 0)
        {
            newCursorY += dirY;

            while (newCursorY >= 0 && newCursorY < soundObjManager.height && selectedGO == null)
            {
                selectedGO = soundObjects[newCursorY, newCursorX];
                if (selectedGO == null) newCursorY += dirY;
            }

            cursorUpdated = true;
        }

        if (cursorUpdated && selectedGO != null)
        {
            cursorX = newCursorX; cursorY = newCursorY;
            objectCursor.transform.position = new Vector3(cursorX + 1, cursorHeight, -cursorY);
        }
    }

    void setCursorNavigationMode(bool active)
    {
        navigationInstructions.SetActive(active);
        if (!active)
        {
            objectCursor.SetActive(false);
            objectNavigationMode = false;
        }
        else
        {
            objectCursor.transform.position = new Vector3(1, cursorHeight, 0);
            cursorX = 0;
            cursorY = 0;
            objectCursor.SetActive(true);
            objectNavigationMode = true;
        }
    }


    // Add a synth to the scene
    public void AddSynth(string name)
    {
        print("New synth added");

        // Instantiate instrument in the scenario and add it to SuperCollider
        GameObject soundObj = null;
        switch (name)
        {
            case "/sin":
                soundObj = sinPrefab;
                break;
            case "/square":
                soundObj = squarePrefab;
                break;
            default:
                break;
        }
        soundObjManager.addSoundObject(name, soundObj);
    }
}
