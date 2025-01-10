using System;
using UnityEngine;
using VRMod.Patches;
using UnityEngine.SceneManagement;
using VRMod.Assets;
using Valve.VR;
using GameCoder.Engine;
using VRMod.UI;
using static VRMod.VRMod;
using DuoyiQaLib;
using UnityEngine.Rendering;
using System.Collections;
using UI;

namespace VRMod.Player
{

    /// <summary>
    /// Responsible for seting up all VR related classes and handling focus state changes.
    /// </summary>
    public class VRSystems : MonoBehaviour
    {
        public VRSystems(IntPtr value) : base(value) { }

        public static VRSystems Instance { get; private set; }
        public Canvas GuideCanvas;

        public void Awake()
        {
            if (Instance)
            {
                Log.Error("Trying to create duplicate VRSystems class!");
                enabled = false;
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            HarmonyPatches.onSceneLoaded += OnSceneLoaded;
            if (VRMod.IsVR)
            {
                SteamVR_Settings.instance.poseUpdateMode = SteamVR_UpdateModes.OnLateUpdate;
                SteamVR.Initialize(false);
            }

            //if (DYSceneManager.GetCurSceneName().ToLower().Contains("start"))
            //    MelonCoroutines.Start(this, HarmonyPatches.ClickStartScreenContinue());
        }

        public void Update()
        {
            //MenuFix.SetDebugUICamera();
            if (Input.GetKey(KeyCode.LeftControl)&&Input.GetKeyUp(KeyCode.H))
            {
                Camera[] cams = FindObjectsOfType<Camera>();
                foreach (Camera c in cams)
                {
                    c.stereoTargetEye = StereoTargetEyeMask.None;
                }
                VRPlayer.Instance.Camera.stereoTargetEye = StereoTargetEyeMask.Both;
            }
            if (GuideCanvas)
            {
                var isGuideAnim = GuideCanvas.transform.Find("OP_ani_UI").gameObject.active || GuideCanvas.transform.Find("ED_ani_UI").gameObject.active;
                if (isGuideAnim)
                {
                    TogglePlayerCam(true);
                    GuideCanvas.transform.position = CUIManager.instance.m_UIRoot.position;
                    GuideCanvas.transform.rotation = CUIManager.instance.m_UIRoot.rotation;
                    GuideCanvas.transform.localScale = CUIManager.instance.m_UIRoot.localScale;
                }
                else
                {
                    TogglePlayerCam(false);
                }
            }

        }

        public void SetGuideCanvas(Canvas canvas)
        {
            GuideCanvas = canvas;
            VRSystems.Instance.GuideCanvas.renderMode = RenderMode.WorldSpace;
        }

        private void CreateCameraRig()
        {
            if (!VRPlayer.Instance)
            {
                GameObject rig = Instantiate(VRAssets.VRCameraRig);
                rig.transform.parent = transform;
                rig.AddComponent<VRPlayer>();
            }
        }


        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log.Debug("VRSyatem.OnSceneLoaded: " + scene.name);
            if (scene.name == "home")
            {
                GuideCanvas = null;
                CreateCameraRig();
            }
        }

        private void TogglePlayerCam(bool toggle)
        {
            if (toggle)
            {
                VRPlayer.Instance.StereoRender.LeftCam.cullingMask = 0;
                VRPlayer.Instance.StereoRender.RightCam.cullingMask = 0;
            }
            else
            {
                VRPlayer.Instance.StereoRender.LeftCam.cullingMask = StereoRender.defaultCullingMask;
                VRPlayer.Instance.StereoRender.RightCam.cullingMask = StereoRender.defaultCullingMask;
            }
        }

        private void OnDestroy()
        {
        }
    }
}