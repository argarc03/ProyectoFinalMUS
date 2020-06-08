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
    public GameObject samplersSelector;

    // Presets menu & buttons
    public GameObject panelPresets;
    public List<GameObject> buttonsPresets = new List<GameObject>();
    public GameObject presetsSelector;

    float angleSector;

    // Button selection
    GameObject selectedSample;
    GameObject selectedPreset;

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

    // Piano

    public GameObject pianoPanel;
    public GameObject drumsPanel;

    public AudioClip openWheelClip;
    public AudioClip changeInstrumentClip;
    public AudioClip openRecorderClip;
    public AudioClip changeSoundClip;
    AudioSource audioUI;

    void Start()
    {
        audioUI = GetComponent<AudioSource>();
        EventSystem.current.SetSelectedGameObject(null, null);
        panelSamplers.SetActive(false);
        samplersSelector.SetActive(false);
        presetsSelector.SetActive(false);
        panelPresets.SetActive(false);
        objectCursor.SetActive(false);

        // Calculate radialUI angle
        angleSector = 90 / buttonsSampler.Count;
    }

    void Update()
    {
        // UPDATE RECORDER PANEL
        if (pianoPanel.activeSelf && Input.GetAxis("LeftBumper") != 0)
            closePiano();

        if (pianoPanel.activeSelf && Input.GetButtonDown("Options"))
            pianoPanel.GetComponent<RecordingController>().cleanAll();

        if (pianoPanel.activeSelf && Input.GetButtonDown("Share"))
            pianoPanel.GetComponent<RecordingController>().undo();

        if (pianoPanel.activeSelf && Input.GetAxis("RightBumper") != 0)
        {
            pianoPanel.GetComponent<RecordingController>().acceptCreation();
            closePiano();
        }

        if (!objectNavigationMode && (Input.GetAxis("LeftHorizontal") != 0 || Input.GetAxis("LeftVertical") != 0) && soundObjManager.getNumOfSounds() > 0)
            setCursorNavigationMode(true);
        else if (objectNavigationMode && (Input.GetButtonDown("Button B") || soundObjManager.getNumOfSounds() == 0))
            setCursorNavigationMode(false);

        if (objectNavigationMode)
            updateCursorNavigationMode();

        // UPDATE SAMPLERS PANEL-------------
        // Show/hide the samplers panel
        if (!panelSamplers.activeSelf && Input.GetAxis("RightTrigger") != 0)
        {
            if (!audioUI.isPlaying)
                audioUI.PlayOneShot(openWheelClip);
        }

        if (!pianoPanel.activeSelf && Input.GetAxis("RightTrigger") != 0)
        {
            panelSamplers.SetActive(true);
            if (objectNavigationMode)
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
        if (panelSamplers.activeSelf)
        {
            float posX = Input.GetAxis("RightHorizontal");
            float posY = Input.GetAxis("RightVertical");
            float angle = Mathf.Atan2(posX, posY) * Mathf.Rad2Deg;

            if (angle == 0 || -angle > buttonsSampler.Count * angleSector)
            {
                EventSystem.current.SetSelectedGameObject(null, null);
                selectedSample = null;
                samplersSelector.SetActive(false);
            }
            else
            {
                int j = buttonsSampler.Count;
                while (j >= 0 && -angle <= angleSector * j)
                    j--;

                if (j >= 0 && j < buttonsSampler.Count)
                {
                    EventSystem.current.SetSelectedGameObject(buttonsSampler[j], null);
                    selectedSample = buttonsSampler[j];
                    samplersSelector.SetActive(true);
                    Quaternion newRot = Quaternion.Euler(0, 0, angleSector * j - 45f + 9 + 5);

                    if (samplersSelector.transform.rotation != newRot)
                        audioUI.PlayOneShot(changeInstrumentClip, 0.5f);

                    samplersSelector.transform.rotation = newRot;
                }
            }
        }

        // UPDATE PRESETS PANEL-------------
        // Show/hide the samplers panel
        if (!panelPresets.activeSelf && Input.GetAxis("LeftTrigger") != 0)
        {
            if (!audioUI.isPlaying)
                audioUI.PlayOneShot(openWheelClip);
        }

        if (!pianoPanel.activeSelf && Input.GetAxis("LeftTrigger") != 0)
        {
            panelPresets.SetActive(true);
            if (objectNavigationMode)
                setCursorNavigationMode(false);
        }
        else
        {
            // Call the onClick event if the selected gameObject is not null
            if (selectedPreset != null && panelPresets.activeSelf)
            {
                EventSystem.current.SetSelectedGameObject(selectedPreset, null);
                EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
            }

            panelPresets.SetActive(false);
        }

        // Select button with left joystick
        if (panelPresets.activeSelf)
        {
            float posX = Input.GetAxis("LeftHorizontal");
            float posY = Input.GetAxis("LeftVertical");
            float angle = Mathf.Atan2(posX, posY) * Mathf.Rad2Deg;

            if (angle == 0 || angle > buttonsPresets.Count * angleSector)
            {
                EventSystem.current.SetSelectedGameObject(null, null);
                selectedPreset = null;
                presetsSelector.SetActive(false);
            }
            else
            {
                int j = buttonsPresets.Count;
                while (j >= 0 && angle <= angleSector * j)
                    j--;

                if (j >= 0 && j < buttonsPresets.Count)
                {
                    EventSystem.current.SetSelectedGameObject(buttonsPresets[j], null);
                    selectedPreset = buttonsPresets[j];
                    presetsSelector.SetActive(true);
                    Quaternion newRot = Quaternion.Euler(0, 0, -angleSector * j - 45f - 9 - 5 - 1 + 15);

                    if (presetsSelector.transform.rotation != newRot)
                        audioUI.PlayOneShot(changeInstrumentClip, 0.5f);

                    presetsSelector.transform.rotation = newRot;
                }
            }
        }

        //selector.transform.rotation = Quaternion.Euler(0, 0, -45f + 9);

        /*if (angle < 0)
        {
            EventSystem.current.SetSelectedGameObject(buttonsSampler[0], null);
            selectedSample = buttonsSampler[0];
            selector.SetActive(true);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(buttonsSampler[1], null);
            selectedSample = buttonsSampler[1];
        }*/

        // UPDATE FX PANEL-----------
        // Show/hide the samplers panel
        /*if (Input.GetAxis("LeftTrigger") != 0)
            panelPresets.SetActive(true);
        else
        {
            // Call the onClick event if the selected gameObject is not null
            if (selectedPreset != null && panelPresets.activeSelf)
            {
                EventSystem.current.SetSelectedGameObject(selectedPreset, null);
                EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
            }


            panelPresets.SetActive(false);
        }
        /*
        if (selectedPreset != null)
        {
            print(selectedPreset.name);
        }
        else
        {
            print("NUULL");
        }
        
        // Select button with left joystick
        float FXposX = Input.GetAxis("LeftHorizontal");
        float FXposY = Input.GetAxis("LeftVertical");
        float FXangle = Mathf.Atan2(FXposX, FXposY) * Mathf.Rad2Deg;
        //print(angle);

        if (FXangle == 0)
        { selectedPreset = null; EventSystem.current.SetSelectedGameObject(null, null); }
        else if (FXangle < 0)
        { selectedPreset = buttonsPresets[0]; EventSystem.current.SetSelectedGameObject(buttonsPresets[0], null); }
        else
        { selectedPreset = buttonsPresets[1]; EventSystem.current.SetSelectedGameObject(buttonsPresets[1], null); }*/
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
        else if (selectedSoundObject == null && Input.GetButtonDown("Button X"))
        {
            if (soundObjects[cursorY, cursorX] != null)
            {
                if (soundObjects[cursorY, cursorX].GetComponent<Sound>().isMuted())
                    soundObjManager.desmuteSoundObject(cursorX, cursorY);
                else
                    soundObjManager.muteSoundObject(cursorX, cursorY);
            }
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
        else prevDirY = dirY;

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
                if (selectedGO == null) newCursorX += dirX;
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

            audioUI.PlayOneShot(changeSoundClip);
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

    void closePiano()
    {
        pianoPanel.SetActive(false);
        pianoPanel.GetComponent<RecordingController>().deactivatePiano();

        //audioUI.PlayOneShot();
    }

    public void openPiano(string name)
    {
        pianoPanel.SetActive(true);
        pianoPanel.GetComponent<RecordingController>().init(name);

        pianoPanel.GetComponent<RecordingController>().icon.sprite = selectedSample.GetComponent<Button>().image.sprite;

        audioUI.PlayOneShot(openRecorderClip, 0.6f);
    }

    // Add a synth to the scene
    public void AddSynth(string name)
    {
        print("New synth added");

        // Instantiate instrument in the scenario and add it to SuperCollider
        GameObject soundObj = sinPrefab;
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
