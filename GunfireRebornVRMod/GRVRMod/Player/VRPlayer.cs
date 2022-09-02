using HeroCameraName;
using InControl;
using System;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Valve.VR;
using VRMod.Patches;
using VRMod.Player.GameData;
using VRMod.Player.MotionControlls;
using VRMod.Player.VRInput;
using VRMod.UI.Pointers;
using static VRMod.Player.MotionControlls.HandController;
using static VRMod.VRMod;

namespace VRMod.Player
{
    public class VRPlayer : MonoBehaviour
    {
        public VRPlayer(IntPtr value) : base(value) { }


        public static VRPlayer Instance { get; private set; }
        public Transform Origin { get; private set; }
        public Transform Head { get; private set; }
        public Camera Camera { get; private set; }
        public Camera ScreenCam { get; private set; }
        public HandController LeftHand { get; private set; }
        public HandController RightHand { get; private set; }
        public StereoRender StereoRender { get; private set; }

        private Transform HeroSkillHand { get; set; }
        private Animator HeroSkillHandAnimator { get; set; }
        private Transform HeroWeaponHand { get; set; }
        private Animator HeroWeaponHandAnimator { get; set; }
        private Transform HeroLeftHand { get; set; }
        private Transform HeroRightHand { get; set; }
        private Transform HeroWeapon { get; set; }

        public bool isHome = false;

        private void Awake()
        {
            if (Instance)
            {
                Log.Error("Trying to create duplicate VRPlayer!");
                enabled = false;
                return;
            }
            Instance = this;
            isHome = true;

            SteamVR_Settings.instance.poseUpdateMode = SteamVR_UpdateModes.OnLateUpdate;
            SteamVR.Initialize(false);
            XRSettings.gameViewRenderMode = GameViewRenderMode.RightEye;

            HarmonyPatches.onSceneLoaded += OnSceneLoaded;

            // 新增一个新手柄，输入源是自己设定的SteamVR action
            FindObjectOfType<InControlManager>()?.gameObject.GetOrAddComponent<VRInputManager>();

            //初始化VR相机和左右手
            Head = transform.Find("PlayerCamera");
            if (Head)
            {
                Camera = Head.gameObject.GetComponent<Camera>();
                Camera.cullingMask = 0;
                Camera.depth = -1;
                StereoRender = Head.gameObject.AddComponent<StereoRender>();

                ScreenCam = Head.Find("ScreenCam")?.GetComponent<Camera>();

                LeftHand = transform.Find("LeftHand").gameObject.GetOrAddComponent<HandController>();
                RightHand = transform.Find("RightHand").gameObject.GetOrAddComponent<HandController>();
                LeftHand.Setup(HandType.Left);
                RightHand.Setup(HandType.Right);
            }

            Origin = transform.parent;
            SetOriginHome();

            CUIManager.instance.gameObject.transform.parent = Origin;
            var canvasRoot = CUIManager.instance.transform.Find("Canvas_PC(Clone)");
            canvasRoot.position = new Vector3(32.8f, 3.7f, 16f);
            canvasRoot.localEulerAngles = new Vector3(0, 0, 0);
            DontDestroyOnLoad(Origin);
        }

        public void ToggleEventCamera(bool force = false)
        {
            var eventSystem = GameObject.Find("UniverseLibCanvas");

            if (!eventSystem || !RightHand) return;

            var eventSysCom = eventSystem.GetComponent<EventSystem>();
            if (!eventSysCom)
            {
                eventSystem = GameObject.Find("DontDestroyRoot/EventSystem");
                eventSysCom = eventSystem.GetComponent<EventSystem>();
            }

            RightHand.SetupEventSystem(eventSysCom, eventSystem.GetComponent<StandaloneInputModule>());

            var eventCam = RightHand.eventCamera;

            var vrPointerInput = eventSystem.GetComponent<VRPointerInput>();
            if(vrPointerInput != null)
            {
                if(!force)
                    DestroyImmediate(vrPointerInput);
            }
            else
            {
                vrPointerInput = eventSystem.AddComponent<VRPointerInput>();
                vrPointerInput.eventCamera = eventCam;
                vrPointerInput.clikeButton = SteamVR_Actions.gameplay_RT_Fire_InteractUI;
            }
            var canvases = CUIManager.instance.gameObject.GetComponentsInChildren<Canvas>();
            foreach (var canvas in canvases)
            {
                canvas.worldCamera = eventCam;
            }
        }

        void FixedUpdate()
        {
            if (!isHome)
            {
                // 每秒转50度，太快再后期调整吧
                if (VRInputManager.Instance.vrDevice.RightStick.State)
                    Origin.Rotate(Vector3.up, VRInputManager.Instance.vrDevice.RightStick.X);
                if(VRInputManager.Instance.vrDevice.SnapTurnLeft().stateDown)
                    Origin.Rotate(Vector3.up, -60);
                if (VRInputManager.Instance.vrDevice.SnapTurnRight().stateDown)
                    Origin.Rotate(Vector3.up, 60);
            }
        }

