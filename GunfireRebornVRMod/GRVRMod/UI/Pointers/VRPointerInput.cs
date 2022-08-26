using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

namespace VRMod.UI.Pointers
{
    public class VRPointerInput : BaseInput
    {
        public Camera eventCamera = null;
        public SteamVR_Action_Boolean clikeButton = SteamVR_Actions.uI_InteractUI;

        public override void Awake()
        {
            GetComponent<BaseInputModule>().inputOverride = this;
        }

        public override bool GetMouseButton(int button)
        {
            return clikeButton.state;
        }

        public override bool GetMouseButtonDown(int button)
        {
            return clikeButton.stateDown;
        }

        public override bool GetMouseButtonUp(int button)
        {
            return clikeButton.stateUp;
        }

        public override Vector2 mousePosition
        {
            get
            {
                return new Vector2(eventCamera.pixelWidth / 2, eventCamera.pixelHeight / 2);
            }
        }
    }

}