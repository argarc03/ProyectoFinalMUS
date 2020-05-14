using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    // Samplers menu & buttons
    public GameObject panelSamplers;

    public List<GameObject> ButtonsSampler = new List<GameObject>();
    int currentSelected = 0;

    // Use this for initialization
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(ButtonsSampler[0], null);
        panelSamplers.SetActive(false);

        // Calculate 
    }

    void Update()
    {
        // Show/hide the samplers panel
        if (Input.GetAxis("RightTrigger") != 0)
            panelSamplers.SetActive(true);
        else
            panelSamplers.SetActive(false);


        // Select button with right joystick
        float posX = Input.GetAxis("RightHorizontal");
        float posY = Input.GetAxis("RightVertical");

        float angle = Mathf.Atan2(posX, posY) * Mathf.Rad2Deg;

        print(angle);

        if(angle < 0)
            EventSystem.current.SetSelectedGameObject(ButtonsSampler[0], null);
        else
            EventSystem.current.SetSelectedGameObject(ButtonsSampler[1], null);

        /*if (Input.GetKeyUp(KeyCode.X))
        {

            OSCHandler.Instance.SendMessageToClient("SuperCollider", "/synth", -1.0);
            print("Synth removed");
        }*/
    }


    // Add a synth to the scene
    public void AddSynth()
    {
        OSCHandler.Instance.SendMessageToClient("SuperCollider", "/synth", 1.0);
        print("New synth added");
    }
}
