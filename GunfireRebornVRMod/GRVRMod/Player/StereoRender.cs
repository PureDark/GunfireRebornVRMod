using UnityEngine;
using UnityEngine.Experimental.Rendering.LightweightPipeline;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.XR;
using System;
using VRMod.Assets;
using VRMod.Core;
using Valve.VR;
using UnhollowerBaseLib.Attributes;

namespace VRMod.Player
{
    // 在TransparentPass之后插入渲染双眼画面的PASS
    [Il2CppImplements(typeof(IAfterTransparentPass))]
    public class StereoRender : MonoBehaviour/*, IAfterTransparentPass*/
    {
        public StereoRender(IntPtr value) : base(value) { }

        private RenderTexture leftFarRT, rightFarRT;
        private RenderTexture leftNearRT, rightNearRT;
        private RenderTexture leftUIRT, rightUIRT;
        public StereoRenderPass stereoPass;
        public float separation = 0.031f;

        public void Awake()
        {
            transform.Find("ScreenCam")?.gameObject.AddComponent<OutLinePassMgr>();
            var head = transform.Find("Head");
            if (!head)
                head = new GameObject("Head").transform;
            head.parent = transform;
            head.localPosition = Vector3.zero;
            head.localRotation = Quaternion.identity;

            var leftFarEye = head.Find("LeftEye");
            if (!leftFarEye)
                leftFarEye = new GameObject("LeftEye").transform;

            leftFarEye.parent = head;
            leftFarEye.localPosition = new Vector3(-separation, 0, 0);
            leftFarEye.localEulerAngles = new Vector3(0, 0, 0);
            leftFarEye.gameObject.GetOrAddComponent<OutLinePassMgr>();

            var leftFarCam = leftFarEye.gameObject.GetOrAddComponent<Camera>();
            leftFarCam.cullingMask = -966787561;
            leftFarCam.stereoTargetEye = StereoTargetEyeMask.None;
            leftFarCam.clearFlags = CameraClearFlags.SolidColor;
            leftFarCam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            leftFarCam.nearClipPlane = 0.95f;
            leftFarCam.farClipPlane = 1000f;

            var leftNearEye = leftFarEye.Find("LeftNearCam");
            if (!leftNearEye)
                leftNearEye = new GameObject("LeftNearCam").transform;
            leftNearEye.parent = leftFarEye;
            leftNearEye.localPosition = new Vector3(0, 0, 0);
            leftNearEye.localEulerAngles = new Vector3(0, 0, 0);
            leftNearEye.gameObject.GetOrAddComponent<OutLinePassMgr>();

            var leftNearCam = leftNearEye.gameObject.GetOrAddComponent<Camera>();
            leftNearCam.cullingMask = -966787561;
            leftNearCam.stereoTargetEye = StereoTargetEyeMask.None;
            leftNearCam.clearFlags = CameraClearFlags.SolidColor;
            leftNearCam.backgroundColor = new Color(0, 0, 0, 0);
            leftNearCam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            leftNearCam.nearClipPlane = 0.02f;
            leftNearCam.farClipPlane = 1.05f;

            var leftUITran = leftFarEye.Find("LeftUICam");
            if (!leftUITran)
                leftUITran = new GameObject("LeftUICam").transform;
            leftUITran.parent = leftNearEye;
            leftUITran.localPosition = new Vector3(0, 0, 0);
            leftUITran.localEulerAngles = new Vector3(0, 0, 0);
            leftUITran.gameObject.GetOrAddComponent<OutLinePassMgr>();

            var leftUICam = leftUITran.gameObject.GetOrAddComponent<Camera>();
            leftUICam.cullingMask = 32;
            leftUICam.stereoTargetEye = StereoTargetEyeMask.None;
            leftUICam.clearFlags = CameraClearFlags.SolidColor;
            leftUICam.backgroundColor = new Color(0, 0, 0, 0);
            leftUICam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            leftUICam.nearClipPlane = 0.02f;
            leftUICam.farClipPlane = 100f;


            var rightFarEye = head.Find("RightEye");
            if (!rightFarEye)
                rightFarEye = new GameObject("RightEye").transform;
            rightFarEye.parent = head;
            rightFarEye.localPosition = new Vector3(separation, 0, 0);
            rightFarEye.localEulerAngles = new Vector3(0, 0, 0);
            rightFarEye.gameObject.GetOrAddComponent<OutLinePassMgr>();

            var rightFarCam = rightFarEye.gameObject.GetOrAddComponent<Camera>();
            rightFarCam.cullingMask = -966787561;
            rightFarCam.stereoTargetEye = StereoTargetEyeMask.None;
            rightFarCam.clearFlags = CameraClearFlags.SolidColor;
            rightFarCam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            rightFarCam.nearClipPlane = 0.95f;
            rightFarCam.farClipPlane = 1000f;


            var rightNearEye = rightFarEye.Find("RightNearCam");

            if (!rightNearEye)
                rightNearEye = new GameObject("RightNearCam").transform;
            rightNearEye.parent = rightFarEye;
            rightNearEye.localPosition = new Vector3(0, 0, 0);
            rightNearEye.localEulerAngles = new Vector3(0, 0, 0);
            rightNearEye.gameObject.GetOrAddComponent<OutLinePassMgr>();

            var rightNearCam = rightNearEye.gameObject.GetOrAddComponent<Camera>();
            rightNearCam.cullingMask = -966787561;
            rightNearCam.stereoTargetEye = StereoTargetEyeMask.None;
            rightNearCam.clearFlags = CameraClearFlags.SolidColor;
            rightNearCam.backgroundColor = new Color(0, 0, 0, 0);
            rightNearCam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            rightNearCam.nearClipPlane = 0.02f;
            rightNearCam.farClipPlane = 1.05f;

            var rightUITran = leftFarEye.Find("RightUICam");
            if (!rightUITran)
                rightUITran = new GameObject("RightUICam").transform;
            rightUITran.parent = rightNearEye;
            rightUITran.localPosition = new Vector3(0, 0, 0);
            rightUITran.localEulerAngles = new Vector3(0, 0, 0);
            rightUITran.gameObject.GetOrAddComponent<OutLinePassMgr>();

            var rightUICam = rightUITran.gameObject.GetOrAddComponent<Camera>();
            rightUICam.cullingMask = 32;
            rightUICam.stereoTargetEye = StereoTargetEyeMask.None;
            rightUICam.clearFlags = CameraClearFlags.SolidColor;
            rightUICam.backgroundColor = new Color(0, 0, 0, 0);
            rightUICam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            rightUICam.nearClipPlane = 0.02f;
            rightUICam.farClipPlane = 100f;


            var headCam = GetComponent<Camera>();

            headCam.nearClipPlane = 0.95f;
            headCam.farClipPlane = 1000f;
            leftFarCam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            rightFarCam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

            headCam.nearClipPlane = 0.02f;
            headCam.farClipPlane = 1.05f;
            leftNearCam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            rightNearCam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

            headCam.nearClipPlane = 0.02f;
            headCam.farClipPlane = 100f;
            leftUICam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            rightUICam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

            int width = (XRSettings.eyeTextureWidth <= 0) ? 2208 : XRSettings.eyeTextureWidth;
            int height = (XRSettings.eyeTextureHeight <= 0) ? 2452 : XRSettings.eyeTextureHeight;
            if (leftFarRT == null)
                leftFarRT = new RenderTexture(width, height, 8, RenderTextureFormat.ARGB32);
            if (rightFarRT == null)
                rightFarRT = new RenderTexture(width, height, 8, RenderTextureFormat.ARGB32);
            if (leftNearRT == null)
                leftNearRT = new RenderTexture(width, height, 8, RenderTextureFormat.ARGB32);
            if (rightNearRT == null)
                rightNearRT = new RenderTexture(width, height, 8, RenderTextureFormat.ARGB32);
            if (leftUIRT == null)
                leftUIRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            if (rightUIRT == null)
                rightUIRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            leftFarRT.antiAliasing = 4;
            rightFarRT.antiAliasing = 4;
            leftNearRT.antiAliasing = 4;
            rightNearRT.antiAliasing = 4;
            leftUIRT.antiAliasing = 4;
            rightUIRT.antiAliasing = 4;
            leftFarCam.targetTexture = leftFarRT;
            rightFarCam.targetTexture = rightFarRT;
            leftNearCam.targetTexture = leftNearRT;
            rightNearCam.targetTexture = rightNearRT;
            leftUICam.targetTexture = leftUIRT;
            rightUICam.targetTexture = rightUIRT;

            stereoPass = new StereoRenderPass(this);

            Log.Info("XRSettings:" + XRSettings.eyeTextureWidth + "x" + XRSettings.eyeTextureHeight);
        }

