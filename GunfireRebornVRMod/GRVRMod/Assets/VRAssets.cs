using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRMod.Assets
{
    public static class VRAssets
    {
        public static AssetBundle VRAssetBundle = null;
        public static Shader StereoRender;
        public static GameObject VRCameraRig;

        public static void Init()
        {
            VRAssetBundle = ResourceLoader.LoadAssetBundle("vrassets");

            StereoRender = ResourceLoader.LoadAsset<Shader>("PureDark/StereoRender");
            VRCameraRig = ResourceLoader.LoadAsset<GameObject>("[VRCameraRig]");

        }
    }
}
