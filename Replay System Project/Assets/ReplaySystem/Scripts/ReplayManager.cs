using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{

    private List<Record> records = new List<Record>();
    
    private bool isReplayMode = false;
    private int recordMaxLength = 3600; // 60fps * 60seconds = 3600 frames 

    private int frameIndex = 0;
    private float replayTime = 0;

    private float[] speeds = { 0.25f, 0.5f, 1.0f, 2.0f, 4.0f};
    private int speedIndex = 2;

    private GameObject replayCam;
    private Camera[] cameras;
    private int cameraIndex = 0;

    public enum ReplayState { PAUSE, PLAYING, PLAY_REVERSE}
    public ReplayState state = ReplayState.PAUSE;
    
    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
           

            if(isReplayMode)
            {
                QuitReplayMode();
            }
            else
            {
                EnterReplayMode();
            }
        }

        
        if(isReplayMode)
        {
                        
            // Replay playing 
            if (state == ReplayState.PLAYING)
            {
                if (frameIndex < recordMaxLength - 1 && frameIndex < records[0].GetLength() - 1)
                {
                    replayTime += Time.deltaTime;

                    foreach (Record r in records)
                    {
                        SetTransforms(r, frameIndex);
                        Animator animator = r.GetAnimator();
                        if (animator != null)
                            animator.playbackTime = replayTime;

                    }
                    frameIndex++;
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
        replayTime = 0;

        //set gameobjects transforms to starting frame
        foreach (Record r in records)
        {
            SetTransforms(r, 0);

            Animator animator = r.GetAnimator();
            if (animator != null)
            {
                //stop recording animator
                animator.StopRecording();
                r.startedRecording = false;

                //star animator replayMode
                animator.StartPlayback();
                animator.playbackTime = 0;
            }
                
        }
    }

    //Exit replay mode
    public void QuitReplayMode()
    {
        isReplayMode = false;

        DeleteReplayCam();

        //set gameobjects transforms back to current state
        foreach (Record r in records)
        {
            SetTransforms(r, r.GetLength() - 1);

            Animator animator = r.GetAnimator();
            if (animator != null)
                animator.StopPlayback();
        }
    }

    //Start replay from begining
    public void RestartReplay()
    {
        frameIndex = 0;
        replayTime = 0;

        foreach (Record r in records)
        {
            SetTransforms(r, frameIndex);
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
        replayTime += 1 / 60f;

        foreach (Record r in records)
        {
            SetTransforms(r, frameIndex);

            Animator animator = r.GetAnimator();
            if (animator != null)
                animator.playbackTime = replayTime;
        }

        if (frameIndex < recordMaxLength - 1)
            frameIndex++;
    }

    //Back one frame
    public void GoBack()
    {
        state = ReplayState.PAUSE;
        replayTime -= 1 / 60f;

        foreach (Record r in records)
        {
            SetTransforms(r, frameIndex);

            Animator animator = r.GetAnimator();
            if (animator != null)
                animator.playbackTime = replayTime;
        }

        if (frameIndex > 0)
            frameIndex--;
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

}
