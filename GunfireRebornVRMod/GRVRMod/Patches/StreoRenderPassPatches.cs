using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.LightweightPipeline;
using UnityEngine.Rendering;
using VRMod.Core;
using VRMod.Player;

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

    [HarmonyPatch(typeof(RenderTransparentForwardPass), nameof(RenderTransparentForwardPass.Setup))]
    internal class InjectRenderTransparentForwardPassSetup
    {
        private static void Prefix(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorAttachmentHandle, RenderTargetHandle depthAttachmentHandle, RendererConfiguration configuration)
        {
            if (VRPlayer.Instance && VRPlayer.Instance.StereoRender)
            { 
                VRPlayer.Instance.StereoRender.stereoPass.Setup(baseDescriptor, colorAttachmentHandle);
            }
        }
    }

    [HarmonyPatch(typeof(EndXRRenderingPass), nameof(EndXRRenderingPass.Execute))]
    internal class InjectEndXRRenderingPassExecute
    {
        private static bool Prefix(ScriptableRenderer renderer, ref ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (VRPlayer.Instance && VRPlayer.Instance.StereoRender)
            {
                VRPlayer.Instance.StereoRender.stereoPass.Execute(renderer, context, ref renderingData);
            }
            return false;
        }
    }
}
