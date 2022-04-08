using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Record : MonoBehaviour
{

    public ReplayManager replay;

    private Rigidbody rigidBody;
    private List<Frame> frames = new List<Frame>();

    int maxLength;


    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        replay.AddRecord(this);
        maxLength = replay.GetMaxLength();

    }

    void Update()
    {
        
        if(replay.ReplayMode())
        {
            if(rigidBody != null ) rigidBody.isKinematic = true;
        }
        else
        {
            //if it is in replay mode don't use physics
            if (rigidBody != null) rigidBody.isKinematic = false;
        }
    }

    void FixedUpdate()
    {
        //save states if we are not in replay mode
        if(replay.ReplayMode() == false)
        {
            Frame frame = new Frame(gameObject, transform.position, transform.rotation, transform.localScale);
            AddFrame(frame);
        }
       
    }

    void AddFrame(Frame frame)
    {
        if(GetLength() >= maxLength)
        {
            frames.RemoveAt(0);
        }

        frames.Add(frame);

    }

    public Frame GetFrameAtIndex(int index)
    {
        return frames[index];
    }

    public int GetLength()
    {
        return frames.Count - 1;
    }
}
