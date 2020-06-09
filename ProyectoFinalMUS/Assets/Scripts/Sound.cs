using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    public List<Vector3> items = new List<Vector3>(); // time pos, freq, start/stop
    public TimeController timeControl;
    public string name = "";
    int i = 0;

    //MeshRenderer meshRenderer;

    //Material originalMaterial;
    //public Material muteMaterial;

    public GameObject person;

    bool muted = false;
    public bool isPreset = false;

    void Start()
    {
        //meshRenderer = GetComponent<MeshRenderer>();
        //originalMaterial = meshRenderer.material;
    }

    void Update()
    {
        if (!isPreset)
        {
            if (!muted)
                playItems();

            if (timeControl.time <= 0.01f)
                i = 0;
        }
    }

    void playItems()
    {
        if (i < items.Count && (timeControl.time <= items[i].x + 0.01f && timeControl.time >= items[i].x - 0.01f))
        {
            OSCHandler.Instance.SendSoundMessageToClient("SuperCollider", name, items[i].z, items[i].y);
            i++;
        }
    }

    public void mute(int index)
    {
        //meshRenderer.material = muteMaterial;
        person.SetActive(false);
        muted = true;

        if (isPreset)
            OSCHandler.Instance.SendSoundVolumeMessage("SuperCollider", index, 0.0f);
    }

    public void desmute(int index)
    {
        //meshRenderer.material = originalMaterial;
        person.SetActive(true);
        muted = false;

        if (isPreset)
            OSCHandler.Instance.SendSoundVolumeMessage("SuperCollider", index, 1.0f);
    }

    public bool isMuted()
    {
        return muted;
    }
}
