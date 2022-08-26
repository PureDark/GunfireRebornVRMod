using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using VRMod.Assets;
using VRMod.Core;
using VRMod.Patches;

namespace VRMod.Player
{
    public class VRPlayer : MonoBehaviour
    {
        public VRPlayer(IntPtr value) : base(value) { }


        public static VRPlayer Instance { get; private set; }
        public Transform Origin { get; private set; }
        public Transform Head { get; private set; }
        public Camera Camera { get; private set; }
        public SteamVR_Behaviour_Pose LeftHand { get; private set; }
        public SteamVR_Behaviour_Pose RightHand { get; private set; }
        public StereoRender StereoRender { get; private set; }

        public VRInput Input { get; set; }

        private void Awake()
        {
            if (Instance)
            {
                Log.Error("Trying to create duplicate VRPlayer!");
                enabled = false;
                return;
            }
            Instance = this;

            SteamVR_Settings.instance.poseUpdateMode = SteamVR_UpdateModes.OnLateUpdate;
            SteamVR.Initialize(false);

            HarmonyPatches.onSceneLoaded += OnSceneLoaded;

            var CamPoint = GameObject.Find("CamPoint_Camera");
            if (CamPoint)
            {
                CamPoint.GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;
                //CamPoint.GetComponent<Camera>().enabled = false;
            }


            Origin = transform.parent;
            Head = transform.Find("PlayerCamera");
            if (Head)
            {
                Camera = Head.gameObject.GetComponent<Camera>();
                Camera.cullingMask = 0;
                Camera.depth = -1;
                StereoRender = Head.gameObject.AddComponent<StereoRender>();
                //Head.gameObject.AddComponent<OutLinePassMgr>();

                Input = new VRInput();

                LeftHand = transform.Find("LeftHand").gameObject.GetOrAddComponent<SteamVR_Behaviour_Pose>();
                RightHand = transform.Find("RightHand").gameObject.GetOrAddComponent<SteamVR_Behaviour_Pose>();

                LeftHand.poseAction = SteamVR_Actions.Gameplay.Pose;
                RightHand.poseAction = SteamVR_Actions.Gameplay.Pose;
                LeftHand.inputSource = SteamVR_Input_Sources.LeftHand;
                RightHand.inputSource = SteamVR_Input_Sources.RightHand;
                LeftHand.origin = Origin;
                RightHand.origin = Origin;
            }

            SetOriginHomePosition();
            DontDestroyOnLoad(Origin);
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if(scene.name == "home")
            {
                SetOriginHomePosition();
            }
        }

        private void OnDestroy()
        {
            HarmonyPatches.onSceneLoaded -= OnSceneLoaded;
        }

        private void SetOriginHomePosition()
        {
            Origin.position = new Vector3(28f, 1.6f, 21.55f);
        }

        public Vector3 GetWorldForward()
        {
            return Head.forward;
        }

        public Vector3 GetFlatForwardDirection()
        {
            Vector3 dir = Head.forward;
            dir.y = 0;
            return dir.normalized;
        }

        public float GetPlayerHeight()
        {
            if (!Head)
            {
                return 1.8f;
            }
            return Head.localPosition.y;
        }
    }
}
