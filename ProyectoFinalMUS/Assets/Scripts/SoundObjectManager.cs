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
    string[,] objectTypes;
    //bool[,] muted;

    int soundCounter = 0;

    public AudioClip acceptClip;
    public AudioClip removeClip;
    AudioSource audioUI;

    void Start()
    {
        soundObjects = new GameObject[height, width];
        objectTypes = new string[height, width];
        //muted = new bool[height, width];
        audioUI = GetComponent<AudioSource>();
    }

    public void addSoundObject(string name, GameObject soundObj)
    {
        int x = (soundCounter) % width;
        int y = (soundCounter) / width;
        if (y >= height) return;

        if (soundObjects[y, x] == null)
        {
            OSCHandler.Instance.SendMessageToClient("SuperCollider", name, 1.0, soundCounter, -1);

            soundObjects[y, x] = Instantiate(soundObj, new Vector3(x + 1, 0, -y), Quaternion.identity);
            objectTypes[y, x] = name;

            soundCounter++;

            audioUI.PlayOneShot(acceptClip);
        }
        else
            print("Error: Place in object array already occupied by an object\n");
    }

    public void removeSoundObject(int x, int y)
    {
        if (soundObjects[y, x] != null)
        {
            OSCHandler.Instance.SendMessageToClient("SuperCollider", objectTypes[y, x], -1.0, x + y * width, -1);

            Destroy(soundObjects[y, x]);
            soundObjects[y, x] = null;
            objectTypes[y, x] = null;
            rearrangeObjects(x, y);
            soundCounter--;

            audioUI.PlayOneShot(removeClip);
        }
        else
            print("Error: No object at given location\n");
    }

    public bool isMuted(int x, int y)
    {
        return soundObjects[y, x].GetComponent<Sound>().isMuted();
    }

    public void muteSoundObject(int x, int y)
    {
        if (soundObjects[y, x] != null)
        {
            //OSCHandler.Instance.SendMessageToClient("SuperCollider", objectTypes[y, x], -1.0, x + y * width, -1);

            /*Destroy(soundObjects[y, x]);
            soundObjects[y, x] = null;
            objectTypes[y, x] = null;
            rearrangeObjects(x, y);
            soundCounter--;*/

            Sound soundComp = soundObjects[y, x].GetComponent<Sound>();
            if (soundComp != null)
                soundComp.mute(x + (y * width));

            //muted[y, x] = true;
            audioUI.PlayOneShot(removeClip);
        }
        else
            print("Error: No object at given location\n");
    }

    public void desmuteSoundObject(int x, int y)
    {
        if (soundObjects[y, x] != null)
        {
            Sound soundComp = soundObjects[y, x].GetComponent<Sound>();
            if (soundComp != null && soundComp.isMuted())
                soundComp.desmute(x + (y * width));

            audioUI.PlayOneShot(removeClip);
        }
        else
            print("Error: No object at given location\n");
    }

    public void soloSoundObject(int x, int y)
    {
        GameObject soloObject = soundObjects[y, x];
        Sound soloSound = soloObject.GetComponent<Sound>();
        if (soloSound.isMuted())
            soloSound.desmute(x + (y * width));

        for (int i = 0; i < soundObjects.GetLength(1); i++)
        {
            for (int j = 0; j < soundObjects.GetLength(0); j++)
            {
                if (soundObjects[j, i] != null)
                {
                    if (soundObjects[j, i] != soloObject)//j != y && i != x)
                    {
                        Sound sound = soundObjects[j, i].GetComponent<Sound>();
                        if (sound != null) sound.mute(i + (j * width));
                    }
                }
            }
        }
    }

    public GameObject[,] getSoundObjects()
    {
        return soundObjects;
    }

    public int getNumOfSounds()
    {
        return soundCounter;
    }

    void rearrangeObjects(int x, int y)
    {
        bool reachedEnd = false;
        while (!reachedEnd)
        {
            int newX = x + 1, newY = y;
            if (newX >= width)
            {
                newX = 0;   // Reached end of current row
                newY++;
            }

            if (newY >= height) reachedEnd = true; // Reached end of objets array
            else
            {
                GameObject nextObject = soundObjects[newY, newX];
                if (nextObject != null)
                {
                    soundObjects[y, x] = nextObject;
                    objectTypes[y, x] = objectTypes[newY, newX];
                    soundObjects[newY, newX] = null;

                    // Move object to new location
                    soundObjects[y, x].transform.position = new Vector3(x + 1, 0, -y);

                    // Tell SC to change order of objects too
                    int index = newX + (newY * width), newIndex = x + (y * width);
                    OSCHandler.Instance.SendSoundMoveMessage("SuperCollider", index, newIndex);
                    x = newX; y = newY;
                }
                else
                    reachedEnd = true;
            }
        }
    }
}
