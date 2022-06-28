using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frame
{
    //transform data
    Vector3 pos, scale;
    Quaternion rot;

    //RigidBody velocities
    Vector3 RBvelocity, RBAngVelocity;

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

    //RigidBody set velocity data
    public void SetRBVelocities(Vector3 v, Vector3 aV)
    {
        RBvelocity = v;
        RBAngVelocity = aV;
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

    //RigidBody getter
    public Vector3 GetRBVelocity() { return RBvelocity; }
    public Vector3 GetRBAngularVelocity() { return RBAngVelocity; }
    
    //Audio getter
    public AudioData GetAudioData() { return audio; }
   
    //Particle getter
    public float ParticleTime() { return particleTime; }
}
