using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableCamera : MonoBehaviour
{
    Cinemachine.CinemachineBrain brain;
    public ReplayManager replay;

    // Start is called before the first frame update
    void Start()
    {
        brain = GetComponent<Cinemachine.CinemachineBrain>();
    }

    // Update is called once per frame
    void Update()
    {
        if (replay.ReplayMode())
            brain.enabled = false;
        else
            brain.enabled = true;
    }
}
