using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Test : MonoBehaviour
{
    private SteamVR_Behaviour_Pose pose;

    // Start is called before the first frame update
    void Start()
    {
        pose = GetComponent<SteamVR_Behaviour_Pose>();
    }

    // Update is called once per frame
    void Update()
    {

        Debug.Log("pose.isValid = " + pose?.isValid);
        Debug.Log("pose.isActive = " + pose?.isActive);
    }
}
