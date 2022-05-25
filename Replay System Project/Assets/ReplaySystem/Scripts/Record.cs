using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Record : MonoBehaviour
{

    public ReplayManager replay;

    //List of recorded Frames 
    private List<Frame> frames = new List<Frame>();

    //RB recording
    private Rigidbody rigidBody;
    private Vector3 RBvelocity = Vector3.zero;
    private Vector3 RBAngVelocity = Vector3.zero;

    //animator recording
    private Animator animator;
    private bool startedRecording = false;

    //AudioSource recording
    private AudioSource audio;
    private bool audioPlay = false;
    private bool audioStarted = false;

    //Particle system
    private ParticleSystem particle;


    //Maximum amount of frames that can be stored
    public int maxLength = 1000;
    //when true the gameobject will be recorded
    private bool record = false;
    


    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
        particle = GetComponent<ParticleSystem>(); 

        if (replay != null)
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
            //record states if we are not in replay mode
            Frame frame = new Frame(transform.position, transform.rotation, transform.localScale);

            //animations
            RecordAnimation();

            //record audio data
            RecordAudio(frame);

            //record particle data
            RecordParticle(frame);

            //Add new frame to the list
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

    //Record Animation
    void RecordAnimation()
    {
        if (animator != null && startedRecording == false)
        {
            animator.StartRecording(maxLength);
            startedRecording = true;
        }
    }

    //Record Audio
    void RecordAudio(Frame frame)
    {
        if (audio != null)
        {
            if (audio.isPlaying && audioStarted == false)
            {
                audioStarted = true;
                audioPlay = true;
            }
            else if (audioStarted && audioPlay)
            {
                audioPlay = false;
            }
            else if (audio.isPlaying == false && audioStarted)
            {
                audioStarted = false;
            }

            frame.SetAudioData(new AudioData(audioPlay, audio.pitch, audio.volume, audio.panStereo, audio.spatialBlend, audio.reverbZoneMix));
        }
    }

    //Record Particle
    void RecordParticle(Frame frame)
    {
        if (particle != null)
        {

            if(particle.isEmitting)
                frame.SetParticleData(particle.time);
            else
                frame.SetParticleData(0f);
        }
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
                //saving speed values of RB
                RBvelocity = rigidBody.velocity;
                RBAngVelocity = rigidBody.angularVelocity;
            }
            else
            {
                //applaying speed values to RB
                rigidBody.velocity = RBvelocity;
                rigidBody.angularVelocity = RBAngVelocity;
            }

            rigidBody.isKinematic = b;
        }
    }

    // GETTERS
    public Frame GetFrameAtIndex(int index) { return index >= frames.Count ? null : frames[index]; }

    public GameObject GetGameObject() { return gameObject; }

    public int GetLength() { return frames.Count; }

    public Animator GetAnimator() { return animator; }

    public AudioSource GetAudioSource() { return audio; }

    public ParticleSystem GetParticle() { return particle; }
}
