using BepInEx.Configuration;
using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRMod.Settings
{
    public static class ModConfig
    {
        private const string CONFIG_FILE_NAME = "VRMod.cfg";

        private static readonly ConfigFile configFile = new ConfigFile(System.IO.Path.Combine(Paths.ConfigPath, CONFIG_FILE_NAME), true);

        public static ConfigEntry<bool> UseFirstPersonCam { get; private set; }
        public static ConfigEntry<bool> EnableTwoTwoHanded { get; private set; }
        public static ConfigEntry<bool> EnableAimRay { get; private set; }
        public static ConfigEntry<bool> EnablePostProcessing { get; private set; }
        public static ConfigEntry<float> UIDistance { get; private set; }
        public static ConfigEntry<float> UIScale { get; private set; }
        public static ConfigEntry<float> FPCamSmoothTime { get; private set; }
        public static ConfigEntry<float> TPCamSmoothTime { get; private set; }
        public static ConfigEntry<float> SmoothTurningSpeed { get; private set; }
        public static ConfigEntry<float> SnapTurningAngle { get; private set; }
        public static ConfigEntry<float> PlayerWorldScale { get; private set; }
        public static ConfigEntry<bool> EnableDebugMode { get; private set; }

        public static void Init()
        {
            EnableTwoTwoHanded = configFile.Bind<bool>(
                "VR Settings",
                "EnableTwoTwoHanded",
                true,
                "Whether to enable auto-snap to hold a weapon with two hands."
            );
            EnableAimRay = configFile.Bind<bool>(
                "VR Settings",
                "EnableAimRay",
                true,
                "Whether to enable the aim ray."
            );
            EnablePostProcessing = configFile.Bind<bool>(
                "VR Settings",
                "EnablePostProcessing",
                true,
                "Enable post-processing in HMD, disable this to gain some extra FPS."
            );
            PlayerWorldScale = configFile.Bind<float>(
                "VR Settings",
                "PlayerWorldScale",
                1.5f,
                "The larger this value is, the smaller the world looks to you."
            );
            UIDistance = configFile.Bind<float>(
                "VR Settings",
                "BattleHUDDistance",
                3.5f,
                "How far the HUD will be positioned to the front of the player."
            );
            UIScale = configFile.Bind<float>(
                "VR Settings",
                "BattleHUDScale",
                1f,
                "How big should the battle HUD be."
            );
            SmoothTurningSpeed = configFile.Bind<float>(
                "VR Settings",
                "SmoothTurningSpeed",
                1f,
                "Scaling factor for smoothing turning speed."
            );
            SnapTurningAngle = configFile.Bind<float>(
                "VR Settings",
                "SnapTurningAngle",
                45f,
                "Angles for each snap turning."
            );
            UseFirstPersonCam = configFile.Bind<bool>(
                "VR Settings",
                "UseFirstPersonCamera",
                false,
                "Enable an additional camera for showing smoothed first person view."
            );
            FPCamSmoothTime = configFile.Bind<float>(
                "VR Settings",
                "FirstPersonCameraSmoothFactor",
                0.05f,
                "Adjust the smooth strength applied to the first person camera."
            );
            TPCamSmoothTime = configFile.Bind<float>(
                "VR Settings",
                "ThirdPersonCameraSmoothFactor",
                0.15f,
                "Adjust the smooth strength applied to the third person view when using the squirrel's primary skill."
            );
            EnableDebugMode = configFile.Bind<bool>(
                "VR Settings",
                "EnableDebugMode",
                false,
                "Enable debug mode for adjusting weapon offsets."
            );
        }

        public static void Save()
        {
            configFile.Save();
        }
    }
}
