using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;
using VRMod.Patches;
using VRMod.Player;
using VRMod.Assets;
using VRMod.Core;
using UnhollowerRuntimeLib;
using VRMod.UI.Pointers;
using VRMod.Player.VRInput;

namespace VRMod
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class VRMod : BasePlugin
    {
        public const string GUID = "PureDark.GRVRMod";
        public const string NAME = "VRMod";
        public const string AUTHOR = "PureDark/暗暗十分/突破天际的金闪闪";
        public const string VERSION = "1.0.0";
        public const bool DEBUG = true;

        public static VRMod Instance { get; private set; }

        internal static Harmony Harmony { get; } = new Harmony(GUID);

        internal static bool vrEnabled;

        public override void Load()
        {
            Instance = this;
            Core.Log.Setup(Log);

            if (SteamVRRunningCheck())
            {
                InitVR();
            }
            else
            {
                Core.Log.Warning("VR launch aborted, VR is disabled or SteamVR is off!");
            }
        }

        private void InitVR()
        {
            vrEnabled = true;
            VRAssets.Init();
            HarmonyPatches.PatchAll();
            SetupIL2CPPClassInjections();
        }

        private void SetupIL2CPPClassInjections()
        {
            ClassInjector.RegisterTypeInIl2Cpp<VRSystems>();
            ClassInjector.RegisterTypeInIl2Cpp<VRPlayer>();
            ClassInjector.RegisterTypeInIl2Cpp<ShaderFixer>();
            ClassInjector.RegisterTypeInIl2Cpp<StereoRender>();
            ClassInjector.RegisterTypeInIl2Cpp<VRInputManager>();
            ClassInjector.RegisterTypeInIl2Cpp<VRInputDevice>();
            ClassInjector.RegisterTypeInIl2Cpp<UIPointer>();
            ClassInjector.RegisterTypeInIl2Cpp<VRPointerInput>();
        }

        private bool SteamVRRunningCheck()
        {
            List<Process> possibleVRProcesses = new List<Process>();

            possibleVRProcesses.AddRange(Process.GetProcessesByName("vrserver"));
            possibleVRProcesses.AddRange(Process.GetProcessesByName("vrcompositor"));

            Core.Log.Debug("VR processes found - " + possibleVRProcesses.Count);
            foreach (Process p in possibleVRProcesses)
            {
                Core.Log.Debug(p.ToString());
            }
            return possibleVRProcesses.Count > 0;
        }
    }
}