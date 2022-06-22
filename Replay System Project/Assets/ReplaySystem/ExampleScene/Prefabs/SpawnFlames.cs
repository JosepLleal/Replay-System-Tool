using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnFlames : MonoBehaviour
{
    bool triggered = false;
    float timer = 0;
    int mult = 1;
    public int distance = 5;

    public int numberOfInstantiations = 10;

    public ReplayManager replayManager;
    public GameObject prefab;

    // Update is called once per frame
    void Update()
    {
        if(triggered)
        {
            if (timer > 0.5 && mult < numberOfInstantiations)
            {
                mult ++;
                Vector3 position = gameObject.transform.position + (Vector3.forward * mult * distance);
                Instantiate(prefab, position, gameObject.transform.rotation);
                timer = 0;
            }

            timer += Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (replayManager.ReplayMode() == false && triggered == false)
        {
            triggered = true;
            Instantiate(prefab, gameObject.transform.position + Vector3.forward * distance, gameObject.transform.rotation);
        }
    }

}
