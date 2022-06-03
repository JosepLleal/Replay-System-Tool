using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Record : MonoBehaviour
{
    public ReplayManager replay;
    //Replaymanager name in Scene
    public string replayManagerName = "ReplayManager";

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
    private AudioSource audioSource;
    private bool audioPlay = false;
    private bool audioStarted = false;

    //Particle system recording
    private ParticleSystem particle;

    //Maximum amount of frames that can be stored
    public int maxLength = 1000;
    //when true the gameobject will be recorded
    private bool record = false;

    //Useful to know if it was instantiated during the recording
    private int numberFirstFrame;
    private bool instantiated = false;

    //Record Deleted while recording
    // if not deleted it will remain -1, if deleted it will take the frame where it was deleted
    private int recordDeletedFrame = -1;
    //deleted go
    private GameObject deletedGO;
    


    void Start()
    {
        //make sure replay is not NULL
        if(replay == null)
            replay = GameObject.Find(replayManagerName).GetComponent<ReplayManager>();

        //Get components
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        particle = GetComponent<ParticleSystem>(); 

        if (replay != null)
        {
            replay.AddRecord(this);
            maxLength = replay.GetMaxLength();

            //first frame initialization
            numberFirstFrame = replay.GetReplayLength();
            //look if it is an instantiated go
            if(numberFirstFrame != 0) instantiated = true;
        }
        else
            Debug.LogWarning("ReplayManager not found, make sure there is a replayManger in the scene. Make sure to assign it by drag and drop or by puting the correct replayManagerName");
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
        if (audioSource != null)
        {
            if (audioSource.isPlaying && audioStarted == false)
            {
                audioStarted = true;
                audioPlay = true;
            }
            else if (audioStarted && audioPlay)
            {
                audioPlay = false;
            }
            else if (audioSource.isPlaying == false && audioStarted)
            {
                audioStarted = false;
            }

            frame.SetAudioData(new AudioData(audioPlay, audioSource.pitch, audioSource.volume, audioSource.panStereo, audioSource.spatialBlend, audioSource.reverbZoneMix));
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

    //Prepare to record again
    public void ClearFrameList()
    {
        frames.Clear();
        numberFirstFrame = 0;
        instantiated = false;
    }

    //used for the animator recording
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

    //rearrange instantiation and deletion frames
    public void UpdateFramesNum()
    {
        if (replay.GetReplayLength() == maxLength)
        {
            //instantiated record
            if (numberFirstFrame > 0)
                numberFirstFrame--;
            
            //deleted record
            if (recordDeletedFrame != -1 && recordDeletedFrame > 0)
            {
                recordDeletedFrame--;

                //delete frames that are out of the replay
                if(instantiated == false || (instantiated == true && numberFirstFrame == 0))
                    frames.RemoveAt(0);
            }
        }
    }

    //SETTERS
    public void SetDeletedGameObject(GameObject go) { deletedGO = go; }
    public void SetRecordDeletedFrame(int frame) { recordDeletedFrame = frame; }

    // GETTERS
    public int GetLength() { return frames.Count; }
    public Frame GetFrameAtIndex(int index) 
    {
        if (index < 0) return null;

        return index >= frames.Count ? null : frames[index]; 
    }

    //instantiation and deletion
    public int GetFirstFrameIndex() { return numberFirstFrame; }
    public bool IsInstantiated() { return instantiated; }
    public int GetRecordDeletedFrame() { return recordDeletedFrame; }

    //records Go and deleted GO
    public GameObject GetGameObject() { return gameObject; }
    public GameObject GetDeletedGO() { return deletedGO; }

    // other recorded components
    public Animator GetAnimator() { return animator; }
    public AudioSource GetAudioSource() { return audioSource; }
    public ParticleSystem GetParticle() { return particle; }
}
