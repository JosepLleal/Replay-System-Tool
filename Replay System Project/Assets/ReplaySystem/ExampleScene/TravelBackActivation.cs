using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelBackActivation : MonoBehaviour
{
    public ReplayManager replay;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            replay.StartTravelBack(5f);
        }



        if (Input.GetKeyDown(KeyCode.R))
        {
            replay.StartTravelBack();
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            replay.ExitTravelBack();
        }
    }
}
