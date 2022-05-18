using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Record : MonoBehaviour
{

    public ReplayManager replay;

    private Rigidbody rigidBody;
    private Vector3 RBvelocity = Vector3.zero;
    private Vector3 RBAngVelocity = Vector3.zero;

    private Animator animator;

    //List of recorded Frames 
    private List<Frame> frames = new List<Frame>();

    //Maximum amount of frames that can be stored
    public int maxLength = 1000;
    //when true the gameobject will be recorded
    private bool record = false;
    //animator recording
    private bool startedRecording = false;


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
        if (replay != null)
            record = !replay.ReplayMode();
           
        if(record)
        {
            if (animator != null && startedRecording == false)
            {
                animator.StartRecording(maxLength);
                startedRecording = true;
            }

            //record states if we are not in replay mode
            Frame frame = new Frame(transform.position, transform.rotation, transform.localScale);
            AddFrame(frame);
        }
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

    public void ClearFrameList()
    {
        frames.Clear();
    }

    public void SetStartRecording(bool b)
    {
        startedRecording = b;
    }

    //when enter replay mode set to TRUE and when exit set to FALSE
    public void SetKinematic(bool b)
    {
        if(rigidBody != null)
        {
            if(b == true)
            {
                RBvelocity = rigidBody.velocity;
                RBAngVelocity = rigidBody.angularVelocity;
            }
            else
            {
                rigidBody.velocity = RBvelocity;
                rigidBody.angularVelocity = RBAngVelocity;
            }

            rigidBody.isKinematic = b;
        }
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
