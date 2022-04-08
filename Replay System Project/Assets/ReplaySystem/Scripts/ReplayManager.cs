using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{

    private List<Record> records = new List<Record>();
    
    private bool isReplayMode = false;
    private int recordMaxLength = 600;
    private int frameIndex = 0;

    public enum ReplayState { PAUSE, PLAYING, PLAY_REVERSE}
    public ReplayState state = ReplayState.PLAYING;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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

                if (frameIndex < recordMaxLength)
                    frameIndex++;
                else
                {
                    frameIndex = 0;
                    state = ReplayState.PAUSE;
                }
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

    public int GetMaxLength() 
    { 
        return recordMaxLength; 
    }

    public bool ReplayMode()
    {
        return isReplayMode;
    }

    
}
