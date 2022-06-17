using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplayManager : MonoBehaviour
{    
    public enum ReplayState { PAUSE, PLAYING, PLAY_REVERSE }

    //Main system variables
    [HideInInspector]
    public List<Record> records = new List<Record>();
    private bool isReplayMode = false;
    [Header("Maximum frames recorded")]
    [SerializeField]private int recordMaxLength = 3600; // 60fps * 60seconds = 3600 frames 
    private int frameIndex = 0;

    //States
    [HideInInspector]
    public ReplayState state = ReplayState.PAUSE;

    //replay speeds
    private float[] speeds = { 0.25f, 0.5f, 1.0f, 2.0f, 4.0f };
    private int speedIndex = 2;

    //UI elements
    private bool usingSlider = false;
    [Header("Replay System UI")]
    public Slider timeLine;
    public GameObject replayBoxUI;

    //Replay cameras
    private GameObject replayCam;
    private Camera[] cameras;
    private int cameraIndex = 0;

    //Deleted gameobjects pool
    private List<GameObject> DeletedPool = new List<GameObject>();

    private void Awake()
    {
        //needed to have a consistent frame rate
        Application.targetFrameRate = 60;
    }

    //Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (isReplayMode)
            {
                QuitReplayMode();
            }
            else
            {
                EnterReplayMode();
            }
        }

        if (isReplayMode)
        {
            // Replay playing 
            if (state == ReplayState.PLAYING && usingSlider == false)
            {
                //update slider value
                timeLine.value = frameIndex;

                if (frameIndex < recordMaxLength - 1 && frameIndex < timeLine.maxValue - 1)
                {
                    for (int i = 0; i < records.Count; i++)
                    {
                        //Check for instantiated and deleted GOs
                        int auxIndex = frameIndex - records[i].GetFirstFrameIndex();
                        HandleDeletedObjects(records[i], frameIndex);
                        HandleInstantiatedObjects(records[i], auxIndex);                        

                        //if record exists at frameIndex moment
                        if (IsRecordActiveInReplay(records[i], frameIndex))
                        {
                            //transforms
                            SetTransforms(records[i], auxIndex);

                            //animations 
                            Animator animator = records[i].GetAnimator();
                            if (animator != null)
                            {
                                float time = (animator.recorderStopTime - animator.recorderStartTime) / records[i].GetLength();

                                if (time > animator.recorderStopTime)
                                    time = animator.recorderStopTime;

                                animator.playbackTime += time;
                            }

                            //audios
                            AudioSource source = records[i].GetAudioSource();
                            if (source != null)
                            {
                                if (records[i].GetFrameAtIndex(auxIndex).GetAudioData().Playing())
                                    source.Play();

                                if (source.isPlaying)
                                    SetAudioProperties(source, records[i].GetFrameAtIndex(auxIndex).GetAudioData());
                            }

                            //particles
                            ParticleSystem particle = records[i].GetParticle();
                            if (particle != null)
                            {
                                if (records[i].GetFrameAtIndex(auxIndex).ParticleTime() != 0f && particle.isPlaying == false)
                                    particle.Play();

                                if (records[i].GetFrameAtIndex(auxIndex).ParticleTime() == 0 && particle.isPlaying)
                                    particle.Stop();
                            }
                        }
                    }
                    //advance frame
                    frameIndex++;
                }
                else
                    PauseResume();
            }
        }
        else //game is recording
        {
            for (int i = 0; i < records.Count; i++)
            {
                //the deletion of the record is already out of the replay
                if(records[i].GetRecordDeletedFrame() != -1 && records[i].GetRecordDeletedFrame() == 0)
                {
                    //DELETE GAMEOBJECT
                    GameObject delGO = records[i].GetDeletedGO();
                    Record r = delGO.GetComponent<Record>();
                    if (r != null)
                        records.Remove(r);

                    RemoveRecordsFromList(delGO);
                    DeletedPool.Remove(delGO);
                    Destroy(delGO);
                }

                //update instantiation and deletion frames
                records[i].UpdateFramesNum();
            }
                
        }
    }

    //-------------- FUNCTIONS TO ACTIVATE AND DEACTIVATE GAMEOBJECTS (FOR INSTANTIATION AND DELETION) ----------------//

    //This function is responsible for activating and deactivating instantiated GO, dependenig on the current time of the replay 
    void HandleInstantiatedObjects(Record rec, int index)
    {
        //get hierarchy highest parent
        GameObject go = rec.GetGameObject().transform.root.gameObject;

        //it has not been instantiated yet
        if (index < 0)
        {
            if (go.activeInHierarchy == true)
                go.SetActive(false);
        }
        else
        {
            //instantiate 
            if (go.activeInHierarchy == false)
            {
                //if it hasn't been deleted during recording
                if(rec.GetRecordDeletedFrame() == -1)
                {
                    go.SetActive(true);

                    Animator animator = rec.GetAnimator();
                    if (animator != null)
                    {
                        //start animator replayMode
                        animator.StartPlayback();
                        animator.playbackTime = animator.recorderStartTime;
                    }
                }
                else
                {
                    //if it hasn't already been deleted, but it will
                    if(frameIndex < rec.GetRecordDeletedFrame())
                    {
                        go.SetActive(true);

                        Animator animator = rec.GetAnimator();
                        if (animator != null)
                        {
                            //start animator replayMode
                            animator.StartPlayback();
                            animator.playbackTime = animator.recorderStartTime;
                        }
                    }
                }
                
            }
        }
    }

    //Function to activate and deactivate GameObjects that were deleted during the recording phase to simulate the deletion of them
    void HandleDeletedObjects(Record rec, int index)
    {
        //it has not been deleted
        if (rec.GetRecordDeletedFrame() == -1)
            return;

        //if it is not deleted yet based on index
        if (index < rec.GetRecordDeletedFrame())
        {
            //if it has not been instantiated activate gameobject
            if (rec.GetDeletedGO().activeInHierarchy == false && rec.IsInstantiated() == false)
            {
                rec.GetDeletedGO().SetActive(true);
                Animator animator = rec.GetAnimator();
                if (animator != null)
                {
                    //start animator replayMode
                    animator.StartPlayback();
                    animator.playbackTime = animator.recorderStartTime;
                }
            }

        }
        else //it has been deleted based on index so gameobject setActive to false
        {
            if (rec.GetDeletedGO().activeInHierarchy == true)
                rec.GetDeletedGO().SetActive(false);
        }
    }

    //Function that checks in the given frame (index), if the record is active
    bool IsRecordActiveInReplay(Record rec, int index)
    {
        bool ret = false;

        int instantiatedFrame = rec.GetFirstFrameIndex();
        int deletedFrame = rec.GetRecordDeletedFrame();

        //it has not been instantiated neither deleted
        if(rec.IsInstantiated() == false && deletedFrame == -1)
        {
            ret = true;
        }
        //it has been instantiated and deleted
        else if(rec.IsInstantiated() && deletedFrame != -1)
        {
            if(index >= instantiatedFrame && index < deletedFrame)
                ret = true;
        }
        //it has been only instantiated
        else if(rec.IsInstantiated())
        {
            if (index >= instantiatedFrame)
                ret = true;
        }
        //it has been only deleted
        else if (deletedFrame != -1)
        {
            if (index < deletedFrame)
                ret = true;
        }

        return ret;
    }


    //Custom function to delete gameobjects that are recorded.
    //REALLY IMPORTANT to use this function if the deleted go is using a record component
    public void DestroyRecordedGO(GameObject obj)
    {
        DeletedPool.Add(obj);
        obj.SetActive(false);

        Record r = obj.GetComponent<Record>();
        if (r != null)
        {
            r.SetRecordDeletedFrame(GetReplayLength() - 1);
            r.SetDeletedGameObject(obj);
        }

        SetDeleteChildrenRecords(obj, obj);         
    }

    //Set deleted frame and go deleted to childs with also records
    private void SetDeleteChildrenRecords(GameObject deletedGO, GameObject obj)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            GameObject child = obj.transform.GetChild(i).gameObject;

            Record r = child.GetComponent<Record>();
            if(r != null)
            {
                r.SetRecordDeletedFrame(GetReplayLength() - 1);
                r.SetDeletedGameObject(deletedGO);
            }               

            SetDeleteChildrenRecords(deletedGO, child);
        }
    }

    //function to remove all the deleted records from the list of records
    private void RemoveRecordsFromList(GameObject obj)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            GameObject child = obj.transform.GetChild(i).gameObject;
            Record r = child.GetComponent<Record>();
            if(r != null)
                records.Remove(r);

            RemoveRecordsFromList(child);
        }
    }

    //------------------------------------ END OF INSTANTIATION AND DELETION METHODS -------------------------------------//

    //Add record to records list
    public void AddRecord(Record r)
    {
        records.Add(r);
    }

    //Get max length of recordable frames
    public int GetMaxLength() 
    {
        return recordMaxLength;
    }

    //Actual replay length
    public int GetReplayLength()
    {
        int value = 0;

        for (int i = 0; i < records.Count; i++)
            if (records[i].GetLength() > value)
                value = records[i].GetLength();

        return value;
    }

    //Return if in replayMode or not
    public bool ReplayMode()
    {
        return isReplayMode;
    }

    //set transforms from the frame at record[index]
    void SetTransforms(Record rec, int index)
    {
        GameObject go = rec.GetGameObject();

        Frame f = rec.GetFrameAtIndex(index);
        if (f == null) return;

        go.transform.position = f.GetPosition();
        go.transform.rotation = f.GetRotation();
        go.transform.localScale = f.GetScale();
    }

    //set audio source parameters from audio data
    void SetAudioProperties(AudioSource source, AudioData data)
    {
        source.pitch = data.GetPitch();
        source.volume = data.GetVolume();
        source.panStereo = data.GetPanStereo();
        source.spatialBlend = data.GetSpatialBlend();
        source.reverbZoneMix = data.GetReverbZoneMix();
    }

    //Instantiate temporary camera for replay
    void InstantiateReplayCamera()
    {
        replayCam = new GameObject("ReplayCamera");
        replayCam.AddComponent<Camera>();
        replayCam.AddComponent<ReplayCamera>();

        cameras = Camera.allCameras;
    }

    //Delete instantiated replay camera
    void DeleteReplayCam()
    {
        Destroy(replayCam);
    }

    //Slider event: has been clicked
    public void SliderClick()
    {
        usingSlider = true;
    }

    //Slider event: has been released
    public void SliderRelease()
    {
        //set frame to slider value
        frameIndex = (int)timeLine.value;

        for (int i = 0; i < records.Count; i++)
        {
            //Check for instantiated and deleted GO
            int auxIndex = frameIndex - records[i].GetFirstFrameIndex();
            HandleDeletedObjects(records[i], frameIndex);
            HandleInstantiatedObjects(records[i], auxIndex);
            

            if (IsRecordActiveInReplay(records[i], frameIndex))
            {
                SetTransforms(records[i], auxIndex);

                Animator animator = records[i].GetAnimator();
                //set animations replay time: time = startTime + frame * dT
                if (animator != null) 
                {

                    float time = animator.recorderStartTime + (animator.recorderStopTime - animator.recorderStartTime) / records[i].GetLength() * auxIndex;

                    if (time > animator.recorderStopTime)
                        time = animator.recorderStopTime;

                    animator.playbackTime = time;
                }
                    
                //Audios
                AudioSource source = records[i].GetAudioSource();
                if (source != null)
                {
                    if (records[i].GetFrameAtIndex(auxIndex).GetAudioData().Playing())
                        source.Play();

                    if (source.isPlaying)
                        SetAudioProperties(source, records[i].GetFrameAtIndex(auxIndex).GetAudioData());
                }

                //Particles
                ParticleSystem part = records[i].GetParticle();
                if (part != null)
                {
                    if (part.isPlaying)
                    {
                        part.Stop();
                        part.Clear();
                    }

                    if (records[i].GetFrameAtIndex(auxIndex).ParticleTime() != 0f)
                    {
                        part.Simulate(records[i].GetFrameAtIndex(auxIndex).ParticleTime());
                        part.Play();
                    }
                }
            }
        }

        usingSlider = false;
    }    

   

    //------------- REPLAY TOOLS -------------------//

    // Start replay mode
    public void EnterReplayMode()
    {
        isReplayMode = true;

        //temporary replay camera instantiation
        InstantiateReplayCamera();
        //initial frameIndex 
        frameIndex = 0;

        //slider max value
        timeLine.maxValue = GetReplayLength();
        timeLine.value = frameIndex;

        //Enable UI
        UIvisibility(true);

        state = ReplayState.PAUSE;
        Time.timeScale = 0f;

        //set gameobjects states to starting frame
        for (int i = 0; i < records.Count; i++)
        {
            records[i].SetKinematic(true);

            int auxIndex = frameIndex - records[i].GetFirstFrameIndex();

            if(IsRecordActiveInReplay(records[i],frameIndex))
            {
                SetTransforms(records[i], auxIndex);

                //animations
                Animator animator = records[i].GetAnimator();
                if (animator != null)
                {
                    //stop recording animator
                    animator.StopRecording();

                    //start animator replayMode
                    animator.StartPlayback();
                    animator.playbackTime = animator.recorderStartTime;
                }

                //particles
                ParticleSystem part = records[i].GetParticle();
                if (part != null)
                {
                    if (part.isPlaying)
                    {
                        part.Stop();
                        part.Clear();
                    }

                    if (records[i].GetFrameAtIndex(auxIndex).ParticleTime() != 0f)
                    {
                        part.Simulate(records[i].GetFrameAtIndex(auxIndex).ParticleTime());
                        part.Play();
                    }
                }
            }

            //Check for instantiated and deleted GO
            HandleInstantiatedObjects(records[i], auxIndex);
            HandleDeletedObjects(records[i], frameIndex);
        }
    }

    //Exit replay mode
    public void QuitReplayMode()
    {
        //set gameobjects transforms back to current state
        for (int i = 0; i < records.Count; i++)
        {
            records[i].SetKinematic(false);
            
            //Check for instantiated GO
            HandleInstantiatedObjects(records[i], records[i].GetLength() - 1);
            SetTransforms(records[i], records[i].GetLength() - 1);

            //reset animations
            Animator animator = records[i].GetAnimator();
            if (animator != null)
            {
                animator.playbackTime = animator.recorderStopTime;
                animator.StopPlayback();
                records[i].SetStartRecording(false);
            }

            //reset particles
            ParticleSystem part = records[i].GetParticle();
            if (part != null)
            {
                if (part.isPlaying)
                {
                    part.Stop();
                    part.Clear();
                }

                if (records[i].GetFrameAtIndex(records[i].GetLength() - 1).ParticleTime() != 0f)
                {
                    part.Simulate(records[i].GetFrameAtIndex(records[i].GetLength() - 1).ParticleTime());
                    part.Play();
                }
            }
            records[i].ClearFrameList();
        }

        //destroy deleted gameobject and records
        foreach (GameObject go in DeletedPool)
        {
            Record r = go.GetComponent<Record>();
            if (r != null)
                records.Remove(r);

            RemoveRecordsFromList(go);
            Destroy(go);
        }
        DeletedPool.Clear();


        DeleteReplayCam();
        //Disable UI
        UIvisibility(false);

        isReplayMode = false;

        //optional
        Time.timeScale = 1f;
    }

    //Start replay from begining
    public void RestartReplay()
    {
        frameIndex = 0;
        timeLine.value = frameIndex;

        for (int i = 0; i < records.Count; i++)
        {
            int auxIndex = frameIndex - records[i].GetFirstFrameIndex();

            //Check for instantiated and deleted GO
            HandleDeletedObjects(records[i], frameIndex);
            HandleInstantiatedObjects(records[i], auxIndex);

            if(IsRecordActiveInReplay(records[i], frameIndex))
            {
                SetTransforms(records[i], auxIndex);

                //animations
                Animator animator = records[i].GetAnimator();
                if (animator != null)
                {
                    animator.playbackTime = animator.recorderStartTime;
                }

                //particles
                ParticleSystem part = records[i].GetParticle();
                if (part != null)
                {
                    if (part.isPlaying)
                    {
                        part.Stop();
                        part.Clear();
                    }

                    if (records[i].GetFrameAtIndex(auxIndex).ParticleTime() != 0f)
                    {
                        part.Simulate(records[i].GetFrameAtIndex(auxIndex).ParticleTime());
                        part.Play();
                    }
                }
            }            
        }
    }

    //Pause / Resume function
    public void PauseResume()
    {
        if (state == ReplayState.PAUSE)
        {
            state = ReplayState.PLAYING;
            Time.timeScale = 1;
        }            
        else
        {
            state = ReplayState.PAUSE;
            Time.timeScale = 0;
        }
    }

    //Advances one frame 
    public void GoForward()
    {
        state = ReplayState.PAUSE;
        Time.timeScale = 0;
        
        if (frameIndex < recordMaxLength - 1) 
        {
            frameIndex++;
            timeLine.value = frameIndex;

            for (int i = 0; i < records.Count; i++)
            {
                //Check for instantiated and deleted GO
                HandleDeletedObjects(records[i], frameIndex);
                HandleInstantiatedObjects(records[i], frameIndex - records[i].GetFirstFrameIndex());
                int auxIndex = frameIndex - records[i].GetFirstFrameIndex();

                if(IsRecordActiveInReplay(records[i], frameIndex))
                {
                    SetTransforms(records[i], auxIndex);

                    //animations
                    Animator animator = records[i].GetAnimator();
                    if (animator != null)
                    {
                        float time = (animator.recorderStopTime - animator.recorderStartTime) / records[i].GetLength();

                        if (time > animator.recorderStopTime)
                            time = animator.recorderStopTime;

                        animator.playbackTime += time;
                    }

                    //particles
                    ParticleSystem part = records[i].GetParticle();
                    if (part != null)
                    {
                        if (records[i].GetFrameAtIndex(auxIndex).ParticleTime() != 0f)
                        {
                            part.Simulate(records[i].GetFrameAtIndex(auxIndex).ParticleTime());
                            part.Play();
                        }

                    }
                }
            }
        }
    }

    //Back one frame
    public void GoBack()
    {
        state = ReplayState.PAUSE;
        Time.timeScale = 0;

        if (frameIndex > 0)
        {
            frameIndex--;
            timeLine.value = frameIndex;

            for (int i = 0; i < records.Count; i++)
            {
                //Check for instantiated and deleted GO
                HandleDeletedObjects(records[i], frameIndex);
                HandleInstantiatedObjects(records[i], frameIndex - records[i].GetFirstFrameIndex());
                int auxIndex = frameIndex - records[i].GetFirstFrameIndex();

                if(IsRecordActiveInReplay(records[i], frameIndex))
                {
                    SetTransforms(records[i], auxIndex);

                    //animations
                    Animator animator = records[i].GetAnimator();
                    if (animator != null)
                        animator.playbackTime -= (animator.recorderStopTime - animator.recorderStartTime) / (float)records[i].GetLength();

                    //particles
                    ParticleSystem part = records[i].GetParticle();
                    if (part != null)
                    {

                        if (records[i].GetFrameAtIndex(auxIndex).ParticleTime() != 0f)
                        {
                            part.Simulate(records[i].GetFrameAtIndex(auxIndex).ParticleTime());
                            part.Play();
                        }
                    }
                }
            }
        }
    }

    //Increase replay speed
    public void SpeedUp()
    {
        if(speedIndex < speeds.Length - 1)
            speedIndex++;

        //TODO: figure out how to treat speed variations
        //Time.timeScale = speeds[speedIndex];
    }

    //Decrease replay speed
    public void SpeedDown()
    {
        if (speedIndex > 0)
            speedIndex--;

        //TODO: figure out how to treat speed variations
        //Time.timeScale = speeds[speedIndex];
    }

    //Change to next camera in scene
    public void NextCamera()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] == Camera.main)
                cameraIndex = i;
        }

        cameraIndex++;

        if(cameras.Length == cameraIndex)
        {
            cameraIndex = 0;
            cameras[cameras.Length - 1].enabled = false;
            cameras[cameraIndex].enabled = true;
        }
        else
        {
            cameras[cameraIndex - 1].enabled = false;
            cameras[cameraIndex].enabled = true;
        }
    }

    //Change to previous camera in scene
    public void PreviousCamera()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] == Camera.main)
                cameraIndex = i;
        }

        cameraIndex--;

        if (cameraIndex < 0)
        {
            cameraIndex = cameras.Length - 1;
            cameras[0].enabled = false;
            cameras[cameraIndex].enabled = true;
        }
        else
        {
            cameras[cameraIndex + 1].enabled = false;
            cameras[cameraIndex].enabled = true;
        }
    }

    // visibility UI of replay
    public void UIvisibility(bool b)
    {
        replayBoxUI.SetActive(b);
    }
}
