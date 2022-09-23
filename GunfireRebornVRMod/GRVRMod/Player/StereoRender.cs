using System;
using UI;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.LightweightPipeline;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.XR;
using Valve.VR;
using VRMod.Assets;
using static VRMod.VRMod;

namespace VRMod.Player
{
    // 在TransparentPass之后插入渲染双眼画面的PASS
    [Il2CppImplements(typeof(IAfterTransparentPass))]
    public class StereoRender : MonoBehaviour/*, IAfterTransparentPass*/
    {
        public StereoRender(IntPtr value) : base(value) { }

        public Camera HeadCam;
        public Camera LeftCam, RightCam;
        public Camera LeftUICam, RightUICam;
        public RenderTexture leftRT, rightRT;
        public RenderTexture leftUIRT, rightUIRT;
        public StereoRenderPass stereoPass;
        public float separation = 0.031f;
        private float farCamClipStart = 0.1f;
        private float farCamClipEnd = 300f;
        private float UICamClipStart = 0.1f;
        private float UICamClipEnd = 100f;
        public static int defaultCullingMask;
        private int currentWidth, currentHeight;

        public void Awake()
        {
            defaultCullingMask = -966787561;
            defaultCullingMask |= 1 << Layer.Weapon;

            var uiPPLayer = CUIManager.instance.UICamera.GetComponent<PostProcessLayer>();

            var screenCamTrans = transform.Find("ScreenCam");
            screenCamTrans.gameObject.AddComponent<OutLinePassMgr>();
            var screenCam = screenCamTrans.GetComponent<Camera>();
            screenCam.nearClipPlane = farCamClipStart;
            screenCam.farClipPlane = farCamClipEnd;

            var mPostProcessLayer = screenCam.gameObject.AddComponent<PostProcessLayer>();
            mPostProcessLayer.m_Resources = uiPPLayer.m_Resources;
            mPostProcessLayer.volumeLayer = uiPPLayer.volumeLayer;

            var head = transform.Find("Head");
            if (!head)
                head = new GameObject("Head").transform;
            head.parent = transform;
            head.localPosition = Vector3.zero;
            head.localRotation = Quaternion.identity;

            var leftEye = head.Find("LeftEye");
            if (!leftEye)
                leftEye = new GameObject("LeftEye").transform;

            leftEye.parent = head;
            leftEye.localPosition = new Vector3(-separation, 0, 0);
            leftEye.localEulerAngles = new Vector3(0, 0, 0);
            leftEye.gameObject.GetOrAddComponent<OutLinePassMgr>();

            mPostProcessLayer = leftEye.gameObject.AddComponent<PostProcessLayer>();
            mPostProcessLayer.m_Resources = uiPPLayer.m_Resources;
            mPostProcessLayer.volumeLayer = uiPPLayer.volumeLayer;

            LeftCam = leftEye.gameObject.GetOrAddComponent<Camera>();
            LeftCam.cullingMask = defaultCullingMask;
            LeftCam.stereoTargetEye = StereoTargetEyeMask.None;
            LeftCam.clearFlags = CameraClearFlags.SolidColor;
            LeftCam.backgroundColor = new Color(0, 0, 0, 0);
            LeftCam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            LeftCam.nearClipPlane = farCamClipStart;
            LeftCam.farClipPlane = farCamClipEnd;
            LeftCam.depth = 0;

            var leftUITran = leftEye.Find("LeftUICam");
            if (!leftUITran)
                leftUITran = new GameObject("LeftUICam").transform;
            leftUITran.parent = leftEye;
            leftUITran.localPosition = new Vector3(0, 0, 0);
            leftUITran.localEulerAngles = new Vector3(0, 0, 0);
            leftUITran.gameObject.GetOrAddComponent<OutLinePassMgr>();

            LeftUICam = leftUITran.gameObject.GetOrAddComponent<Camera>();
            LeftUICam.cullingMask = 1 << Layer.UI;
            LeftUICam.stereoTargetEye = StereoTargetEyeMask.None;
            LeftUICam.clearFlags = CameraClearFlags.SolidColor;
            LeftUICam.backgroundColor = new Color(0, 0, 0, 0);
            LeftUICam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            LeftUICam.nearClipPlane = UICamClipStart;
            LeftUICam.farClipPlane = UICamClipEnd;


            var rightEye = head.Find("RightEye");
            if (!rightEye)
                rightEye = new GameObject("RightEye").transform;
            rightEye.parent = head;
            rightEye.localPosition = new Vector3(separation, 0, 0);
            rightEye.localEulerAngles = new Vector3(0, 0, 0);
            rightEye.gameObject.GetOrAddComponent<OutLinePassMgr>();

            mPostProcessLayer = rightEye.gameObject.AddComponent<PostProcessLayer>();
            mPostProcessLayer.m_Resources = uiPPLayer.m_Resources;
            mPostProcessLayer.volumeLayer = uiPPLayer.volumeLayer;

            RightCam = rightEye.gameObject.GetOrAddComponent<Camera>();
            RightCam.cullingMask = defaultCullingMask;
            RightCam.stereoTargetEye = StereoTargetEyeMask.None;
            RightCam.clearFlags = CameraClearFlags.SolidColor;
            RightCam.backgroundColor = new Color(0, 0, 0, 0);
            RightCam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            RightCam.nearClipPlane = farCamClipStart;
            RightCam.farClipPlane = farCamClipEnd;
            RightCam.depth = 0;

            var rightUITran = leftEye.Find("RightUICam");
            if (!rightUITran)
                rightUITran = new GameObject("RightUICam").transform;
            rightUITran.parent = rightEye;
            rightUITran.localPosition = new Vector3(0, 0, 0);
            rightUITran.localEulerAngles = new Vector3(0, 0, 0);
            rightUITran.gameObject.GetOrAddComponent<OutLinePassMgr>();

            RightUICam = rightUITran.gameObject.GetOrAddComponent<Camera>();
            RightUICam.cullingMask = 1 << Layer.UI;
            RightUICam.stereoTargetEye = StereoTargetEyeMask.None;
            RightUICam.clearFlags = CameraClearFlags.SolidColor;
            RightUICam.backgroundColor = new Color(0, 0, 0, 0);
            RightUICam.fieldOfView = (SteamVR.instance.fieldOfView > 0) ? SteamVR.instance.fieldOfView : 109.363f;
            RightUICam.nearClipPlane = UICamClipStart;
            RightUICam.farClipPlane = UICamClipEnd;

            UpdateProjectionMatrix();
            UpdateResolution();

            stereoPass = new StereoRenderPass(this);

            Log.Info("XRSettings:" + XRSettings.eyeTextureWidth + "x" + XRSettings.eyeTextureHeight);
        }

