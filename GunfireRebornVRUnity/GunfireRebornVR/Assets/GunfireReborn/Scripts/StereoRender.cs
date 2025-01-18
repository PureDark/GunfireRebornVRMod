//using UnityEngine;
//using UnityEngine.Experimental.Rendering.LightweightPipeline;
//using UnityEngine.Experimental.Rendering;
//using UnityEngine.Rendering;
//using UnityEngine.Rendering.PostProcessing;
//using UnityEngine.XR;
//using Valve.VR;
//using System;

//[ExecuteInEditMode]
//public class StereoRender : MonoBehaviour, IAfterOpaquePass
//{

//    private Camera _camera;
//    public RenderTexture leftFarRT, rightFarRT;
//    public RenderTexture leftNearRT, rightNearRT;
//    public RenderTexture leftUIRT, rightUIRT;
//    public StereoRenderPass stereoPass;
//    public float separation = 0.031f;
//    private float nearCamClipStart = 0.02f;
//    private float nearCamClipEnd = 0.3f;
//    private float farCamClipStart = 0.3f;
//    private float farCamClipEnd = 800f;
//    private float UICamClipStart = 0.02f;
//    private float UICamClipEnd = 100f;

//    public void Awake()
//    {
//        var cullingMask = -966787561;

//        _camera = gameObject.GetComponent<Camera>();

//        var head = transform.Find("Head");
//        if (!head)
//            head = new GameObject("Head").transform;
//        head.parent = transform;
//        head.localPosition = Vector3.zero;
//        head.localRotation = Quaternion.identity;

//        var leftFarEye = head.Find("LeftEye");
//        if (!leftFarEye)
//            leftFarEye = new GameObject("LeftEye").transform;

//        leftFarEye.parent = head;
//        leftFarEye.localPosition = new Vector3(-separation, 0, 0);
//        leftFarEye.localEulerAngles = new Vector3(0, 0, 0);

//        var leftFarCam = leftFarEye.gameObject.GetOrAddComponent<Camera>();
//        leftFarCam.cullingMask = cullingMask;
//        leftFarCam.stereoTargetEye = StereoTargetEyeMask.None;
//        leftFarCam.clearFlags = CameraClearFlags.SolidColor;
//        leftFarCam.fieldOfView = 109.363f;
//        leftFarCam.nearClipPlane = farCamClipStart;
//        leftFarCam.farClipPlane = farCamClipEnd;
//        leftFarCam.depth = 0;

//        var leftNearEye = leftFarEye.Find("LeftNearEye");
//        if (!leftNearEye)
//            leftNearEye = new GameObject("LeftNearEye").transform;
//        leftNearEye.parent = leftFarEye;
//        leftNearEye.localPosition = new Vector3(0, 0, 0);
//        leftNearEye.localEulerAngles = new Vector3(0, 0, 0);

//        var leftNearCam = leftNearEye.gameObject.GetOrAddComponent<Camera>();
//        leftNearCam.cullingMask = cullingMask;
//        leftNearCam.stereoTargetEye = StereoTargetEyeMask.None;
//        leftNearCam.clearFlags = CameraClearFlags.SolidColor;
//        leftNearCam.backgroundColor = new Color(0, 0, 0, 0);
//        leftNearCam.fieldOfView = 109.363f;
//        leftNearCam.nearClipPlane = nearCamClipStart;
//        leftNearCam.farClipPlane = nearCamClipEnd;
//        leftNearCam.depth = 1;

//        var leftUITran = leftFarEye.Find("LeftUICam");
//        if (!leftUITran)
//            leftUITran = new GameObject("LeftUICam").transform;
//        leftUITran.parent = leftNearEye;
//        leftUITran.localPosition = new Vector3(0, 0, 0);
//        leftUITran.localEulerAngles = new Vector3(0, 0, 0);

//        var leftUICam = leftUITran.gameObject.GetOrAddComponent<Camera>();
//        leftUICam.cullingMask = 32;
//        leftUICam.stereoTargetEye = StereoTargetEyeMask.None;
//        leftUICam.clearFlags = CameraClearFlags.SolidColor;
//        leftUICam.backgroundColor = new Color(0, 0, 0, 0);
//        leftUICam.fieldOfView = 109.363f;
//        leftUICam.nearClipPlane = UICamClipStart;
//        leftUICam.farClipPlane = UICamClipEnd;


//        var rightFarEye = head.Find("RightEye");
//        if (!rightFarEye)
//            rightFarEye = new GameObject("RightEye").transform;
//        rightFarEye.parent = head;
//        rightFarEye.localPosition = new Vector3(separation, 0, 0);
//        rightFarEye.localEulerAngles = new Vector3(0, 0, 0);

