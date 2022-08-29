using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class VRInput : BaseInput
{
    public Camera eventCamera = null;
    public SteamVR_Action_Single clikeButton = SteamVR_Actions.gameplay_RT_Fire_InteractUI;

    protected override void Awake()
    {
        GetComponent<BaseInputModule>().inputOverride = this;
    }

    public override bool GetMouseButton(int button)
    {
        return clikeButton.axis > 0.8f;
    }

    public override bool GetMouseButtonDown(int button)
    {
        return clikeButton.lastAxis <= 0.8f && clikeButton.axis > 0.8f;
    }

    public override bool GetMouseButtonUp(int button)
    {
        return clikeButton.lastAxis > 0.8f && clikeButton.axis <= 0.8f;
    }

    public override Vector2 mousePosition
    {
        get
        {
            return new Vector2(eventCamera.pixelWidth / 2, eventCamera.pixelHeight / 2);
        }
    }
}
