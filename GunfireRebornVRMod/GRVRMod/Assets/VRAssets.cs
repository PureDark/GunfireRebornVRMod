using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRMod.Assets
{
    public static class VRAssets
    {
        public static Shader StereoRender;
        public static Shader LaserUnlit;
        public static GameObject VRCameraRig;
        public static GameObject CircleScope;

        public static void Init()
        {
            ResourceLoader.LoadAssetBundle("vrassets");

            StereoRender = ResourceLoader.LoadAsset<Shader>("PureDark/StereoRender");
            LaserUnlit = ResourceLoader.LoadAsset<Shader>("PureDark/LaserUnlit");
            VRCameraRig = ResourceLoader.LoadAsset<GameObject>("[VRCameraRig]");
            CircleScope = ResourceLoader.LoadAsset<GameObject>("CircleScope");
        }
    }
}
