using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplayManager : MonoBehaviour
{
    //Main system variables
    private List<Record> records = new List<Record>();
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


    void Awake()
    {
        //recomended FPS
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
                timeLine.value = frameIndex;

                if (frameIndex < recordMaxLength - 1 && frameIndex < records[0].GetLength() - 1)
                {
                    frameIndex++;

                    for (int i = 0; i < records.Count; i++)
                    {
                        SetTransforms(records[i], frameIndex);
                        Animator animator = records[i].GetAnimator();
                        if (animator != null)
                            animator.playbackTime += (animator.recorderStopTime - animator.recorderStartTime)/ (float)records[i].GetLength();

                    }

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

    //Slider event: has been clicked
    public void SliderClick()
    {
        usingSlider = true;
        
        if(state == ReplayState.PLAYING)
            PauseResume();
    }
    //Slider event: has been released
    public void SliderRelease()
    {
        //set frame to slider value
        frameIndex = (int)timeLine.value;

        for (int i = 0; i < records.Count; i++)
        {
            Animator animator = records[i].GetAnimator();
            //set animations replay time: time = startTime + frame * dT
            if (animator != null)
                animator.playbackTime = animator.recorderStartTime + ((animator.recorderStopTime - animator.recorderStartTime) / (float)records[i].GetLength()) * (float)frameIndex;

        }

        usingSlider = false;
        PauseResume();
    }

    //set transforms from the frame at record[index]
    void SetTransforms(Record rec, int index)
    {
        Frame f = rec.GetFrameAtIndex(index);
        if (f == null) return;
        
        GameObject go = rec.GetGameObject();

        go.transform.position = f.GetPosition();
        go.transform.rotation = f.GetRotation();
        go.transform.localScale = f.GetScale();
    }

    //Instantiate temporary camera for replay
    void InstantiateReplayCamera()
    {
        //TODO: instantiate camera prefab
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

    //------------- REPLAY TOOLS -------------------//

    // Start replay mode
    public void EnterReplayMode()
    {
        isReplayMode = true;

        //temporary replay camera instantiation
        InstantiateReplayCamera();
        frameIndex = 0;

        //slider max value
        timeLine.maxValue = records[0].GetLength();
        timeLine.value = frameIndex;

        //Enable UI
        UIvisibility(true);

        //set gameobjects transforms to starting frame
        for (int i = 0; i < records.Count; i++)
        {
            records[i].SetKinematic(true);

            SetTransforms(records[i], frameIndex);

            Animator animator = records[i].GetAnimator();
            if (animator != null)
            {
                //stop recording animator
                animator.StopRecording();

                //star animator replayMode
                animator.StartPlayback();
                animator.playbackTime = animator.recorderStartTime;
            }

        }
    }

    //Exit replay mode
    public void QuitReplayMode()
    {
        float time = records[0].GetLength() / Application.targetFrameRate;

        //set gameobjects transforms back to current state
        for (int i = 0; i < records.Count; i++)
        {
            SetTransforms(records[i], records[i].GetLength() - 1);
            records[i].SetKinematic(false);
            
            Animator animator = records[i].GetAnimator();
            if (animator != null)
            {
                animator.playbackTime = animator.recorderStopTime;
                animator.StopPlayback();
                records[i].SetStartRecording(false);
            }

            records[i].ClearFrameList();
        }
                
        state = ReplayState.PAUSE;
        DeleteReplayCam();
        //Disable UI
        UIvisibility(false);

        isReplayMode = false;
    }

    //Start replay from begining
    public void RestartReplay()
    {
        frameIndex = 0;

        for (int i = 0; i < records.Count; i++)
        {
            SetTransforms(records[i], frameIndex);

            Animator animator = records[i].GetAnimator();
            if (animator != null)
            {
                animator.playbackTime = animator.recorderStartTime;
            }
        }

    }

    //Pause / Resume function
    public void PauseResume()
    {
        if (state == ReplayState.PAUSE)
        {
            state = ReplayState.PLAYING;
            
        }            
        else
        {
            state = ReplayState.PAUSE;
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
                SetTransforms(records[i], frameIndex);

                Animator animator = records[i].GetAnimator();
                if (animator != null)
                    animator.playbackTime += (animator.recorderStopTime - animator.recorderStartTime) / (float)records[i].GetLength();
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
                SetTransforms(records[i], frameIndex);

                Animator animator = records[i].GetAnimator();
                if (animator != null)
                    animator.playbackTime -= (animator.recorderStopTime - animator.recorderStartTime) / (float)records[i].GetLength();
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