        public StereoRenderPass GetPassToEnqueue(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorAttachmentHandle, RenderTargetHandle depthAttachmentHandle)
        {
            if (stereoPass == null)
                stereoPass = new StereoRenderPass(this);
            stereoPass.Setup(baseDescriptor, colorAttachmentHandle);
            return stereoPass;
        }

        //因为IL2CPP的限制，没法直接继承ScriptableRenderPass，转为注入在管线中没有特别大作用的pass
        //在StereoRenderPassPatches里将StartXRRenderingPass和EndXRRenderingPass都禁用了
        public class StereoRenderPass : EndXRRenderingPass
        {

            private static readonly string k_RenderTag = "Render Stereo Texture";
            //获取shader中的属性
            private static readonly int TempTargetId = Shader.PropertyToID("_TempTargetVREye");
            private StereoRender stereoRender;
            private Material stereoMaterial;

            RenderTextureDescriptor baseDescriptor;
            RenderTargetHandle colorHandle;

            public StereoRenderPass(StereoRender stereoRender)
            {
                this.stereoRender = stereoRender;

                var shader = VRAssets.StereoRender;
                if (shader == null)
                {
                    Debug.LogError("Shader not found.");
                    return;
                }
                stereoMaterial = CoreUtils.CreateEngineMaterial(shader);
                stereoMaterial.SetTexture("_LeftFarTex", stereoRender.leftFarRT);
                stereoMaterial.SetTexture("_RightFarTex", stereoRender.rightFarRT);
                stereoMaterial.SetTexture("_LeftNearTex", stereoRender.leftNearRT);
                stereoMaterial.SetTexture("_RightNearTex", stereoRender.rightNearRT);
                stereoMaterial.SetTexture("_LeftUITex", stereoRender.leftUIRT);
                stereoMaterial.SetTexture("_RightUITex", stereoRender.rightUIRT);
            }

            //获得渲染目标的引用
            public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorHandle)
            {
                this.baseDescriptor = baseDescriptor;
                this.colorHandle = colorHandle;
            }

            public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (!stereoRender.enabled)
                    return;
                if (stereoMaterial == null)
                {
                    Debug.LogError("Material not created.");
                    return;
                }
                //获取命令缓冲区并为其分配名称
                var cmd = CommandBufferPool.Get(k_RenderTag);
                //定义渲染过程
                Render(cmd);
                //执行渲染命令
                context.ExecuteCommandBuffer(cmd);
                //释放该渲染命令缓冲区
                CommandBufferPool.Release(cmd);
            }

            void Render(CommandBuffer cmd)
            {
                //获得屏幕RT
                var source = colorHandle.Identifier();
                int destination = TempTargetId;
                //获取宽高
                var w = baseDescriptor.width;
                var h = baseDescriptor.height;
                //使用临时纹理复制当前屏幕纹理
                cmd.GetTemporaryRT(destination, w, h, 0, FilterMode.Point, RenderTextureFormat.Default);
                cmd.Blit(source, destination);
                //使用材质的0pass渲染到临时纹理并复制到屏幕纹理
                cmd.Blit(destination, source, stereoMaterial, 0);
            }
        }

    }

}
