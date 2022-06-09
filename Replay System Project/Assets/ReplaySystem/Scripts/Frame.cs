using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frame
{
    //transform data
    Vector3 pos, scale;
    Quaternion rot;

    //audio data
    AudioData audio;

    //particles data
    float particleTime;

    //Constructor
    public Frame(Vector3 position, Quaternion rotation, Vector3 scale_)
    {
        pos = position;
        rot = rotation;
        scale = scale_;
    }


    //audio set data
    public void SetAudioData(AudioData data)
    {
        audio = data;
    }

    //particle set data
    public void SetParticleData(float time)
    {
        particleTime = time;
    }

    //Getters
    public Vector3 GetPosition() { return pos; }
    public Vector3 GetScale() { return scale; }
    public Quaternion GetRotation() { return rot; }
    //Audio getter
    public AudioData GetAudioData() { return audio; }
    //Particle getter
    public float ParticleTime() { return particleTime; }



}
