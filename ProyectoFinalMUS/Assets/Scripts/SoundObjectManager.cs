using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundObjectManager : MonoBehaviour
{
    // Scenario width and height
    public int width = 8;
    public int height = 4;

    // Sound Objects
    GameObject[,] soundObjects;
    
    void Start()
    {
        soundObjects = new GameObject[height, width];
    }
    
    void Update()
    {
        
    }

    public void addSoundObject(int x, int y, GameObject soundObj)
    {
        // TODO: Buscar un hueco libre en la matriz
        if (soundObjects[y, x] == null) 
            soundObjects[y, x] = soundObj;
        else
            print("Error: Place in object array already occupied by an object\n");
    }

    public void removeSoundObject(int x, int y)
    {
        if (soundObjects[y, x] != null)
        {
            Destroy(soundObjects[y, x]);
            soundObjects[y, x] = null;
        }
        else
            print("Error: No object at given location\n");
    }

    public GameObject[,] getSoundObjects() 
    {
        return soundObjects;
    }
}
