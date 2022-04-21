using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Record : MonoBehaviour
{

    public ReplayManager replay;

    private Rigidbody rigidBody;
    private List<Frame> frames = new List<Frame>();

    int maxLength;

    public bool record = false;


    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        replay.AddRecord(this);
        maxLength = replay.GetMaxLength();

    }

    void Update()
    {
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