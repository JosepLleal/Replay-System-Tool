using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frame
{
    Vector3 pos, scale;
    Quaternion rot;

    AudioData audio;

    //Constructor
    public Frame(Vector3 position, Quaternion rotation, Vector3 scale_)
    {
        pos = position;
        rot = rotation;
        scale = scale_;
    }


    public void SetAudioData(AudioData data)
    {
        audio = data;
    }

    //Getters
    public Vector3 GetPosition() { return pos; }
    public Vector3 GetScale() { return scale; }
    public Quaternion GetRotation() { return rot; }
    public AudioData GetAudioData() { return audio; }

    

}
