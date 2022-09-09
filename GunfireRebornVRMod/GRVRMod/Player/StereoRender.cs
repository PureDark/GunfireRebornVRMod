using UnityEngine;
using UnityEngine.Experimental.Rendering.LightweightPipeline;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.XR;
using System;
using VRMod.Assets;
using Valve.VR;
using UnhollowerBaseLib.Attributes;
using static VRMod.VRMod;
using UI;
using UnityEngine.Rendering.PostProcessing;

namespace VRMod.Player
{
    // 在OpaquePass之后插入渲染双眼画面的PASS，要在post processing前
    [Il2CppImplements(typeof(IAfterOpaquePass))]
    public class StereoRender : MonoBehaviour/*, IAfterOpaquePass*/
    {
        public StereoRender(IntPtr value) : base(value) { }

        private Camera _camera;
        private RenderTexture leftFarRT, rightFarRT;
        private RenderTexture leftNearRT, rightNearRT;
        private RenderTexture leftUIRT, rightUIRT;
        public StereoRenderPass stereoPass;
        public float separation = 0.031f;
        private float nearCamClipStart = 0.02f;
        private float nearCamClipEnd = 0.3f;
        private float farCamClipStart = 0.3f;
        private float farCamClipEnd = 800f;
        private float UICamClipStart = 0.02f;
        private float UICamClipEnd = 100f;

