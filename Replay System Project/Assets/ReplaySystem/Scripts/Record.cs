using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Record : MonoBehaviour
{

    public ReplayManager replay;


    private Rigidbody rigidBody;

    //List of recorded Frames 
    private List<Frame> frames = new List<Frame>();

    //Maximum amount of frames that can be stored
    public int maxLength = 300;

    //when true the gameobject will be recorded
    public bool record = false;


    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        if(replay != null)
        {
            replay.AddRecord(this);
            maxLength = replay.GetMaxLength();
        }
    }

    void Update()
    {
        if(replay != null)
            record = !replay.ReplayMode();
        
        if(!record)
        {
            if(rigidBody != null ) rigidBody.isKinematic = true;
        }
        else
        {
            //if it is in replay mode, don't use physics
            if (rigidBody != null) rigidBody.isKinematic = false;
        }
    }

    void FixedUpdate()
    {
        //record states if we are not in replay mode
        if(record)
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
        return index >= frames.Count ? null : frames[index];
    }

    public int GetLength()
    {
        return frames.Count;
    }
}
