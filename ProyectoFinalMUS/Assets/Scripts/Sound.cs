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

    MeshRenderer meshRenderer;

    Material originalMaterial;
    public Material muteMaterial;

    bool muted = false;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originalMaterial = meshRenderer.material;
    }

    void Update()
    {
        if(!muted)
            playItems();

        if (timeControl.time <= 0.01f)
            i = 0;
    }

    void playItems()
    {
        if (i < items.Count && (timeControl.time <= items[i].x + 0.01f && timeControl.time >= items[i].x - 0.01f))
        {
            OSCHandler.Instance.SendSoundMessageToClient("SuperCollider", name, items[i].z, items[i].y);
            i++;
        }
    }

    public void mute()
    {
        meshRenderer.material = muteMaterial;
        muted = true;
    }

    public void desmute()
    {
        meshRenderer.material = originalMaterial;
        muted = false;
    }

    public bool isMuted()
    {
        return muted;
    }
}
