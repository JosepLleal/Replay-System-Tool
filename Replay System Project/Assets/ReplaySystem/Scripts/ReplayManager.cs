using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{

    private List<Record> records = new List<Record>();
    
    private bool isReplayMode = false;
    private int recordMaxLength = 600;
    private int frameIndex = 0;

    private float[] speeds = { 0.25f, 0.5f, 1.0f, 2.0f, 4.0f};
    private int speedIndex = 2;

    private GameObject replayCam;
    private Camera[] cameras;
    private int cameraIndex = 0;

    public enum ReplayState { PAUSE, PLAYING, PLAY_REVERSE}
    public ReplayState state = ReplayState.PAUSE;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

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

        // ------ TESTING INPUTS ------------
        if(isReplayMode)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                PauseResume();
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                RestartReplay();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                SpeedDown();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                SpeedUp();
            }

            

            if (Input.GetKeyDown(KeyCode.X))
            {
                NextCamera();
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                PreviousCamera();
            }
        }        
        //----------------------------------------
    }

    private void FixedUpdate()
    {
        if (isReplayMode)
        {
            if (state == ReplayState.PLAYING)
            {
                foreach (Record r in records)
                {
                    SetTransforms(r, frameIndex);
                    SetAnimationParameters(r, frameIndex);
                }

                if (frameIndex < recordMaxLength - 1)
                    frameIndex++;

            }

            // Testing purposes -------------------
            if (Input.GetKey(KeyCode.F3))
            {
                GoForward();
            }

            if (Input.GetKey(KeyCode.F2))
            {
                GoBack();
            }

        }
    }

    

    //Add record to records list
    public void AddRecord(Record r)
    {
        records.Add(r);
    }

    //Get max length of recordable frames
    public  int  GetMaxLength() 
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

    //set animator parameters values from the frame at record[index]
    void SetAnimationParameters(Record rec, int index)
    {
        Frame f = rec.GetFrameAtIndex(index);
        if (f == null) return;

        Animator animator = rec.GetAnimator();

        foreach(AnimationRecord anim in f.GetAnimationRecords())
        {
            if(anim.GetAnimatorType() == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(anim.GetName(), anim.GetBoolValue());
            }
            else if (anim.GetAnimatorType() == AnimatorControllerParameterType.Float)
            {
                animator.SetFloat(anim.GetName(), anim.GetFloatValue());
            }
            else if(anim.GetAnimatorType() == AnimatorControllerParameterType.Int)
            {
                animator.SetInteger(anim.GetName(), anim.GetIntValue());
            }
        }
    }

    //Instantiate temporary camera for replay
    void InstantiateReplayCamera()
    {
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
    void EnterReplayMode()
    {
        isReplayMode = true;

        //temporary replay camera instantiation
        InstantiateReplayCamera();

        //set gameobjects transforms to starting frame
        foreach (Record r in records)
        {
            SetTransforms(r, 0);
        }
    }

    //Exit replay mode
    void QuitReplayMode()
    {
        isReplayMode = false;

        DeleteReplayCam();

        //set gameobjects transforms back to current state
        foreach (Record r in records)
        {
            SetTransforms(r, r.GetLength() - 1);
        }
    }

    //Start replay from begining
    void RestartReplay()
    {
        frameIndex = 0;

        foreach (Record r in records)
        {
            SetTransforms(r, frameIndex);
        }

    }

    //Pause / Resume function
    void PauseResume()
    {
        if (state == ReplayState.PAUSE)
        {
            state = ReplayState.PLAYING;
            Time.timeScale = speeds[speedIndex];
        }            
        else
        {
            state = ReplayState.PAUSE;
            Time.timeScale = 0;
        }
            
    }

    //Advances one frame 
    void GoForward()
    {
        state = ReplayState.PAUSE;

        foreach (Record r in records)
        {
            SetTransforms(r, frameIndex);
        }

        if (frameIndex < recordMaxLength - 1)
            frameIndex++;
    }

    //Back one frame
    void GoBack()
    {
        state = ReplayState.PAUSE;

        foreach (Record r in records)
        {
            SetTransforms(r, frameIndex);
        }

        if (frameIndex > 0)
            frameIndex--;
    }

    //Increase replay speed
    void SpeedUp()
    {
        if(speedIndex < speeds.Length - 1)
            speedIndex++;

        Time.timeScale = speeds[speedIndex];
    }

    //Decrease replay speed
    void SpeedDown()
    {
        if (speedIndex > 0)
            speedIndex--;

        Time.timeScale = speeds[speedIndex];
    }

    //Change to next camera in scene
    void NextCamera()
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
    void PreviousCamera()
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
