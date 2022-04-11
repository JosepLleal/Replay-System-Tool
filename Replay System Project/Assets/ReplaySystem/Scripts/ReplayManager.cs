using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{

    private List<Record> records = new List<Record>();
    
    private bool isReplayMode = false;
    private int recordMaxLength = 600;

    private int recordLength;
    private int frameIndex = 0;

    private float[] speeds = { 0.25f, 0.5f, 1.0f, 2.0f, 4.0f};
    private int speedIndex = 2;

    public enum ReplayState { PAUSE, PLAYING, PLAY_REVERSE}
    public ReplayState state = ReplayState.PAUSE;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        recordLength = records[0].GetLength();

        if(Input.GetKeyDown(KeyCode.R))
        {
            isReplayMode = !isReplayMode;

            if(isReplayMode)
            {
                //set gameobjects transforms to starting frame
                foreach(Record r in records)
                {
                    SetTransforms(r, 0);
                }
            }
            else
            {
                //set gameobjects transforms back to current state
                foreach (Record r in records)
                {
                    SetTransforms(r, r.GetLength());
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (state == ReplayState.PAUSE)
                state = ReplayState.PLAYING;
            else
                state = ReplayState.PAUSE;
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
                }

                if (frameIndex < recordLength - 1)
                    frameIndex++;
                
            }

        }
    }

    void SetTransforms(Record rec, int index)
    {
        Frame f = rec.GetFrameAtIndex(index);
        GameObject go = f.GetGO();

        go.transform.position = f.GetPosition();
        go.transform.rotation = f.GetRotation();
        go.transform.localScale = f.GetScale();
    }

    public void AddRecord(Record r)
    {
        records.Add(r);
    }

    public  int  GetMaxLength() 
    {
        return recordMaxLength;
    }

    public bool ReplayMode()
    {
        return isReplayMode;
    }

    //---- REPLAY TOOLS ---------

    void RestartReplay()
    {
        frameIndex = 0;

        if (state == ReplayState.PAUSE)
            state = ReplayState.PLAYING;
    }

    void SpeedUp()
    {
        if(speedIndex < speeds.Length - 1)
            speedIndex++;

        Time.timeScale = speeds[speedIndex];
    }

    void SpeedDown()
    {
        if (speedIndex > 0)
            speedIndex--;

        Time.timeScale = speeds[speedIndex];
    }

    

    
}