        public void Awake()
        {
            var cullingMask = -966787561;
            cullingMask |= 1 << Layer.Weapon;

            _camera = gameObject.GetComponent<Camera>();

            var screenCam = transform.Find("ScreenCam");
            screenCam.gameObject.AddComponent<OutLinePassMgr>();

            var uiPPLayer = CUIManager.instance.UICamera.GetComponent<PostProcessLayer>();
            var mPostProcessLayer = screenCam.gameObject.AddComponent<PostProcessLayer>();
            mPostProcessLayer.m_Resources = uiPPLayer.m_Resources;
            mPostProcessLayer.volumeLayer = uiPPLayer.volumeLayer;


            mPostProcessLayer = gameObject.AddComponent<PostProcessLayer>();
            mPostProcessLayer.m_Resources = uiPPLayer.m_Resources;
            mPostProcessLayer.volumeLayer = uiPPLayer.volumeLayer;

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
            leftFarCam.cullingMask = cullingMask;
            leftFarCam.stereoTargetEye = StereoTargetEyeMask.None;
            leftFarCam.clearFlags = CameraClearFlags.SolidColor;
            leftFarCam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            leftFarCam.nearClipPlane = farCamClipStart;
            leftFarCam.farClipPlane = farCamClipEnd;

            var leftNearEye = leftFarEye.Find("LeftNearEye");
            if (!leftNearEye)
                leftNearEye = new GameObject("LeftNearEye").transform;
            leftNearEye.parent = leftFarEye;
            leftNearEye.localPosition = new Vector3(0, 0, 0);
            leftNearEye.localEulerAngles = new Vector3(0, 0, 0);
            leftNearEye.gameObject.GetOrAddComponent<OutLinePassMgr>();

            var leftNearCam = leftNearEye.gameObject.GetOrAddComponent<Camera>();
            leftNearCam.cullingMask = cullingMask;
            leftNearCam.stereoTargetEye = StereoTargetEyeMask.None;
            leftNearCam.clearFlags = CameraClearFlags.SolidColor;
            leftNearCam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            leftNearCam.nearClipPlane = nearCamClipStart;
            leftNearCam.farClipPlane = nearCamClipEnd;

            var leftUITran = leftFarEye.Find("LeftUICam");
            if (!leftUITran)
                leftUITran = new GameObject("LeftUICam").transform;
            leftUITran.parent = leftNearEye;
            leftUITran.localPosition = new Vector3(0, 0, 0);
            leftUITran.localEulerAngles = new Vector3(0, 0, 0);
            leftUITran.gameObject.GetOrAddComponent<OutLinePassMgr>();

            var leftUICam = leftUITran.gameObject.GetOrAddComponent<Camera>();
            leftUICam.cullingMask = 1 << Layer.UI;
            leftUICam.stereoTargetEye = StereoTargetEyeMask.None;
            leftUICam.clearFlags = CameraClearFlags.SolidColor;
            leftUICam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            leftUICam.nearClipPlane = UICamClipStart;
            leftUICam.farClipPlane = UICamClipEnd;


            var rightFarEye = head.Find("RightEye");
            if (!rightFarEye)
                rightFarEye = new GameObject("RightEye").transform;
            rightFarEye.parent = head;
            rightFarEye.localPosition = new Vector3(separation, 0, 0);
            rightFarEye.localEulerAngles = new Vector3(0, 0, 0);
            rightFarEye.gameObject.GetOrAddComponent<OutLinePassMgr>();

            var rightFarCam = rightFarEye.gameObject.GetOrAddComponent<Camera>();
            rightFarCam.cullingMask = cullingMask;
            rightFarCam.stereoTargetEye = StereoTargetEyeMask.None;
            rightFarCam.clearFlags = CameraClearFlags.SolidColor;
            rightFarCam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            rightFarCam.nearClipPlane = farCamClipStart;
            rightFarCam.farClipPlane = farCamClipEnd;


            var rightNearEye = rightFarEye.Find("RightNearEye");

            if (!rightNearEye)
                rightNearEye = new GameObject("RightNearEye").transform;
            rightNearEye.parent = rightFarEye;
            rightNearEye.localPosition = new Vector3(0, 0, 0);
            rightNearEye.localEulerAngles = new Vector3(0, 0, 0);
            rightNearEye.gameObject.GetOrAddComponent<OutLinePassMgr>();

            var rightNearCam = rightNearEye.gameObject.GetOrAddComponent<Camera>();
            rightNearCam.cullingMask = cullingMask;
            rightNearCam.stereoTargetEye = StereoTargetEyeMask.None;
            rightNearCam.clearFlags = CameraClearFlags.SolidColor;
            rightNearCam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            rightNearCam.nearClipPlane = nearCamClipStart;
            rightNearCam.farClipPlane = nearCamClipEnd;

            var rightUITran = leftFarEye.Find("RightUICam");
            if (!rightUITran)
                rightUITran = new GameObject("RightUICam").transform;
            rightUITran.parent = rightNearEye;
            rightUITran.localPosition = new Vector3(0, 0, 0);
            rightUITran.localEulerAngles = new Vector3(0, 0, 0);
            rightUITran.gameObject.GetOrAddComponent<OutLinePassMgr>();

            var rightUICam = rightUITran.gameObject.GetOrAddComponent<Camera>();
            rightUICam.cullingMask = 1 << Layer.UI;
            rightUICam.stereoTargetEye = StereoTargetEyeMask.None;
            rightUICam.clearFlags = CameraClearFlags.SolidColor;
            rightUICam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            rightUICam.nearClipPlane = UICamClipStart;
            rightUICam.farClipPlane = UICamClipEnd;


            var headCam = GetComponent<Camera>();

            headCam.nearClipPlane = farCamClipStart;
            headCam.farClipPlane = farCamClipEnd;
            leftFarCam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            rightFarCam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

            headCam.nearClipPlane = nearCamClipStart;
            headCam.farClipPlane = nearCamClipEnd;
            leftNearCam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            rightNearCam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

            headCam.nearClipPlane = UICamClipStart;
            headCam.farClipPlane = UICamClipEnd;
            leftUICam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            rightUICam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

            int width = (XRSettings.eyeTextureWidth <= 0) ? 2208 : XRSettings.eyeTextureWidth;
            int height = (XRSettings.eyeTextureHeight <= 0) ? 2452 : XRSettings.eyeTextureHeight;
            if (leftFarRT == null)
                leftFarRT = new RenderTexture(width, height, 24, RenderTextureFormat.DefaultHDR);
            if (rightFarRT == null)
                rightFarRT = new RenderTexture(width, height, 24, RenderTextureFormat.DefaultHDR);
            if (leftNearRT == null)
                leftNearRT = new RenderTexture(width, height, 24, RenderTextureFormat.DefaultHDR);
            if (rightNearRT == null)
                rightNearRT = new RenderTexture(width, height, 24, RenderTextureFormat.DefaultHDR);
            if (leftUIRT == null)
                leftUIRT = new RenderTexture(width, height, 24, RenderTextureFormat.DefaultHDR);
            if (rightUIRT == null)
                rightUIRT = new RenderTexture(width, height, 24, RenderTextureFormat.DefaultHDR);
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
                // 因为Postprocessing一直会替换主相机的culling mask，要替换回0，不需要主相机渲染任何东西
                stereoRender._camera.cullingMask = 0;
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
