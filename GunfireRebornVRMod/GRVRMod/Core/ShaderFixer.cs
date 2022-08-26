using UnityEngine;
using VRMod.Assets;
using System;
using VRMod.UI;
using System.Collections.Generic;
using VRMod.Patches;
using UnityEngine.SceneManagement;
using UnityEngine.Experimental.Rendering.LightweightPipeline;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace VRMod.Core
{
    public class ShaderFixer : MonoBehaviour
    {
        public ShaderFixer(IntPtr value) : base(value) { }

        public Dictionary<string, int> shaderCount = new Dictionary<string, int>();

        private void Awake()
        {
            HarmonyPatches.onSceneLoaded += OnSceneLoaded;
        }

        private void Update()
        {
            MenuFix.SetDebugUICamera();

            //if (Input.GetKeyUp(KeyCode.H))
            //{
            //    CUIManager.instance.ShowUIForm(UIFormName.CHARATER_PANEL);
            //    TimeLineManager instance = TimeLineManager.Instance;
            //    int curSid = CharaterData.CurFightheroSID;
            //    //instance.PlayHeroAndOther(curFightheroSID, UINameDefine.Home, true);
            //    instance.PlayTimeLine(UINameDefine.Home, String.Format("Hero{0}", curSid), true);
            //    //int curFightheroSID2 = CharaterData.CurFightheroSID;
            //    //CharaterData.SetCurSelectHeroSID(curFightheroSID2);
            //}
            //if (Input.GetKeyUp(KeyCode.P))
            //{
            //    if(GraphicsSettings.renderPipelineAsset != null)
            //        GraphicsSettings.renderPipelineAsset = null;
            //    Renderer[] renderers = Resources.FindObjectsOfTypeAll<Renderer>();
            //    FixRenderers(renderers);
            //}

        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //if(GraphicsSettings.renderPipelineAsset != null)
            //    GraphicsSettings.renderPipelineAsset = null;
            //if(scene.name == "home")
            //{
            //    var renderers = FindObjectsOfType<Renderer>();
            //    FixRenderers(renderers);
            //}
        }

        public static void FixRenderers(Renderer[] renderers)
        {

            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.materials)
                {
                    FixMaterial(mat);
                }
            }
        }

        public static void FixMaterial(Material mat)
        {
            //if (mat.shader.name.Contains("M1/Scene"))
            //{
            //    mat.shader = VRAssets.VertexLit;
            //}
            //else if (mat.shader.name.Contains("M1/Character"))
            //{
            //    if (mat.GetFloat("_WorkflowMode") > 0)
            //        mat.shader = VRAssets.Standard;
            //    else
            //        mat.shader = VRAssets.StandardSpecular;

            //}
            //else if (mat.shader.name.Contains("Shader Graphs/"))
            //{
            //    if (mat.shader.name.Contains("Particle_Additive") || mat.shader.name.Contains("Particle_AlphaBlend"))
            //    {
            //        var tex = mat.GetTexture("_ParticleTexture");
            //        var color = mat.GetColor("_MainColor");
            //        var tilingAndOffset = mat.GetVector("_ParticleTextureTilingAndOffset");
            //        mat.shader = mat.shader.name.Contains("Particle_Additive") ? VRAssets.ParticleAdditive : VRAssets.ParticleAlphaBlend;
            //        mat.SetTexture("_MainTex", tex);
            //        mat.SetColor("_TintColor", color);
            //        mat.SetTextureScale("_MainTex", new Vector2(tilingAndOffset.x, tilingAndOffset.y));
            //        mat.SetTextureOffset("_MainTex", new Vector2(tilingAndOffset.z, tilingAndOffset.w));
            //    }
            //    else if (mat.shader.name.Contains("UVAnimInChannel_AlphaBlend"))
            //    {
            //        var color = mat.GetColor("_MainColor");
            //        color.a *= 0f;
            //        mat.shader = VRAssets.ParticleAlphaBlend;
            //        mat.SetColor("_MainColor", color);
            //    }
            //}
            //else if (mat.shader.name.Contains("NBFX/"))
            //{
            //    if (mat.shader.name.Contains("Particle_Additive") || mat.shader.name.Contains("Particle_AlphaBlend"))
            //    {
            //        var color = mat.GetColor("_MainColor");
            //        var softPower = mat.GetFloat("_SoftPower");
            //        var _SoftRatio = mat.GetFloat("_SoftRatio");
            //        mat.shader = mat.shader.name.Contains("Particle_Additive") ? VRAssets.ParticleAdditive : VRAssets.ParticleAlphaBlend;
            //        mat.SetColor("_TintColor", color);
            //        mat.SetFloat("_InvFade", softPower * _SoftRatio);
            //    }
            //}
        }




        //public void GetMaterialsInObject(string name)
        //{
        //    shaderCount = new Dictionary<string, int>();
        //    GameObject root = GameObject.Find(name);

        //    Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        //    Log.Info("renderers.lengh=" + renderers.Length);
        //    foreach (Renderer renderer in renderers)
        //    {
        //        if (renderer.material)
        //            LogM1Character(renderer.material);
        //    }

        //    foreach (KeyValuePair<string, int> kv in shaderCount)
        //    {
        //        Log.Info(kv.Key + " : " + kv.Value);
        //    }
        //}

        //private void LogMaterial(Material mat)
        //{
        //    string text = "Material \n";
        //    text += "Name: " + mat.name + "\n";
        //    text += "Color: " + mat.color + "\n";
        //    text += "Shader: " + mat.shader.name + "\n";
        //    text += "shaderKeywords: ";
        //    foreach (var item in mat.shaderKeywords)
        //    {
        //        text += item;
        //    }
        //    text += "\n";
        //    Log.Info(text);
        //    if (shaderCount.ContainsKey(mat.shader.name))
        //        shaderCount[mat.shader.name]++;
        //    else
        //        shaderCount.Add(mat.shader.name, 1);
        //}
        //private void LogUVAnimAlphaBlend(Material mat)
        //{
        //    string text = "\nMaterial";
        //    text += "\n Name: " + mat.name;
        //    text += "\n Shader: " + mat.shader.name;
        //    text += "\n _MainTexture: " + mat.GetTexture("_MainTexture");
        //    text += "\n _MainTexTilingAndOffset: " + mat.GetVector("_MainTexTilingAndOffset");
        //    text += "\n _MainColor: " + mat.GetColor("_MainColor");
        //    text += "\n _UVXSpeed: " + mat.GetFloat("_UVXSpeed");
        //    text += "\n _UVYSpeed: " + mat.GetFloat("_UVYSpeed");
        //    text += "\n _MaskTex: " + mat.GetTexture("_MaskTex");
        //    text += "\n _MaskTexTilingAndOffset: " + mat.GetVector("_MaskTexTilingAndOffset");
        //    text += "\n _MaskTexUVXSpeed: " + mat.GetFloat("_MaskTexUVXSpeed");
        //    text += "\n _MaskTexUVYSpeed: " + mat.GetFloat("_MaskTexUVYSpeed");


        //    text += "\nshaderKeywords: ";
        //    foreach (var item in mat.shaderKeywords)
        //    {
        //        text += item;
        //    }
        //    text += "\n";
        //    Log.Info(text);
        //    if (shaderCount.ContainsKey(mat.shader.name))
        //        shaderCount[mat.shader.name]++;
        //    else
        //        shaderCount.Add(mat.shader.name, 1);
        //}


        //public void SaveTexture(Texture tex, string name, string path)
        //{
        //    if (tex == null)
        //        return;
        //    RenderTexture rt = new RenderTexture(tex.width, tex.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        //    Graphics.Blit(tex, rt);
        //    byte[] bytes = toTexture2D(rt).EncodeToPNG();
        //    System.IO.File.WriteAllBytes(path +"/"+ name + ".png", bytes);
        //}
        //Texture2D toTexture2D(RenderTexture rTex)
        //{
        //    Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        //    RenderTexture.active = rTex;
        //    tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        //    tex.Apply();
        //    return tex;
        //}

        //private void LogM1Character(Material mat)
        //{


        //    string text = "\nMaterial";
        //    text += "\n Name: " + mat.name;
        //    text += "\n Shader: " + mat.shader.name;
        //    text += "\n _WorkflowMode : " + mat.GetFloat("_WorkflowMode");
        //    text += "\n _RenderQueueOffset  : " + mat.GetFloat("_RenderQueueOffset");
        //    text += "\n _Color : " + mat.GetColor("_Color");
        //    text += "\n _MainTex : " + mat.GetTexture("_MainTex");
        //    text += "\n _OverlayColor : " + mat.GetColor("_OverlayColor");
        //    text += "\n _Cutoff : " + mat.GetFloat("_Cutoff");
        //    text += "\n _DitherClip : " + mat.GetFloat("_DitherClip");
        //    text += "\n _DitherScale : " + mat.GetFloat("_DitherScale");
        //    text += "\n _Glossiness : " + mat.GetFloat("_Glossiness");
        //    text += "\n _GlossMapScale : " + mat.GetFloat("_GlossMapScale");
        //    text += "\n _SmoothnessTextureChannel : " + mat.GetFloat("_SmoothnessTextureChannel");
        //    text += "\n _Metallic : " + mat.GetFloat("_Metallic");
        //    text += "\n _MetallicGlossMap : " + mat.GetTexture("_MetallicGlossMap");
        //    text += "\n _SpecColor : " + mat.GetColor("_SpecColor");
        //    text += "\n _SpecGlossMap : " + mat.GetTexture("_SpecGlossMap");
        //    text += "\n _SpecularHighlights : " + mat.GetFloat("_SpecularHighlights");
        //    text += "\n _GlossyReflections : " + mat.GetFloat("_GlossyReflections");
        //    text += "\n _ReflectionRatio : " + mat.GetFloat("_ReflectionRatio");
        //    text += "\n _MainLightIntensity : " + mat.GetFloat("_MainLightIntensity");
        //    text += "\n _OcclusionStrength : " + mat.GetFloat("_OcclusionStrength");
        //    text += "\n _OcclusionMap : " + mat.GetTexture("_OcclusionMap");
        //    text += "\n _EmissionColor : " + mat.GetColor("_EmissionColor");
        //    text += "\n _EmissionMap : " + mat.GetTexture("_EmissionMap");
        //    text += "\n _FogFactor : " + mat.GetFloat("_FogFactor");
        //    text += "\n _Surface : " + mat.GetFloat("_Surface");
        //    text += "\n _Blend : " + mat.GetFloat("_Blend");
        //    text += "\n _AlphaClip : " + mat.GetFloat("_AlphaClip");
        //    text += "\n _SrcBlend : " + mat.GetFloat("_SrcBlend");
        //    text += "\n _DstBlend : " + mat.GetFloat("_DstBlend");
        //    text += "\n _ENABLE_OUTLINE : " + mat.GetFloat("_ENABLE_OUTLINE");
        //    text += "\n _OutlineColor : " + mat.GetColor("_OutlineColor");
        //    text += "\n _OutlineWidth : " + mat.GetFloat("_OutlineWidth");
        //    text += "\n _OutLineRef : " + mat.GetFloat("_OutLineRef");
        //    text += "\n _InfluenceByViewDis : " + mat.GetFloat("_InfluenceByViewDis");
        //    text += "\n _ENABLE_HIDE_OUTLINE : " + mat.GetFloat("_ENABLE_HIDE_OUTLINE");
        //    text += "\n _HideOutlineColor : " + mat.GetColor("_HideOutlineColor");
        //    text += "\n _ENABLE_DISSOLVE : " + mat.GetFloat("_ENABLE_DISSOLVE");
        //    text += "\n _EnableDirectionalDissolve : " + mat.GetFloat("_EnableDirectionalDissolve");
        //    text += "\n _DissolveNoise : " + mat.GetTexture("_DissolveNoise");
        //    text += "\n _DissolveEdgeAlbedo : " + mat.GetColor("_DissolveEdgeAlbedo");
        //    text += "\n _DissolveEdgeWidth : " + mat.GetFloat("_DissolveEdgeWidth");
        //    text += "\n _DissolveEdgeTex : " + mat.GetTexture("_DissolveEdgeTex");
        //    text += "\n _DissolveAmount : " + mat.GetFloat("_DissolveAmount");
        //    text += "\n _DissolveStart : " + mat.GetVector("_DissolveStart");
        //    text += "\n _DissolveEnd : " + mat.GetVector("_DissolveEnd");
        //    text += "\n _DissolveBand : " + mat.GetVector("_DissolveBand");
        //    text += "\n _DirChange : " + mat.GetFloat("_DirChange");
        //    text += "\n _DirChangeTex : " + mat.GetTexture("_DirChangeTex");
        //    text += "\n _DirChangeColor : " + mat.GetColor("_DirChangeColor");
        //    text += "\n _EnableShield : " + mat.GetFloat("_EnableShield");
        //    text += "\n _EdgeColor : " + mat.GetColor("_EdgeColor");
        //    text += "\n _EdgeRatio : " + mat.GetFloat("_EdgeRatio");
        //    text += "\n _FresnelAniDir : " + mat.GetVector("_FresnelAniDir");
        //    text += "\n _FresnelAniTex : " + mat.GetTexture("_FresnelAniTex");
        //    text += "\n _FresnelAniRatio : " + mat.GetFloat("_FresnelAniRatio");
        //    text += "\n _EnableSmoothFresnel : " + mat.GetFloat("_EnableSmoothFresnel");
        //    text += "\n _FresnelMap1 : " + mat.GetTexture("_FresnelMap1");
        //    text += "\n _FresnelScale1 : " + mat.GetFloat("_FresnelScale1");
        //    text += "\n _FresnelMap2 : " + mat.GetTexture("_FresnelMap2");
        //    text += "\n _FresnelScale2 : " + mat.GetFloat("_FresnelScale2");
        //    text += "\n _ENABLE_UV_ANIMATION : " + mat.GetFloat("_ENABLE_UV_ANIMATION");
        //    text += "\n _AnimationDir : " + mat.GetVector("_AnimationDir");
        //    text += "\n _AnimationMask : " + mat.GetTexture("_AnimationMask");
        //    text += "\n _AnimationTex : " + mat.GetTexture("_AnimationTex");
        //    text += "\n _AnimationTex2 : " + mat.GetTexture("_AnimationTex2");
        //    text += "\n _AnimationRatio : " + mat.GetFloat("_AnimationRatio");
        //    text += "\n _AnimationColor : " + mat.GetColor("_AnimationColor");
        //    text += "\n _Use2UV : " + mat.GetFloat("_Use2UV");
        //    text += "\n _AnimationBlendFunc : " + mat.GetFloat("_AnimationBlendFunc");
        //    text += "\n _ZTest : " + mat.GetFloat("_ZTest");

        //    text += "\nshaderKeywords: ";
        //    foreach (var item in mat.shaderKeywords)
        //    {
        //        text += item + " ";
        //    }
        //    text += "\n";
        //    Log.Info(text);

        //    string path = "F:/GithubMods/GunfireReborn/GunfireRebornVR/GunfireRebornVRUnity/Exported/dump/" + mat.name;
        //    System.IO.Directory.CreateDirectory(path);

        //    SaveTexture(mat.GetTexture("_MainTex"), "_MainTex", path);
        //    SaveTexture(mat.GetTexture("_MetallicGlossMap"), "_MetallicGlossMap", path);
        //    SaveTexture(mat.GetTexture("_SpecGlossMap"), "_SpecGlossMap", path);
        //    SaveTexture(mat.GetTexture("_OcclusionMap"), "_OcclusionMap", path);
        //    SaveTexture(mat.GetTexture("_EmissionMap"), "_EmissionMap", path);
        //    SaveTexture(mat.GetTexture("_DissolveNoise"), "_DissolveNoise", path);
        //    SaveTexture(mat.GetTexture("_DissolveEdgeTex"), "_DissolveEdgeTex", path);
        //    SaveTexture(mat.GetTexture("_DirChangeTex"), "_DirChangeTex", path);
        //    SaveTexture(mat.GetTexture("_FresnelAniTex"), "_FresnelAniTex", path);
        //    SaveTexture(mat.GetTexture("_FresnelMap1"), "_FresnelMap1", path);
        //    SaveTexture(mat.GetTexture("_FresnelMap2"), "_FresnelMap2", path);
        //    SaveTexture(mat.GetTexture("_AnimationMask"), "_AnimationMask", path);
        //    SaveTexture(mat.GetTexture("_AnimationTex"), "_AnimationTex", path);
        //    SaveTexture(mat.GetTexture("_AnimationTex2"), "_AnimationTex2", path);


        //    System.IO.File.WriteAllText("F:/GithubMods/GunfireReborn/GunfireRebornVR/GunfireRebornVRUnity/Exported/dump/" + mat.name + ".txt", text);

        //    if (shaderCount.ContainsKey(mat.shader.name))
        //        shaderCount[mat.shader.name]++;
        //    else
        //        shaderCount.Add(mat.shader.name, 1);
        //}
    }
}
