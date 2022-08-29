using InControl;
using System;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Valve.VR;
using VRMod.Assets;
using VRMod.Core;
using VRMod.Patches;
using VRMod.Player.VRInput;
using VRMod.UI.Pointers;

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

            // 新增一个新手柄给游戏识别
            FindObjectOfType<InControl.InControlManager>()?.gameObject.GetOrAddComponent<VRInputManager>();

            Origin = transform.parent;
            Head = transform.Find("PlayerCamera");
            if (Head)
            {
                Camera = Head.gameObject.GetComponent<Camera>();
                Camera.cullingMask = 0;
                Camera.depth = -1;
                StereoRender = Head.gameObject.AddComponent<StereoRender>();

                LeftHand = transform.Find("LeftHand").gameObject.GetOrAddComponent<SteamVR_Behaviour_Pose>();
                RightHand = transform.Find("RightHand").gameObject.GetOrAddComponent<SteamVR_Behaviour_Pose>();

                LeftHand.poseAction = SteamVR_Actions.Gameplay.Pose;
                RightHand.poseAction = SteamVR_Actions.Gameplay.Pose;
                LeftHand.inputSource = SteamVR_Input_Sources.LeftHand;
                RightHand.inputSource = SteamVR_Input_Sources.RightHand;
                LeftHand.origin = Origin;
                RightHand.origin = Origin;

                var shader = Resources.FindObjectsOfTypeAll<Shader>().First(x=>x.name.Contains("M1/Character"));
                var renderers = gameObject.GetComponentsInChildren<Renderer>();
                foreach(var renderer in renderers)
                {
                    renderer.material.shader = shader;
                    renderer.material.SetFloat("_OutlineWidth", 0.005f);
                }
            }

            SetOriginHomePosition();
            DontDestroyOnLoad(Origin);
        }

        public void ToggleEventCamera()
        {
            //var eventSystem = GameObject.Find("DontDestroyRoot/EventSystem");
            var eventSystem = GameObject.Find("UniverseLibCanvas");
            //var eventSystem = GameObject.Find("EventSystem");

            var uIPointer = RightHand.transform.Find("CanvasPointer").gameObject.GetOrAddComponent<UIPointer>();
            uIPointer.eventSystem = eventSystem.GetComponent<EventSystem>();
            uIPointer.inputModule = eventSystem.GetComponent<StandaloneInputModule>();

            var eventCam = uIPointer.GetComponent<Camera>();

            var vrPointerInput = eventSystem.GetComponent<VRPointerInput>();
            if(vrPointerInput != null)
            {
                DestroyImmediate(vrPointerInput);
            }
            else
            {
                vrPointerInput = eventSystem.AddComponent<VRPointerInput>();
                vrPointerInput.eventCamera = eventCam;
                vrPointerInput.clikeButton = SteamVR_Actions.gameplay_RT_Fire_InteractUI;
            }
            var canvases = CUIManager.instance.gameObject.GetComponentsInChildren<Canvas>();
            foreach (var canvas in canvases)
            {
                canvas.worldCamera = eventCam;
            }
        }
        
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.J))
            {
                ToggleEventCamera();
            }
            if (Input.GetKeyUp(KeyCode.N))
            {
                Log.Info(" InputManager.activeDevice.RightStick.Vector : " + InputManager.activeDevice.RightStick.Vector);
            }
            
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if(scene.name == "home")
            {
                SetOriginHomePosition();
            }
            else if(RegexManager.IsNumber(scene.name))
            {
                Camera[] cams = FindObjectsOfType<Camera>();
                foreach (Camera c in cams)
                {
                    c.stereoTargetEye = StereoTargetEyeMask.None;
                }
                Camera.stereoTargetEye = StereoTargetEyeMask.Both;
                Origin.parent = GameObject.Find("FPSCam").transform.parent;
            }
        }

        private void OnDestroy()
        {
            HarmonyPatches.onSceneLoaded -= OnSceneLoaded;
        }

        private void SetOriginHomePosition()
        {
            Origin.position = new Vector3(28f, 1.6f, 16f);
            Origin.localEulerAngles = new Vector3(0, 90, 0);
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
