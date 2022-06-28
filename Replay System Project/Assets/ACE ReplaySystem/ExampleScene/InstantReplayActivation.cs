using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantReplayActivation : MonoBehaviour
{
    public ReplayManager replay;

    // Update is called once per frame
    void Update()
    {
        //Enter replay mode
        if (Input.GetKeyDown(KeyCode.R) && !replay.ReplayMode())
            replay.EnterReplayMode();

    }
}
