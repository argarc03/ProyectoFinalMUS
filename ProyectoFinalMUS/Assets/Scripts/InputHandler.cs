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
    public GameObject rtIcon;

    // Presets menu & buttons
    public GameObject panelPresets;
    public List<GameObject> buttonsPresets = new List<GameObject>();
    public GameObject presetsSelector;
    public GameObject ltIcon;

    float angleSector;

    // Button selection
    GameObject selectedSample;
    GameObject selectedPreset;

    // Instruments prefabs
    public List<GameObject> instrumentPrefabs = new List<GameObject>();

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
    public GameObject instrumentControls, drumsControls;

    public AudioClip openWheelClip;
    public AudioClip changeInstrumentClip;
    public AudioClip openRecorderClip;
    public AudioClip changeSoundClip;
    AudioSource audioUI;

    public GameObject exitMenu;

    // Presets
    bool[] usedPresets;
    public List<GameObject> disabledPresetObjs;

    void Start()
    {
        usedPresets = new bool[6];
        for (int i = 0; i < 6; i++) usedPresets[i] = false;
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

    // Update the UI of presets and record wheels,
    // the recorder, and the navigation input
    void Update()
    {
        // UPDATE EXIT MENU
        if (Input.GetButtonDown("Start"))
            exitMenu.SetActive(true);

        if (exitMenu.activeSelf && Input.GetButtonDown("Button A"))
            Application.Quit();
        if (exitMenu.activeSelf && Input.GetButtonDown("Button B"))
            exitMenu.SetActive(false);

        if (exitMenu.activeSelf)
            return;

        // UPDATE RECORDER PANEL
        if (pianoPanel.activeSelf && Input.GetAxis("LeftBumper") != 0)
            closePiano();

        if (pianoPanel.activeSelf && Input.GetButtonDown("Options"))
            pianoPanel.GetComponent<RecordingController>().cleanAll();

        if (pianoPanel.activeSelf && Input.GetButtonDown("Share"))
            pianoPanel.GetComponent<RecordingController>().undo();

        if (pianoPanel.activeSelf && !pianoPanel.GetComponent<RecordingController>().pianoPlaying
            && pianoPanel.GetComponent<RecordingController>().items.Count != 0 && Input.GetAxis("RightBumper") != 0)
        {
            pianoPanel.GetComponent<RecordingController>().acceptCreation();
            closePiano();
        }

        // Trigger icons
        if (!pianoPanel.activeSelf && Input.GetAxis("LeftTrigger") == 0 && Input.GetAxis("RightTrigger") == 0)
        {
            ltIcon.SetActive(true);
            rtIcon.SetActive(true);
        }
        else
        {
            ltIcon.SetActive(false);
            rtIcon.SetActive(false);
        }

        if (!objectNavigationMode && (Input.GetAxis("LeftHorizontal") != 0 || Input.GetAxis("LeftVertical") != 0) && soundObjManager.getNumOfSounds() > 0)
            setCursorNavigationMode(true);
        else if (objectNavigationMode && (Input.GetButtonDown("Button B") || soundObjManager.getNumOfSounds() == 0))
            setCursorNavigationMode(false);

        if (objectNavigationMode)
            updateCursorNavigationMode();

        if (pianoPanel.activeSelf)
        {
            if (objectNavigationMode)
                setCursorNavigationMode(false);
        }

        // UPDATE SAMPLERS PANEL-------------
        // Show/hide the samplers panel
        if (!pianoPanel.activeSelf && !panelSamplers.activeSelf && Input.GetAxis("RightTrigger") != 0)
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
                    Quaternion newRot = Quaternion.Euler(0, 0, angleSector * j - 31);

                    if (samplersSelector.transform.rotation != newRot)
                        audioUI.PlayOneShot(changeInstrumentClip, 0.5f);

                    samplersSelector.transform.rotation = newRot;
                }
            }
        }

        // UPDATE PRESETS PANEL-------------
        // Show/hide the samplers panel
        if (!pianoPanel.activeSelf && !panelPresets.activeSelf && Input.GetAxis("LeftTrigger") != 0)
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
                    Quaternion newRot = Quaternion.Euler(0, 0, -angleSector * j - 45);

                    if (presetsSelector.transform.rotation != newRot)
                        audioUI.PlayOneShot(changeInstrumentClip, 0.5f);

                    presetsSelector.transform.rotation = newRot;
                }
            }
        }
    }

    // Update the cursor position and the musician selected
    void updateCursorNavigationMode()
    {
        GameObject[,] soundObjects = soundObjManager.getSoundObjects();

        // Select an object
        if (selectedSoundObject == null && Input.GetButtonDown("Button A"))
        {
            string n = soundObjManager.getObjectType(cursorX, cursorY);
            switch (n)
            {
                case "/bellPreset":
                    usedPresets[0] = false;
                    disabledPresetObjs[0].SetActive(false);
                    break;
                case "/pianoPreset":
                    usedPresets[1] = false;
                    disabledPresetObjs[1].SetActive(false);
                    break;
                case "/drumPreset":
                    usedPresets[2] = false;
                    disabledPresetObjs[2].SetActive(false);
                    break;
                case "/violinPreset":
                    usedPresets[3] = false;
                    disabledPresetObjs[3].SetActive(false);
                    break;
                case "/guitarPreset":
                    usedPresets[4] = false;
                    disabledPresetObjs[4].SetActive(false);
                    break;
                case "/flutePreset":
                    usedPresets[5] = false;
                    disabledPresetObjs[5].SetActive(false);
                    break;
                default:
                    break;
            }
            if (soundObjects[cursorY, cursorX] != null)
            {
                soundObjManager.removeSoundObject(cursorX, cursorY);

                if (cursorX + cursorY * soundObjManager.width == soundObjManager.getNumOfSounds())
                {
                    int soundCounter = soundObjManager.getNumOfSounds() - 1;

                    cursorX = (soundCounter) % soundObjManager.width;
                    cursorY = (soundCounter) / soundObjManager.width;

                    objectCursor.transform.position = new Vector3(cursorX + 1, cursorHeight, -cursorY);
                }
            }
        }
        else if (selectedSoundObject == null && Input.GetButtonDown("Button X"))
        {
            if (soundObjects[cursorY, cursorX] != null)
            {
                if (soundObjManager.isMuted(cursorX, cursorY))
                    soundObjManager.desmuteSoundObject(cursorX, cursorY);
                else
                    soundObjManager.muteSoundObject(cursorX, cursorY);
            }
        }
        else if (selectedSoundObject == null && Input.GetButtonDown("Button Y"))
        {
            if (soundObjects[cursorY, cursorX] != null)
            {
                soundObjManager.soloSoundObject(cursorX, cursorY);
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

            if (!panelPresets.activeSelf && !panelSamplers.activeSelf && !pianoPanel.activeSelf)
                audioUI.PlayOneShot(changeSoundClip);
        }
    }

    // Start musicians navigation 
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

    // Close the recorder
    void closePiano()
    {
        pianoPanel.SetActive(false);
        drumsControls.SetActive(false);
        instrumentControls.SetActive(false);
        pianoPanel.GetComponent<RecordingController>().deactivatePiano();
    }

    // Open the recorder
    public void openPiano(string name)
    {
        pianoPanel.SetActive(true);
        if (name != "/drums")
            instrumentControls.SetActive(true);
        else
            drumsControls.SetActive(true);
        pianoPanel.GetComponent<RecordingController>().init(name, instrumentPrefabs, soundObjManager.getNumOfSounds());

        pianoPanel.GetComponent<RecordingController>().icon.sprite = selectedSample.GetComponent<Button>().image.sprite;

        audioUI.PlayOneShot(openRecorderClip, 0.6f);
    }

    // Add a synth to the scene
    public void AddSynth(string name)
    {
        // Instantiate instrument in the scenario and add it to SuperCollider
        GameObject soundObj = null;
        switch (name)
        {
            case "/bellPreset":
                if (!usedPresets[0])
                {
                    soundObj = instrumentPrefabs[0];
                    usedPresets[0] = true;
                    disabledPresetObjs[0].SetActive(true);
                }
                break;
            case "/pianoPreset":
                if (!usedPresets[1])
                {
                    soundObj = instrumentPrefabs[1];
                    usedPresets[1] = true;
                    disabledPresetObjs[1].SetActive(true);
                }
                break;
            case "/drumPreset":
                if (!usedPresets[2])
                {
                    soundObj = instrumentPrefabs[2];
                    usedPresets[2] = true;
                    disabledPresetObjs[2].SetActive(true);
                }
                break;
            case "/violinPreset":
                if (!usedPresets[3])
                {
                    soundObj = instrumentPrefabs[3];
                    usedPresets[3] = true;
                    disabledPresetObjs[3].SetActive(true);
                }
                break;
            case "/guitarPreset":
                if (!usedPresets[4])
                {
                    soundObj = instrumentPrefabs[4];
                    usedPresets[4] = true;
                    disabledPresetObjs[4].SetActive(true);
                }
                break;
            case "/flutePreset":
                if (!usedPresets[5])
                {
                    soundObj = instrumentPrefabs[5];
                    usedPresets[5] = true;
                    disabledPresetObjs[5].SetActive(true);
                }
                break;
            default:
                break;
        }

        if (soundObj != null)
        {
            soundObj.GetComponent<Sound>().isPreset = true;
            soundObjManager.addSoundObject(name, soundObj);
        }
    }
}
