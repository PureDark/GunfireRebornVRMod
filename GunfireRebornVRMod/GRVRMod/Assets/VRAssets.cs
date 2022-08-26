using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VRMod.Core;

namespace VRMod.Assets
{
    public static class VRAssets
    {
        public static AssetBundle VRAssetBundle = null;
        //public static Shader Standard;
        //public static Shader StandardSpecular;
        //public static Shader VertexLit;
        //public static Shader VertexLitTransparent;
        //public static Shader VertexLitTransparentCutout;
        //public static Shader ParticleAdditive;
        //public static Shader ParticleAlphaBlend;
        //public static Shader UIParticleAdditive;
        //public static Shader UIParticleAlphaBlend;
        //public static Shader ToonShader;
        //public static Shader UVAnimInChannel_AlphaBlend;
        public static Shader StereoRender;
        public static GameObject VRCameraRig;

        public static void Init()
        {
            VRAssetBundle = ResourceLoader.LoadAssetBundle("vrassets");
            //VertexLit = Shader.Find("Legacy Shaders/VertexLit");
            //Standard = ResourceLoader.LoadAsset<Shader>("Standard");
            //StandardSpecular = ResourceLoader.LoadAsset<Shader>("Standard (Specular setup)");
            //VertexLitTransparent = ResourceLoader.LoadAsset<Shader>("Legacy Shaders/Transparent/VertexLit");
            //VertexLitTransparentCutout = ResourceLoader.LoadAsset<Shader>("Legacy Shaders/Transparent/Cutout/VertexLit");
            //ParticleAdditive = ResourceLoader.LoadAsset<Shader>("Legacy Shaders/Particles/Additive (Soft)");
            //ParticleAlphaBlend = ResourceLoader.LoadAsset<Shader>("Legacy Shaders/Particles/Anim Alpha Blended");
            //UIParticleAdditive = ResourceLoader.LoadAsset<Shader>("UI Extensions/Particles/Additive");
            //UIParticleAlphaBlend = ResourceLoader.LoadAsset<Shader>("UI Extensions/Particles/Alpha Blended");
            //ToonShader = ResourceLoader.LoadAsset<Shader>("Custom/ToonShader");
            //UVAnimInChannel_AlphaBlend = ResourceLoader.LoadAsset<Shader>("PureDark/UVAnimInChannel_AlphaBlend");

            StereoRender = ResourceLoader.LoadAsset<Shader>("PureDark/StereoRender");
            VRCameraRig = ResourceLoader.LoadAsset<GameObject>("[VRCameraRig]");

        }


        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null)
                throw new ArgumentNullException("GetOrAddComponent: gameObject is null!");

            T comp = gameObject.GetComponent<T>();
            if (comp == null)
                comp = gameObject.AddComponent<T>();

            return comp;
        }
    }
}
