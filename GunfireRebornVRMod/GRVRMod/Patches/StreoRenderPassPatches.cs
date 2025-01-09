using HarmonyLib;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.LightweightPipeline;
using VRMod.Player;
using static UnityEngine.UI.Image;
using static VRMod.VRMod;

namespace VRMod.Patches
{
    [HarmonyPatch(typeof(BeginXRRenderingPass), nameof(BeginXRRenderingPass.Execute))]
    internal class InjectBeginXRRenderingPassExecute
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(EndXRRenderingPass), nameof(EndXRRenderingPass.Execute))]
    internal class InjectEndXRRenderingPassExecute
    {
        private static bool Prefix(ScriptableRenderer renderer, ref ScriptableRenderContext context, ref RenderingData renderingData)
        {
            VRPlayer.Instance.StereoRender.Execute(renderer, context, ref renderingData);
            return false;
        }
    }
}
