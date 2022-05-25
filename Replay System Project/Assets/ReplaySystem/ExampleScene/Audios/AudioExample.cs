using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioExample : MonoBehaviour
{

    public AudioSource source;
    public ParticleSystem particle;
    public ReplayManager replayManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) && replayManager.ReplayMode() == false)
        {
            source.Play();
            particle.Play();
        }

        if (Input.GetKeyDown(KeyCode.F2) && replayManager.ReplayMode() == false)
        {
            source.Play();
            particle.Stop();
        }
    }
}