//        var rightFarCam = rightFarEye.gameObject.GetOrAddComponent<Camera>();
//        rightFarCam.cullingMask = cullingMask;
//        rightFarCam.stereoTargetEye = StereoTargetEyeMask.None;
//        rightFarCam.clearFlags = CameraClearFlags.SolidColor;
//        rightFarCam.fieldOfView = 109.363f;
//        rightFarCam.nearClipPlane = farCamClipStart;
//        rightFarCam.farClipPlane = farCamClipEnd;
//        rightFarCam.depth = 0;


//        var rightNearEye = rightFarEye.Find("RightNearEye");

//        if (!rightNearEye)
//            rightNearEye = new GameObject("RightNearEye").transform;
//        rightNearEye.parent = rightFarEye;
//        rightNearEye.localPosition = new Vector3(0, 0, 0);
//        rightNearEye.localEulerAngles = new Vector3(0, 0, 0);

//        var rightNearCam = rightNearEye.gameObject.GetOrAddComponent<Camera>();
//        rightNearCam.cullingMask = cullingMask;
//        rightNearCam.stereoTargetEye = StereoTargetEyeMask.None;
//        rightNearCam.clearFlags = CameraClearFlags.SolidColor;
//        rightNearCam.backgroundColor = new Color(0, 0, 0, 0);
//        rightNearCam.fieldOfView = 109.363f;
//        rightNearCam.nearClipPlane = nearCamClipStart;
//        rightNearCam.farClipPlane = nearCamClipEnd;
//        rightNearCam.depth = 1;

//        var rightUITran = leftFarEye.Find("RightUICam");
//        if (!rightUITran)
//            rightUITran = new GameObject("RightUICam").transform;
//        rightUITran.parent = rightNearEye;
//        rightUITran.localPosition = new Vector3(0, 0, 0);
//        rightUITran.localEulerAngles = new Vector3(0, 0, 0);

//        var rightUICam = rightUITran.gameObject.GetOrAddComponent<Camera>();
//        rightUICam.cullingMask = 32;
//        rightUICam.stereoTargetEye = StereoTargetEyeMask.None;
//        rightUICam.clearFlags = CameraClearFlags.SolidColor;
//        rightUICam.backgroundColor = new Color(0, 0, 0, 0);
//        rightUICam.fieldOfView = 109.363f;
//        rightUICam.nearClipPlane = UICamClipStart;
//        rightUICam.farClipPlane = UICamClipEnd;


//        var headCam = GetComponent<Camera>();

//        headCam.nearClipPlane = farCamClipStart;
//        headCam.farClipPlane = farCamClipEnd;
//        leftFarCam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
//        rightFarCam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

//        headCam.nearClipPlane = nearCamClipStart;
//        headCam.farClipPlane = nearCamClipEnd;
//        leftNearCam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
//        rightNearCam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

//        headCam.nearClipPlane = UICamClipStart;
//        headCam.farClipPlane = UICamClipEnd;
//        leftUICam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
//        rightUICam.projectionMatrix = headCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

//        int width = (XRSettings.eyeTextureWidth <= 0) ? 2208 : XRSettings.eyeTextureWidth;
//        int height = (XRSettings.eyeTextureHeight <= 0) ? 2452 : XRSettings.eyeTextureHeight;
//        if (leftFarRT == null)
//            leftFarRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
//        if (rightFarRT == null)
//            rightFarRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
//        if (leftNearRT == null)
//            leftNearRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
//        if (rightNearRT == null)
//            rightNearRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
//        if (leftUIRT == null)
//            leftUIRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
//        if (rightUIRT == null)
//            rightUIRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
//        leftFarRT.antiAliasing = 4;
//        rightFarRT.antiAliasing = 4;
//        leftNearRT.antiAliasing = 4;
//        rightNearRT.antiAliasing = 4;
//        leftUIRT.antiAliasing = 4;
//        rightUIRT.antiAliasing = 4;
//        leftFarCam.targetTexture = leftFarRT;
//        rightFarCam.targetTexture = rightFarRT;
//        leftNearCam.targetTexture = leftNearRT;
//        rightNearCam.targetTexture = rightNearRT;
//        leftUICam.targetTexture = leftUIRT;
//        rightUICam.targetTexture = rightUIRT;

//        stereoPass = new StereoRenderPass(this);

//    }

//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.L))
//        {
//            Debug.Log("L Pressed!");
//            Debug.Log("Stereo enabled = " + _camera.stereoEnabled);

