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

    // Intruments prefabs
    public GameObject sinPrefab;
    public GameObject squarePrefab;

    // Scenario width and height
    float width = 8;
    float height = 4;

    float x = 0;
    float y = 0;

    // Use this for initialization
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(null, null);
        panelSamplers.SetActive(false);
        panelFX.SetActive(false);
        // Calculate 
    }

    void Update()
    {
        // UPDATE SAMPLERS PANEL-------------
        // Show/hide the samplers panel
        if (Input.GetAxis("RightTrigger") != 0)
            panelSamplers.SetActive(true);
        else
        {
            // Call the onClick event if the selected gameObject is not null
            if (EventSystem.current.currentSelectedGameObject != null && panelSamplers.activeSelf)
                EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();

            panelSamplers.SetActive(false);
        }

        // Select button with right joystick
        float posX = Input.GetAxis("RightHorizontal");
        float posY = Input.GetAxis("RightVertical");
        float angle = Mathf.Atan2(posX, posY) * Mathf.Rad2Deg;
        //print(angle);

        if (angle == 0)
            EventSystem.current.SetSelectedGameObject(null, null);
        else if (angle < 0)
            EventSystem.current.SetSelectedGameObject(buttonsSampler[0], null);
        else
            EventSystem.current.SetSelectedGameObject(buttonsSampler[1], null);

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


    // Add a synth to the scene
    public void AddSynth(string name)
    {
        OSCHandler.Instance.SendMessageToClient("SuperCollider", name, 1.0);
        print("New synth added");

        x++;
        if (x > width)
        {
            x = 1;
            y++;

            if (y > height) return; // Cannot add more instruments, scenario full
        }

        // Instantiate instrument in the scenario
        switch (name)
        {
            case "/sin":
                Instantiate(sinPrefab, new Vector3(x, 0, -y), Quaternion.identity);
                break;
            case "/square":
                Instantiate(squarePrefab, new Vector3(x, 0, -y), Quaternion.identity);
                break;
            default:
                break;
        }


    }
}