        public void SetCameraMask(int mask)
        {
            LeftCam.cullingMask = mask;
            RightCam.cullingMask = mask;
        }

        public void UpdateProjectionMatrix()
        {
            HeadCam = GetComponent<Camera>();

            HeadCam.nearClipPlane = farCamClipStart;
            HeadCam.farClipPlane = farCamClipEnd;
            LeftCam.projectionMatrix = HeadCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            RightCam.projectionMatrix = HeadCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

            HeadCam.nearClipPlane = UICamClipStart;
            HeadCam.farClipPlane = UICamClipEnd;
            LeftUICam.projectionMatrix = HeadCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            RightUICam.projectionMatrix = HeadCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
        }

        public void UpdateResolution()
        {
            currentWidth = (XRSettings.eyeTextureWidth <= 0) ? 2208 : XRSettings.eyeTextureWidth;
            currentHeight = (XRSettings.eyeTextureHeight <= 0) ? 2452 : XRSettings.eyeTextureHeight;
            if (leftRT != null)
                Destroy(leftRT);
            if (rightRT != null)
                Destroy(rightRT);
            if (leftUIRT != null)
                Destroy(leftUIRT);
            if (rightUIRT != null)
                Destroy(rightUIRT);
            leftRT = new RenderTexture(currentWidth, currentHeight, 24, RenderTextureFormat.DefaultHDR);
            rightRT = new RenderTexture(currentWidth, currentHeight, 24, RenderTextureFormat.DefaultHDR);
            leftUIRT = new RenderTexture(currentWidth, currentHeight, 24, RenderTextureFormat.DefaultHDR);
            rightUIRT = new RenderTexture(currentWidth, currentHeight, 24, RenderTextureFormat.DefaultHDR);
            leftRT.antiAliasing = 4;
            rightRT.antiAliasing = 4;
            leftUIRT.antiAliasing = 4;
            rightUIRT.antiAliasing = 4;
            LeftCam.targetTexture = leftRT;
            RightCam.targetTexture = rightRT;
            LeftUICam.targetTexture = leftUIRT;
            RightUICam.targetTexture = rightUIRT;
        }

        public void FixedUpdate()
        {
            if (currentWidth < XRSettings.eyeTextureWidth || currentHeight < XRSettings.eyeTextureHeight)
            {
                UpdateResolution();
                stereoPass.UpdateResolution();
            }
        }

        public StereoRenderPass GetPassToEnqueue(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorAttachmentHandle, RenderTargetHandle depthAttachmentHandle)
        {
            if (stereoPass == null)
                stereoPass = new StereoRenderPass(this);
            stereoPass.Setup(baseDescriptor, colorAttachmentHandle);
            return stereoPass;
        }

        public void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if(stereoPass.isRendering)
                stereoPass.Execute(renderer, context, ref renderingData);
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
            public bool isRendering;

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
                UpdateResolution();
            }


            public void UpdateResolution()
            {
                stereoMaterial.SetTexture("_LeftFirstTex", stereoRender.leftRT);
                stereoMaterial.SetTexture("_RightFirstTex", stereoRender.rightRT);
                stereoMaterial.SetTexture("_LeftLastTex", stereoRender.leftUIRT);
                stereoMaterial.SetTexture("_RightLastTex", stereoRender.rightUIRT);
                stereoMaterial.SetFloat("_AlphaMultiplier", 2);
            }

            //获得渲染目标的引用
            public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorHandle)
            {
                this.baseDescriptor = baseDescriptor;
                this.colorHandle = colorHandle;
                isRendering = true;
                // 因为Postprocessing一直会替换主相机的culling mask，要替换回0，不需要主相机渲染任何东西
                stereoRender.HeadCam.cullingMask = 0;
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

                var cmd = CommandBufferPool.Get(k_RenderTag);
                var source = colorHandle.Identifier();
                int destination = TempTargetId;
                cmd.GetTemporaryRT(destination, baseDescriptor);
                cmd.Blit(destination, source, stereoMaterial, 0);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);

                isRendering = false;
            }
        }

    }

}
