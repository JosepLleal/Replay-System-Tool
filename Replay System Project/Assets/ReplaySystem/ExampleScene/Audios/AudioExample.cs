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

    double timer = 0;

    // Update is called once per frame
    void Update()
    {
        if(replayManager.ReplayMode() == false)
        {
            timer += Time.deltaTime;

            if (timer >= 10.0f)
            {
                source.Play();
                particle.Play();

                if (sphere != null)
                    replayManager.DestroyRecordedGO(sphere);

                sphere = Instantiate(ballPrefab);
                sphere.transform.position = gameObject.transform.position;
                timer = 0;
            }
        }
    }
}