//            var t = OpenVR.System.GetProjectionMatrix(EVREye.Eye_Left, 0.1f, 300f);
//            var string1 = t.m0 + " " + t.m1 + " " + t.m2 + " " + t.m3 + " " + t.m4 + " " + t.m5 + " " + t.m6 + " " + t.m7 + " " + t.m8 + " " + t.m9 + " " + t.m10 + " " + t.m11 + " " + t.m12 + " " + t.m13 + " " + t.m14 + " " + t.m15;
//            t = OpenVR.System.GetProjectionMatrix(EVREye.Eye_Right, 0.1f, 300f);
//            var string2 = t.m0 + " " + t.m1 + " " + t.m2 + " " + t.m3 + " " + t.m4 + " " + t.m5 + " " + t.m6 + " " + t.m7 + " " + t.m8 + " " + t.m9 + " " + t.m10 + " " + t.m11 + " " + t.m12 + " " + t.m13 + " " + t.m14 + " " + t.m15;


//            Debug.Log("Left projectionMatrix = " + _camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left));
//            Debug.Log("Right projectionMatrix = " + _camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right));
//            Debug.Log("Left projectionMatrixO = " + string1);
//            Debug.Log("Right projectionMatrixO = " + string2);
//        }
//    }

//    public ScriptableRenderPass GetPassToEnqueue(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorAttachmentHandle, RenderTargetHandle depthAttachmentHandle)
//    {
//        if (stereoPass == null)
//            stereoPass = new StereoRenderPass(this);
//        stereoPass.Setup(baseDescriptor, colorAttachmentHandle);
//        return stereoPass;
//    }

//    public void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
//    {
//        if (stereoPass.isRendering)
//            stereoPass.Execute(renderer, context, ref renderingData);
//    }

//    //因为IL2CPP的限制，没法直接继承ScriptableRenderPass，转为注入在管线中没有特别大作用的pass
//    //在StereoRenderPassPatches里将StartXRRenderingPass和EndXRRenderingPass都禁用了
//    public class StereoRenderPass : ScriptableRenderPass
//    {

//        private static readonly string k_RenderTag = "Render Stereo Texture";
//        //获取shader中的属性
//        private static readonly int TempTargetId = Shader.PropertyToID("_TempTargetVREye");
//        private StereoRender stereoRender;
//        private Material stereoMaterial;
//        public bool isRendering;
//        public float lastTime = 0;

//        RenderTextureDescriptor baseDescriptor;
//        RenderTargetHandle colorHandle;

//        public StereoRenderPass(StereoRender stereoRender)
//        {
//            this.stereoRender = stereoRender;

//            var shader = Shader.Find("PureDark/StereoRender");
//            if (shader == null)
//            {
//                Debug.LogError("Shader not found.");
//                return;
//            }
//            stereoMaterial = CoreUtils.CreateEngineMaterial(shader);
//            stereoMaterial.SetTexture("_LeftFirstTex", stereoRender.leftNearRT);
//            stereoMaterial.SetTexture("_RightFirstTex", stereoRender.rightNearRT);
//            stereoMaterial.SetTexture("_LeftLastTex", stereoRender.leftUIRT);
//            stereoMaterial.SetTexture("_RightLastTex", stereoRender.rightUIRT);
//            stereoMaterial.SetFloat("_AlphaMultiplier", 2);
//        }

//        //获得渲染目标的引用
//        public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorHandle)
//        {
//            this.baseDescriptor = baseDescriptor;
//            this.colorHandle = colorHandle;
//            isRendering = true;
//            // 因为Postprocessing一直会替换主相机的culling mask，要替换回0，不需要主相机渲染任何东西
//            stereoRender._camera.cullingMask = 0;
//        }

//        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
//        {
//            if (!stereoRender.enabled)
//                return;
//            if (stereoMaterial == null)
//            {
//                Debug.LogError("Material not created.");
//                return;
//            }

//            //获取命令缓冲区并为其分配名称
//            var cmd = CommandBufferPool.Get(k_RenderTag);
//            //定义渲染过程
//            Render(cmd);
//            //执行渲染命令
//            context.ExecuteCommandBuffer(cmd);
//            //释放该渲染命令缓冲区
//            CommandBufferPool.Release(cmd);

//            isRendering = false;
//        }

//        void Render(CommandBuffer cmd)
//        {
//            //获得屏幕RT
//            var source = colorHandle.Identifier();
//            int destination = TempTargetId;
//            //获取宽高
//            var w = baseDescriptor.width;
//            var h = baseDescriptor.height;
//            //使用临时纹理复制当前屏幕纹理
//            cmd.GetTemporaryRT(destination, w, h, 0, FilterMode.Point, RenderTextureFormat.Default);
//            cmd.Blit(source, destination);
//            //使用材质的0pass渲染到临时纹理并复制到屏幕纹理
//            cmd.Blit(destination, source, stereoMaterial, 0);
//        }
//    }

//}

