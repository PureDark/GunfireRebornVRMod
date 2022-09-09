using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRMod.Assets
{
    public static class VRAssets
    {
        public static Shader StereoRender;
        public static GameObject VRCameraRig;
        public static GameObject CircleScope;
        public static GameObject CurvedUILaserPointer;

        public static void Init()
        {
            ResourceLoader.LoadAssetBundle("vrassets");

            StereoRender = ResourceLoader.LoadAsset<Shader>("PureDark/StereoRender");
            VRCameraRig = ResourceLoader.LoadAsset<GameObject>("[VRCameraRig]");
            CircleScope = ResourceLoader.LoadAsset<GameObject>("CircleScope");
            CurvedUILaserPointer = ResourceLoader.LoadAsset<GameObject>("CurvedUILaserPointer");
        }
    }
}
