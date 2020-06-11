using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    public List<Vector3> items = new List<Vector3>(); // time pos, freq, start/stop
    public TimeController timeControl;
    public string name = "";
    public int objectIndex;
    int i = 0;

    MeshRenderer meshRenderer;

    Material[] originalMaterials = new Material[4];
    public Material muteMaterial;
    Material[] muteMaterials = new Material[4];

    public GameObject person;

    bool muted = false;
    public bool isPreset = false;

    void Start()
    {
        meshRenderer = person.GetComponent<MeshRenderer>();
        List<Material> aux = new List<Material>();
        meshRenderer.GetMaterials(aux);

        for (int i = 0; i < aux.Count; i++)
            originalMaterials[i] = aux[i];

        for (int j = 0; j < originalMaterials.Length; j++)
            muteMaterials[j] = muteMaterial;
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
            OSCHandler.Instance.SendSoundMessageToClient("SuperCollider", name, items[i].z, items[i].y, objectIndex);
            i++;
        }
    }

    public void mute(int index)
    {
        meshRenderer.materials = muteMaterials;
        muted = true;

        if (isPreset)
            OSCHandler.Instance.SendSoundVolumeMessage("SuperCollider", index, 0.0f);
        else if (i >= 0 && i < items.Count)
            OSCHandler.Instance.SendSoundMessageToClient("SuperCollider", name, -1, items[i].y, objectIndex);
    }

    public void desmute(int index)
    {
        meshRenderer.materials = originalMaterials;
        muted = false;

        if (isPreset)
            OSCHandler.Instance.SendSoundVolumeMessage("SuperCollider", index, 1.0f);
    }

    public bool isMuted()
    {
        return muted;
    }
}
