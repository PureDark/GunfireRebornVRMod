using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;
using VRMod.Patches;
using VRMod.Player;
using VRMod.Assets;
using VRMod.UI.Pointers;
using VRMod.Player.VRInput;
using VRMod.Player.MotionControlls;
using BepInEx.Logging;
using VRMod.UI;
using VRMod.Player.Behaviours;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Valve.VR;
using VRMod.Settings;
using Il2CppInterop.Runtime.Injection;

namespace VRMod
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class VRMod : BasePlugin
    {
        public const string GUID = "PureDark.GRVRMod";
        public const string NAME = "VRMod";
        public const string AUTHOR = "PureDark/暗暗十分/突破天际的金闪闪 | xPrinny";
        public const string VERSION = "1.0.8.0";

        public static VRMod Instance { get; private set; }

        public static bool IsVR = true;

        internal static Harmony Harmony { get; } = new Harmony(GUID);

        internal static bool vrEnabled;

        public override void Load()
        {
            Instance = this;
            Log.Setup(base.Log);

            if (SteamVRRunningCheck())
            {
                InitVR();
            }
            else
            {
                Log.Warning("VR launch aborted, VR is disabled or SteamVR is off!");
            }
        }

        private void InitVR()
        {
            vrEnabled = true;
            HarmonyPatches.PatchAll();
            VRAssets.Init();
            ModConfig.Init();
            SetupIL2CPPClassInjections();
            SceneManager.sceneLoaded += new Action<Scene, LoadSceneMode>(OnSceneLoaded);
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!VRSystems.Instance)
            {
                //VR相关的入口，在CUIManager初始化时进行注入
                new GameObject("VR_Globals").AddComponent<VRSystems>();
                MenuFix.Prefix();
            }
        }

        private void SetupIL2CPPClassInjections()
        {
            ClassInjector.RegisterTypeInIl2Cpp<VRSystems>();
            ClassInjector.RegisterTypeInIl2Cpp<VRPlayer>();
            ClassInjector.RegisterTypeInIl2Cpp<StereoRender>();
            ClassInjector.RegisterTypeInIl2Cpp<VRInputManager>();
            ClassInjector.RegisterTypeInIl2Cpp<VRInputDevice>();
            ClassInjector.RegisterTypeInIl2Cpp<VRPointerInput>();
            ClassInjector.RegisterTypeInIl2Cpp<VRBattleUI>();
            ClassInjector.RegisterTypeInIl2Cpp<HandController>();
            ClassInjector.RegisterTypeInIl2Cpp<VRScope>();
            ClassInjector.RegisterTypeInIl2Cpp<CameraSmoother>();
        }

        private bool SteamVRRunningCheck()
        {
            List<Process> possibleVRProcesses = new List<Process>();

            possibleVRProcesses.AddRange(Process.GetProcessesByName("vrserver"));
            possibleVRProcesses.AddRange(Process.GetProcessesByName("vrcompositor"));

            Log.Debug("VR processes found - " + possibleVRProcesses.Count);
            foreach (Process p in possibleVRProcesses)
            {
                Log.Debug(p.ToString());
            }
            return possibleVRProcesses.Count > 0;
        }

        public static new class Log
        {
            static ManualLogSource log;

            public static void Setup(ManualLogSource log)
            {
                Log.log = log;
            }

            public static void Warning(string msg)
            {
                log.LogWarning(msg);
            }

            public static void Error(string msg)
            {
                log.LogError(msg);
            }

            public static void Info(string msg)
            {
                log.LogInfo(msg);
            }

            public static void Debug(string msg)
            {
                log.LogDebug(msg);
            }
        }
    }
}