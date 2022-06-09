using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSpawn : MonoBehaviour
{
    public int numInstances = 20;
    public GameObject obj;
    public ReplayManager replayManager;
    public AudioSource sound;
    public ParticleSystem particle;

    public enum Function { SPAWN_MULTIPLE, TELEPORT }
    public Function function = Function.SPAWN_MULTIPLE;

    public Vector3 teleportDestination;

    private bool insideTrigger = false;
    private float timeInside = 0f;
    private GameObject triggerGO;

    private void Update()
    {
        if (replayManager.ReplayMode() == false)
        {
            if (insideTrigger)
                timeInside += Time.deltaTime;
            else
                timeInside = 0f;


            if (function == Function.TELEPORT && timeInside >= 2f)
            {
                Teleport(triggerGO);
                if (particle != null)
                    particle.Stop();
                
            }
                
        }
    }
    void SpawnMultiple()
    {
        for (int i = 0; i < numInstances; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-2, 2), Random.Range(-2, 2), Random.Range(-2, 2));
            GameObject go = Instantiate(obj);
            go.transform.position = gameObject.transform.position + offset;
        }
    }

    void Teleport(GameObject obj)
    {
        CharacterController cc = obj.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        obj.transform.position = teleportDestination;

        if (cc != null)
            cc.enabled = true;

        timeInside = 0f;
        insideTrigger = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(replayManager.ReplayMode() == false)
        {
            sound.Play();
            if (particle != null)
                particle.Play();

            insideTrigger = true;
            triggerGO = other.gameObject;            

            if (function == Function.SPAWN_MULTIPLE)
                SpawnMultiple();
                   
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (replayManager.ReplayMode() == false)
        {
            insideTrigger = false;
            timeInside = 0f;

            if (particle != null)
                particle.Stop();

            if (function == Function.TELEPORT)
                sound.Stop();
        }
            
    }
}