        public int weaponSID = -1;
        public Vector3 offsetOverride = new Vector3(0, -0.08f, -0.15f);
        private NewPlayerObject heroObj;

        private int lastWeaponSID;
        private bool isLeftHandEnabled = false;
        private bool isRightHandEnabled = false;
        private Transform Muzzle;

        // 在LateUpdate里才能覆盖英雄身体的位置和朝向
        void LateUpdate()
        {
            if (Input.GetKeyUp(KeyCode.J))
            {
                ToggleEventCamera();
            }
            if (Input.GetKeyUp(KeyCode.KeypadPlus))
            {
                offsetOverride = new Vector3(offsetOverride.x, offsetOverride.y+0.01f, offsetOverride.z);
            }
            if (Input.GetKeyUp(KeyCode.KeypadMinus))
            {
                offsetOverride = new Vector3(offsetOverride.x, offsetOverride.y-0.01f, offsetOverride.z);
            }
            if (Input.GetKeyUp(KeyCode.KeypadMultiply))
            {
                offsetOverride = new Vector3(offsetOverride.x, offsetOverride.y, offsetOverride.z + 0.01f);
            }
            if (Input.GetKeyUp(KeyCode.KeypadDivide))
            {
                offsetOverride = new Vector3(offsetOverride.x, offsetOverride.y, offsetOverride.z - 0.01f);
            }
            if (Input.GetKeyUp(KeyCode.Keypad4))
            {
                offsetOverride = new Vector3(offsetOverride.x - 0.01f, offsetOverride.y, offsetOverride.z);
            }
            if (Input.GetKeyUp(KeyCode.Keypad6))
            {
                offsetOverride = new Vector3(offsetOverride.x + 0.01f, offsetOverride.y, offsetOverride.z);
            }
            if (!isHome && HeroCameraManager.HeroObj != null && HeroCameraManager.HeroTran != null)
            {
                // 将游玩空间的位置跟英雄的位置同步
                Origin.position = HeroCameraManager.HeroTran.position;

                // 强制英雄的朝向与玩家头部朝向同步
                HeroCameraManager.HeroTran.rotation = Quaternion.LookRotation(GetFlatForwardDirection());

                // 不放技能时默认隐藏技能手
                //HeroSkillHand.gameObject.active = HeroSkillHandAnimator.IsPlaying();
                //HeroWeaponHand.gameObject.active = HeroWeaponHandAnimator.IsPlaying();

                // 默认大小在第一人称下看着太大了，需要缩小一点
                CameraManager.MainCamera.localScale = new Vector3(0.67f, 0.67f, 0.67f);

                // 强制游戏自带的手和武器模型与玩家手同步
                var currWeapon = HeroCameraManager.HeroObj.PlayerCom.GetCurWeapon(HeroCameraManager.HeroObj.PlayerCom.CurWeaponID);
                heroObj = HeroCameraManager.HeroObj;
                if (currWeapon != null)
                {
                    if (currWeapon.MainWeapon != null)
                    {
                        weaponSID = HeroCameraManager.HeroObj.PlayerCom.CurWeaponSID;
                        if (weaponSID != lastWeaponSID)
                        {
                            lastWeaponSID = weaponSID;
                            HeroLeftHand = currWeapon.MainWeapon.Find("Home/hero_fpp_101_A_L");
                            HeroRightHand = currWeapon.MainWeapon.Find("Home/hero_fpp_101_A_R");
                            Muzzle = currWeapon.MainWeapon.Find("muzzle");
                            HeroWeapon = currWeapon.MainWeapon.Find("Home/weapon_"+ weaponSID);
                            if(!HeroWeapon)
                                HeroWeapon = currWeapon.MainWeapon.Find("Home/weapon_pistol_1");
                            isLeftHandEnabled = HeroLeftHand.gameObject.activeSelf;
                            isRightHandEnabled = HeroRightHand.gameObject.activeSelf;
                        }
                        var weaponData = WeaponDatas.GetWeaponData(weaponSID);
                        AttachWeaponToHand(currWeapon.MainWeapon, weaponData);
                    }
                }
                bool isLeftHandAim = VRInputManager.Instance.vrDevice.RB_SecondarySkill().state || VRInputManager.Instance.vrDevice.LB_PrimarySkill().state
                    || VRInputManager.Instance.vrDevice.RB_SecondarySkill().stateUp || VRInputManager.Instance.vrDevice.LB_PrimarySkill().stateUp;

                // 瞄准当前手柄指向的位置
                var hand = isLeftHandAim ? LeftHand : RightHand;
                CameraManager.MainCamera.position = Head.position;
                Vector3 dir = hand.GetRayHitPosition() - CameraManager.MainCamera.position;
                CameraManager.MainCamera.rotation = Quaternion.LookRotation(dir);
            }
        }

