using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Record : MonoBehaviour
{

    public ReplayManager replay;

    private Rigidbody rigidBody;
    private Animator animator;

    //List of recorded Frames 
    private List<Frame> frames = new List<Frame>();

    //Maximum amount of frames that can be stored
    public int maxLength = 1000;
    //when true the gameobject will be recorded
    public bool record = false;
    public bool startedRecording = false;


    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

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
            //if it is in replay mode, don't use physics
            if(rigidBody != null ) 
                rigidBody.isKinematic = true;
        }
        else
        {
            
            if(rigidBody != null) 
                rigidBody.isKinematic = false;

            if (animator != null && !startedRecording)
            {
                animator.StartRecording(maxLength);
                startedRecording = true;
            }

            //record states if we are not in replay mode
            Frame frame = new Frame(transform.position, transform.rotation, transform.localScale);
            AddFrame(frame);
        }
    }

    void FixedUpdate()
    {
        
       
    }

    //Add frame, if list has maxLength remove first element
    void AddFrame(Frame frame)
    {
        if(GetLength() >= maxLength)
        {
            frames.RemoveAt(0);
        }

        frames.Add(frame);

    }

    // GETTERS
    public Frame GetFrameAtIndex(int index)
    {
        return index >= frames.Count ? null : frames[index];
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public int GetLength()
    {
        return frames.Count;
    }

    public Animator GetAnimator()
    {
        return animator;
    }
}
