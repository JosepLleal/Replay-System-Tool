using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frame
{
    Vector3 pos, scale;
    Quaternion rot;

    List<AnimationRecord> animationRecords;

    public Frame(Vector3 position, Quaternion rotation, Vector3 scale_, List<AnimationRecord> aRecords)
    {
        pos = position;
        rot = rotation;
        scale = scale_;

        animationRecords = aRecords;
    }


    public Vector3 GetPosition() { return pos; }
    public Vector3 GetScale() { return scale; }
    public Quaternion GetRotation() { return rot; }
    public List<AnimationRecord> GetAnimationRecords() { return animationRecords; }


    
}
