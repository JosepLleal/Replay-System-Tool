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
    private List<AnimationRecord> animationRecords;

    //Maximum amount of frames that can be stored
    public int maxLength = 300;
    //when true the gameobject will be recorded
    public bool record = false;


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
            //Create list of animation values for each frame 
            animationRecords = new List<AnimationRecord>();

            if (animator != null)
            {
                foreach(var param in animator.parameters)
                {
                    AddAnimRecord(param);
                }
            }

            Frame frame = new Frame(gameObject, transform.position, transform.rotation, transform.localScale, animationRecords);
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

    void AddAnimRecord(AnimatorControllerParameter p)
    {
        if (p.type == AnimatorControllerParameterType.Bool)
            animationRecords.Add(new AnimationRecord(p.name, animator.GetBool(p.name), p.type));
        else if (p.type == AnimatorControllerParameterType.Float)
            animationRecords.Add(new AnimationRecord(p.name, animator.GetFloat(p.name), p.type));
        else if (p.type == AnimatorControllerParameterType.Int)
            animationRecords.Add(new AnimationRecord(p.name, animator.GetInteger(p.name), p.type));
    }

    // GETTERS
    public Frame GetFrameAtIndex(int index)
    {
        return index >= frames.Count ? null : frames[index];
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
