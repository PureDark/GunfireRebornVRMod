using System;
using UnityEngine;
using VRMod.Patches;
using UnityEngine.SceneManagement;
using VRMod.Core;
using VRMod.Assets;
using Valve.VR;

namespace VRMod.Player
{

    /// <summary>
    /// Responsible for seting up all VR related classes and handling focus state changes.
    /// </summary>
    public class VRSystems : MonoBehaviour
    {
        public VRSystems(IntPtr value) : base(value) { }

        public static VRSystems Instance { get; private set; }

        private void Awake()
        {
            if (Instance)
            {
                Log.Error("Trying to create duplicate VRSystems class!");
                enabled = false;
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            //gameObject.AddComponent<ShaderFixer>();

            HarmonyPatches.onSceneLoaded += OnSceneLoaded;

            //if (DYSceneManager.GetCurSceneName().ToLower().Contains("start"))
            //    MonoBehaviourExtensions.StartCoroutine(this, HarmonyPatches.ClickStartScreenContinue());
        }
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.J))
            {
                Log.Info("Screen: " + Screen.width + "x" + Screen.height);
            }
            if (Input.GetKey(KeyCode.LeftControl)&&Input.GetKeyUp(KeyCode.H))
            {
                Camera[] cams = FindObjectsOfType<Camera>();
                foreach (Camera c in cams)
                {
                    c.stereoTargetEye = StereoTargetEyeMask.None;
                }
            }

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
            if(scene.name == "home")
            {
                CreateCameraRig();
                HarmonyPatches.onSceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnDestroy()
        {
        }
    }
}