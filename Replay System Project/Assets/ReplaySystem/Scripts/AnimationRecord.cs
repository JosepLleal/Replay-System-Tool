using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRecord
{
    string name;
    float fValue;
    int iValue;
    bool bValue;

    AnimatorControllerParameterType animatorType;

    //Constructor with float
    public AnimationRecord(string n, float value, AnimatorControllerParameterType type)
    {
        name = n;
        fValue = value;
        animatorType = type;
    }

    //Constructor with int
    public AnimationRecord(string n, int value, AnimatorControllerParameterType type)
    {
        name = n;
        iValue = value;
        animatorType = type;
    }

    //Constructor with bool
    public AnimationRecord(string n, bool value, AnimatorControllerParameterType type)
    {
        name = n;
        bValue = value;
        animatorType = type;
    }

    public string GetName() { return name; }
    public float GetFloatValue() { return fValue; }
    public int GetIntValue() { return iValue; }
    public bool GetBoolValue() { return bValue; }
    public AnimatorControllerParameterType GetAnimatorType() { return animatorType; }


     


}