        void AttachWeaponToHand(Transform weapon, WeaponDatas.WeaponData weaponData)
        {
            bool twohanded = false;
            if (weaponData.isTwoHanded)
            {
                var guidingVector = LeftHand.model.position - RightHand.model.position;
                float angleBetweenHands = Vector3.Angle(RightHand.model.forward, guidingVector);

                if (angleBetweenHands <= 45f)
                {
                    var offset = weaponData.offset;
                    if (offset == Vector3.zero)
                        offset = offsetOverride;
                    var rot = Quaternion.LookRotation(guidingVector, RightHand.model.up);
                    RightHand.muzzle.rotation = rot;
                    weapon.rotation = rot;
                    if (weaponData.mainHand == HandType.Left)
                        weapon.position = LeftHand.model.position + weapon.right * offset.x + weapon.up * offset.y + weapon.forward * (offset.z + 0.1f);
                    else
                        weapon.position = RightHand.model.position + weapon.right * offset.x + weapon.up * offset.y + weapon.forward * offset.z;
                    if (HeroLeftHand != null)
                        HeroLeftHand.gameObject.active = isLeftHandEnabled;
                    twohanded = true;
                }
            }

            if(!twohanded)
            {
                var offset = weaponData.offset;
                if (offset == Vector3.zero)
                    offset = offsetOverride;
                RightHand.muzzle.localEulerAngles = new Vector3(36, 0, 0);
                weapon.rotation = RightHand.model.rotation;
                weapon.position = RightHand.model.position + weapon.right * offset.x + weapon.up * offset.y + weapon.forward * offset.z;
                if (HeroLeftHand != null)
                    HeroLeftHand.gameObject.active = false;
            }

            if (HeroRightHand != null)
                HeroRightHand.gameObject.active = isRightHandEnabled;
            if (HeroWeapon != null)
                HeroWeapon.gameObject.active = true;

            if (Muzzle != null)
                Muzzle.position -= Muzzle.right * 0.7f;
        }

        void SetUIMode(bool uiMode)
        {
            LeftHand.uiMode = uiMode;
            RightHand.uiMode = uiMode;
            LeftHand.model.gameObject.active = uiMode;
            RightHand.model.gameObject.active = uiMode;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if(scene.name?.ToLower() == "home")
            {
                SetOriginHome();
                isHome = true;
                SetUIMode(true);
            }
            else
            {
                isHome = false;
                SetUIMode(false);
            }

            if(RegexManager.IsNumber(scene.name))
            {
                MelonCoroutines.Start(BattlePrep());
                SetUIMode(false);
            }
            ToggleEventCamera(true);
        }

        IEnumerator BattlePrep()
        {
            while (HeroCameraManager.HeroObj == null)
                yield return new WaitForSeconds(0.1f);

            // UI
            Camera[] cams = FindObjectsOfType<Camera>();
            foreach (Camera c in cams)
            {
                c.stereoTargetEye = StereoTargetEyeMask.None;
            }
            Camera.stereoTargetEye = StereoTargetEyeMask.Both;
            var vrBattleUI = Head.gameObject.GetOrAddComponent<VRBattleUI>();
            vrBattleUI.Setup();
            Origin.position = HeroCameraManager.HeroTran.position;

            CameraManager.MainCamera.GetComponent<FPSCamMotionCtrl>().enabled = false;
            cams = CameraManager.MainCamera.GetComponentsInChildren<Camera>();
            foreach (Camera c in cams)
            {
                c.enabled = false;
            }

            // 储存技能手Transform的引用
            foreach (var child in CameraManager.MainCamera)
            {
                if (RegexManager.IsNumber(child.Cast<Transform>().gameObject.name))
                {
                    HeroSkillHand = child.Cast<Transform>();
                    HeroSkillHandAnimator = HeroSkillHand.GetComponent<Animator>();
                } 
                else if (child.Cast<Transform>().gameObject.name.Contains("_WeaponHands"))
                {
                    HeroWeaponHand = child.Cast<Transform>();
                    HeroWeaponHandAnimator = HeroWeaponHand.GetComponent<Animator>();
                }
            }
        }

        private void OnDestroy()
        {
            HarmonyPatches.onSceneLoaded -= OnSceneLoaded;
        }

        private void SetOriginHome()
        {
            SetOriginPosRotScl(new Vector3(28f, 1.6f, 16f), new Vector3(0, 90, 0), new Vector3(1, 1, 1));
        }

        public void SetOriginPosRotScl(Vector3 pos, Vector3 euler, Vector3 scale)
        {
            Origin.position = pos;
            Origin.localEulerAngles = euler;
            Origin.localScale = scale;
        }

        public void SetOriginScale(float scale)
        {
            Origin.localScale = new Vector3(scale, scale, scale);
        }

        public Vector3 GetWorldForward()
        {
            return Head.forward;
        }

        public Vector3 GetFlatForwardDirection()
        {
            Vector3 dir = Head.forward;
            dir.y = 0;
            return dir.normalized;
        }

        public float GetPlayerHeight()
        {
            if (!Head)
            {
                return 1.8f;
            }
            return Head.localPosition.y;
        }
    }
}
