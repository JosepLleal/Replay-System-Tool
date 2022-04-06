using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frame
{
    GameObject go;

    Vector3 pos, scale;
    Quaternion rot;

    public Frame(GameObject gameobject, Vector3 position, Quaternion rotation, Vector3 scale_)
    {
        go = gameobject;

        pos = position;
        rot = rotation;
        scale = scale_;
    }


    public Vector3 GetPosition() { return pos; }
    public Vector3 GetScale() { return scale; }
    public Quaternion GetRotation() { return rot; }
    public GameObject GetGO() { return go; }
    
}
