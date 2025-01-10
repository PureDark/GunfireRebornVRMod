using System;
using UI;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using VRMod.Assets;
using VRMod.Player.VRInput;
using static VRMod.Player.GameData.WeaponDatas;

namespace VRMod.Player.Behaviours
{
    public class VRScope : MonoBehaviour
    {
        public VRScope(IntPtr value) : base(value) { }
        public GameObject circleScope;
        public RifleWeaponData weaponData;

        private float scaleLerp = 0f;

        public bool IsScoping { get; private set; }
        public bool IsShowing { get; private set; }

        public void Setup(RifleWeaponData weaponData)
        {
            this.weaponData = weaponData;
            if (!circleScope)
                circleScope = Instantiate(VRAssets.CircleScope);
            circleScope.transform.parent = transform;
            circleScope.transform.localPosition = weaponData.scopePos;
            circleScope.transform.localEulerAngles = weaponData.scopeEuler;
            circleScope.transform.localScale = weaponData.scopeScale;
            circleScope.SetLayerRecursively(Layer.UI);
            SetIsScoping(true);
        }

        public void LateUpdate()
        {
            if (!circleScope || weaponData == null)
                return;

            if(VRPlayer.Instance.isUIMode && IsScoping)
            {
                SetIsScoping(false);
            }
            else if(!VRPlayer.Instance.isUIMode && !IsScoping)
            {
                SetIsScoping(true);
            }

            if (IsScoping)
            {
                circleScope.transform.rotation = Quaternion.LookRotation(circleScope.transform.forward, Vector3.up);
                VRPlayer.Instance.RightHand.eventCamera.transform.rotation = Quaternion.LookRotation(VRPlayer.Instance.RightHand.eventCamera.transform.forward, Vector3.up);
            }

            IsShowing = VRInputManager.Instance.vrDevice.LT_WeaponSkill.axis > 0.9f && !VRPlayer.Instance.isUIMode;
            if (scaleLerp != (IsShowing ? 1f : 0f))
                scaleLerp = Mathf.Clamp(scaleLerp + (IsShowing ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime) * 6, 0f, 1f);
            circleScope.transform.localScale = Vector3.Lerp(Vector3.zero, weaponData.scopeScale, scaleLerp);
        }

        public void OnEnable()
        {
            SetIsScoping(true);
        }

        public void OnDisable()
        {
            SetIsScoping(false);
        }

        public void SetIsScoping(bool isScoping)
        {
            if (isScoping && circleScope)
            {
                VRPlayer.Instance.RightHand.eventCamera.stereoTargetEye = StereoTargetEyeMask.None;
                //VRPlayer.Instance.RightHand.eventCamera.fieldOfView = Utils.CalculateZoomFOV(SteamVR.instance.fieldOfView, 20);
                VRPlayer.Instance.RightHand.eventCamera.fieldOfView = 7;
                VRPlayer.Instance.RightHand.eventCamera.cullingMask = -966787561;
                VRPlayer.Instance.RightHand.eventCamera.targetTexture = circleScope.GetComponentInChildren<MeshRenderer>().material.mainTexture.Cast<RenderTexture>();
                VRPlayer.Instance.RightHand.eventCamera.enabled = true;
                VRPlayer.Instance.RightHand.eventCamera.gameObject.GetOrAddComponent<OutLinePassMgr>();

                var UICamera = CUIManager.instance.UIRoot.Find("Camera");
                var uiPPLayer = UICamera?.GetComponent<PostProcessLayer>();
                if(uiPPLayer != null)
                {
                    var mPostProcessLayer = VRPlayer.Instance.RightHand.eventCamera.gameObject.GetOrAddComponent<PostProcessLayer>();
                    mPostProcessLayer.m_Resources = uiPPLayer.m_Resources;
                    mPostProcessLayer.volumeLayer = uiPPLayer.volumeLayer;
                }
            }
            else
            {
                VRPlayer.Instance.RightHand.eventCamera.stereoTargetEye = StereoTargetEyeMask.None;
                VRPlayer.Instance.RightHand.eventCamera.fieldOfView = 60;
                VRPlayer.Instance.RightHand.eventCamera.cullingMask = 1<<Layer.UI;
                VRPlayer.Instance.RightHand.eventCamera.targetTexture = null;
                VRPlayer.Instance.RightHand.eventCamera.enabled = false;
                VRPlayer.Instance.RightHand.eventCamera.transform.localRotation = Quaternion.identity;
            }
            this.IsScoping = isScoping;
        }


    }
}
