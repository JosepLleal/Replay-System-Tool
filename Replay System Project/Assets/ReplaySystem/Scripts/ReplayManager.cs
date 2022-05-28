using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplayManager : MonoBehaviour
{
    //Main system variables
    public List<Record> records = new List<Record>();
    private bool isReplayMode = false;
    private int recordMaxLength = 3600; // 60fps * 60seconds = 3600 frames 
    private int frameIndex = 0;

    //States
    public enum ReplayState { PAUSE, PLAYING, PLAY_REVERSE }
    public ReplayState state = ReplayState.PAUSE;

    //replay speeds
    private float[] speeds = { 0.25f, 0.5f, 1.0f, 2.0f, 4.0f };
    private int speedIndex = 2;

    //UI elements
    private bool usingSlider = false;
    public Slider timeLine;
    public GameObject replayUI;

    //Replay cameras
    private GameObject replayCam;
    private Camera[] cameras;
    private int cameraIndex = 0;    

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
                timeLine.value = frameIndex;

                if (frameIndex < recordMaxLength - 1 && frameIndex < records[0].GetLength() - 1)
                {
                    for (int i = 0; i < records.Count; i++)
                    {
                        //Check for instantiated GO
                        HandleInstantiatedObjects(records[i], frameIndex - records[i].GetFirstFrameIndex());
                        int auxIndex = frameIndex - records[i].GetFirstFrameIndex();

                        if (auxIndex >= 0)
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
                    frameIndex++;
                }
                else
                    PauseResume();
            }
        }
    }

    //This function is responsible for activating and deactivating instantiated GO, dependenig on the current time of the replay 
    void HandleInstantiatedObjects(Record rec, int index)
    {
        GameObject go = rec.GetGameObject();

        //it has not been instantiated yet
        if (index < 0)
        {
            go.SetActive(false);
        }
        else
        {
            if (go.activeInHierarchy == false)
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
            //Check for instantiated GO
            HandleInstantiatedObjects(records[i], frameIndex - records[i].GetFirstFrameIndex());
            int auxIndex = frameIndex - records[i].GetFirstFrameIndex();

            if (auxIndex >= 0)
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
        timeLine.maxValue = records[0].GetLength();
        timeLine.value = frameIndex;

        //Enable UI
        UIvisibility(true);

        state = ReplayState.PAUSE;
        Time.timeScale = 0f;

        //set gameobjects states to starting frame
        for (int i = 0; i < records.Count; i++)
        {
            records[i].SetKinematic(true);
            
            SetTransforms(records[i], frameIndex);

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
            if(part != null)
            {
                if (part.isPlaying)
                {
                    part.Stop();
                    part.Clear();
                }

                if (records[i].GetFrameAtIndex(frameIndex).ParticleTime() != 0f)
                {
                    part.Simulate(records[i].GetFrameAtIndex(frameIndex).ParticleTime());
                    part.Play();
                }
            }

            //Check for instantiated GO
            HandleInstantiatedObjects(records[i], frameIndex - records[i].GetFirstFrameIndex());
        }
    }

    //Exit replay mode
    public void QuitReplayMode()
    {
        //set gameobjects transforms back to current state
        for (int i = 0; i < records.Count; i++)
        {
            //Check for instantiated GO
            HandleInstantiatedObjects(records[i], records[i].GetLength() - 1 - records[i].GetFirstFrameIndex());

            SetTransforms(records[i], records[i].GetLength() - 1);
            records[i].SetKinematic(false);
            
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
                
        state = ReplayState.PAUSE;
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
            //Check for instantiated GO
            HandleInstantiatedObjects(records[i], frameIndex - records[i].GetFirstFrameIndex());

            SetTransforms(records[i], frameIndex);

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

                if (records[i].GetFrameAtIndex(frameIndex).ParticleTime() != 0f)
                {
                    part.Simulate(records[i].GetFrameAtIndex(frameIndex).ParticleTime());
                    part.Play();
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
        
        if (frameIndex < recordMaxLength - 1) 
        {
            frameIndex++;

            for (int i = 0; i < records.Count; i++)
            {
                //Check for instantiated GO
                HandleInstantiatedObjects(records[i], frameIndex - records[i].GetFirstFrameIndex());
                int auxIndex = frameIndex - records[i].GetFirstFrameIndex();

                if(auxIndex >= 0)
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
        
        if (frameIndex > 0)
        {
            frameIndex--;

            for (int i = 0; i < records.Count; i++)
            {
                //Check for instantiated GO
                HandleInstantiatedObjects(records[i], frameIndex - records[i].GetFirstFrameIndex());
                int auxIndex = frameIndex - records[i].GetFirstFrameIndex();

                if(auxIndex >= 0)
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

    public void UIvisibility(bool b)
    {
        replayUI.SetActive(b);
    }
}
