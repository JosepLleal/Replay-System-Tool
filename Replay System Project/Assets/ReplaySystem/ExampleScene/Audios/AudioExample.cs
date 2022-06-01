using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioExample : MonoBehaviour
{

    public AudioSource source;
    public ParticleSystem particle;
    public ReplayManager replayManager;

    public GameObject ballPrefab;

    GameObject sphere;

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

            sphere = Instantiate(ballPrefab);
            sphere.transform.position = gameObject.transform.position;
            sphere.GetComponentInChildren<Record>().replay = replayManager;
        }

        if (Input.GetKeyDown(KeyCode.F2) && replayManager.ReplayMode() == false)
        {
            source.Play();
            particle.Stop();
        }

        if (Input.GetKeyDown(KeyCode.F3) && replayManager.ReplayMode() == false && sphere != null)
        {
            replayManager.DestroyRecordedGO(sphere);
        }
    }
}
