using DYPublic.GamePlatform;
using DYPublic.UI;
using HeroCameraName;
using InControl;
using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using Valve.VR;
using VRMod.Patches;
using VRMod.Player.Behaviours;
using VRMod.Player.GameData;
using VRMod.Player.MotionControlls;
using VRMod.Player.VRInput;
using VRMod.Settings;
using VRMod.UI;
using VRMod.UI.Pointers;
using static UnityEngine.UI.Image;
using static VRMod.Player.MotionControlls.HandController;
using static VRMod.VRMod;
using Mathf = Valve.VR.Mathf;

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

        public bool isHome = false;
        public bool isHomeFixed = false;
        public bool isUIMode = false;
        public bool CannonFix = false;
        public Transform effectRoot;

        public void Awake()
        {
            if (Instance)
            {
                Log.Error("Trying to create duplicate VRPlayer!");
                enabled = false;
                return;
            }
            Instance = this;
            isHome = true;
            //SteamVR_Settings.instance.poseUpdateMode = SteamVR_UpdateModes.OnLateUpdate;
            //SteamVR.Initialize(false);

            HarmonyPatches.onSceneLoaded += OnSceneLoaded;
            if (VRMod.IsVR)
            {
                // 新增一个新手柄，输入源是自己设定的SteamVR action
                FindObjectOfType<InControlManager>()?.gameObject.GetOrAddComponent<VRInputManager>();
            }

            // 初始化游玩空间
            //gameObject.AddComponent<SteamVR_PlayArea>().drawInGame = false;

            // 初始化VR相机和左右手
            Head = transform.Find("PlayerCamera");
            if (Head)
            {
                Camera = Head.gameObject.GetComponent<Camera>();
                Camera.cullingMask = 0;
                Camera.depth = -1;
                StereoRender = Head.gameObject.AddComponent<StereoRender>();

                ScreenCam = Head.Find("ScreenCam")?.GetComponent<Camera>();
                ScreenCam.clearFlags = CameraClearFlags.SolidColor;
                ScreenCam.backgroundColor = new Color(0, 0, 0, 0);
                ScreenCam.enabled = ModConfig.UseFirstPersonCam.Value;
                ScreenCam.cullingMask = ScreenCam.enabled? -1 : 0;
                ScreenCam.depth = 1;
                ScreenCam.transform.SetParent(null);
                var smoother = ScreenCam.gameObject.AddComponent<CameraSmoother>();
                smoother.smoothTime = ModConfig.FPCamSmoothTime.Value;
                smoother.enableRotationSmoothing = true;
                smoother.target = Head;

                LeftHand = transform.Find("LeftHand").gameObject.AddComponent<HandController>();
                RightHand = transform.Find("RightHand").gameObject.AddComponent<HandController>();
                LeftHand.Setup(HandType.Left);
                RightHand.Setup(HandType.Right);
            }

            Origin = transform.parent;
            SetOriginHome();
            SetupHome();

            CUIManager.instance.gameObject.transform.parent = Origin;
            var canvasRoot = CUIManager.instance.transform.Find("Canvas_PC(Clone)");
            canvasRoot.position = new Vector3(32.8f, 3.7f, 16f);
            canvasRoot.localEulerAngles = new Vector3(0, 0, 0);
            DontDestroyOnLoad(Origin);
        }

        public void ToggleEventCamera(bool force = false)
        {
            if (!VRMod.IsVR)
                return;
            var eventSystemGO = GameObject.Find("UniverseLibCanvas");
            if(!eventSystemGO)
                eventSystemGO = GameObject.Find("DontDestroyRoot/EventSystem");

            if (!eventSystemGO || !RightHand) return;

            var eventSystem = eventSystemGO.GetComponent<EventSystem>();
            if (!eventSystem)
            {
                eventSystemGO = GameObject.Find("DontDestroyRoot/EventSystem");
                eventSystem = eventSystem.GetComponent<EventSystem>();
            }

            RightHand.SetupEventSystem(eventSystem, eventSystemGO.GetComponent<StandaloneInputModule>());

            var eventCam = RightHand.eventCamera;

            var vrPointerInput = eventSystemGO.GetComponent<VRPointerInput>();
            if (vrPointerInput != null)
            {
                if (!force)
                    DestroyImmediate(vrPointerInput);
            }
            else
            {
                vrPointerInput = eventSystemGO.AddComponent<VRPointerInput>();
                vrPointerInput.eventCamera = eventCam;
                vrPointerInput.clikeButton = SteamVR_Actions.gameplay_InteractUI;
            }
            var canvases = CUIManager.instance.gameObject.GetComponentsInChildren<Canvas>();
            foreach (var canvas in canvases)
            {
                canvas.worldCamera = eventCam;
            }
        }




        #region Motion Controls

        public Vector3 offsetOverride = Vector3.zero;
        public Vector3 rotationEulerOverride = Vector3.zero;
        public List<BezierLineRenderer> bezierLineRenderers = new List<BezierLineRenderer>();
        public List<RayLineLaser> rayLineLasers = new List<RayLineLaser>();
        public bool allowUpdateRayLineLaser = false;

        public VRBattleUI vrBattleUI;
        public bool IsReadyForBattle;

        private int lastWeaponID;
        private float aimSwitchDelay = 1f;

        private ETCTouchPad etcTouchPad;
        private ETCTransformCtrl etcTransformCtrl_Hero;
        private ETCTransformCtrl etcTransformCtrl_FPSCam;

        private FPSCamMotionCtrl fpsCamMotionCtrl;
        private Transform HeroSkillHand;
        private Animator HeroSkillHandAnimator;
        private Transform HeroWeaponHand;
        private Animator HeroWeaponHandAnimator;
        private Transform WeaponLeftHand;
        private Transform WeaponRightHand;
        private Animator WeaponAnimator;
        private Transform Muzzle;

        // 猫头鹰用主动技能时，技能手有个游离的右手需要隐藏
        private Transform HeroSkillHandHide;

        // 扔手雷时需要隐藏的如律令模型
        private Transform TalismanLeft;
        private Transform TalismanAmuletLeft;

        // 左手的模型，直接从技能手复制一份
        private Transform LeftHandMesh;

        private bool isInIdleState = false;
        private bool isEnteringPrimarySkillState = false;
        private bool isInPrimarySkillState = false;
        private bool isExitingPrimarySkillState = false;
        private bool isTwoHanded = false;

        //松鼠的主动技能进入第三人称

        private Transform OriginTarget;
        private Transform TPSCamBoom;
        private Transform TPSCam;
        private bool isInThirdPersonLastFrame = false;
        private bool isInThirdPerson = false;
        private float tempAngleTPS;

        //CG场景
        private bool isInCG = false;
        private Transform CGCamera;
        private float tempAngleCG;

        private Vector3 velocity = Vector3.zero;
        private Quaternion deriv = Quaternion.identity;
        private VRScope vrScope;

        public bool isDualWield { get { return dualWieldAKGameObj? dualWieldAKGameObj.enabled : false; } }

        private AkGameObj dualWieldAKGameObj;

        public bool isParentOverride = true;

        //public Dictionary<int, float> states = new Dictionary<int, float>();

        public void SetDualWield(Transform dualWieldTrans)
        {
            this.dualWieldAKGameObj = dualWieldTrans.GetComponent<AkGameObj>();
            vrBattleUI.UpdateCrossHair();
            VRInputManager.Instance.vrDevice.dualWieldDelay = 0.5f;
        }

        public void FixCannonBall()
        {
            if (effectRoot == null)
                effectRoot = GameObject.Find("UIEffectRoot").transform;
            if (effectRoot != null)
            {
                var effectLayer = effectRoot.Find("EffectLayer");
                if (effectLayer != null)
                {
                    foreach (var child in effectLayer)
                    {
                        var effect = child.Cast<Transform>().Find("bg_gren/Gren_effect/postion_01");
                        if (effect != null)
                        {
                            effect.gameObject.active = false;
                        }
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            if (isHome && !isHomeFixed)
            {
                if (!GameObject.Find("CamPoint_Camera"))
                    return;
                ScreenCam.gameObject.active = true;
                //ScreenCam.enabled = true;
                MenuFix.HomeFix();
                ToggleEventCamera(true);
                SetUIMode(true);
                isHomeFixed = true;
            }
        }

        // 在LateUpdate里才能覆盖英雄身体的位置和朝向
        public void LateUpdate()
        {
            if (VRMod.IsVR && !isHome && !isUIMode && !isInCG)
            {
                if (!isInThirdPerson)
                {
                    if (VRInputManager.Device.RightStick.State)
                        Origin.Rotate(Vector3.up, VRInputManager.Device.RightStick.X * Time.deltaTime * 50 * ModConfig.SmoothTurningSpeed.Value);
                    else
                    if (VRInputManager.Device.SnapTurnLeft.stateUp)
                        Origin.Rotate(Vector3.up, ModConfig.SnapTurningAngle.Value * -1);
                    else if (VRInputManager.Device.SnapTurnRight.stateUp)
                        Origin.Rotate(Vector3.up, ModConfig.SnapTurningAngle.Value);

                }
            }

            if (Input.GetKeyUp(KeyCode.LeftAlt))
            {
                if(Time.timeScale > 0.5f)
                    Time.timeScale = 0.1f;
                else if(Time.timeScale > 0.05f)
                    Time.timeScale = 0.01f;
                else
                    Time.timeScale = 1.0f;
            }
            if (Input.GetKeyUp(KeyCode.P))
            {
                ScreenCam.enabled = !ScreenCam.enabled;
                ScreenCam.cullingMask = (ScreenCam.enabled) ? -1 : 0;
                MenuFix.GetUICamera().enabled = !ScreenCam.enabled;
            }
            if (ModConfig.EnableDebugMode.Value)
            {
                if (Input.GetKeyUp(KeyCode.J))
                {
                    ToggleEventCamera();
                }
                if (Input.GetKeyUp(KeyCode.KeypadPlus))
                {
                    offsetOverride = new Vector3(offsetOverride.x, offsetOverride.y + 0.01f, offsetOverride.z);
                }
                if (Input.GetKeyUp(KeyCode.KeypadMinus))
                {
                    offsetOverride = new Vector3(offsetOverride.x, offsetOverride.y - 0.01f, offsetOverride.z);
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
                if (Input.GetKeyUp(KeyCode.Keypad7))
                {
                    rotationEulerOverride = new Vector3(rotationEulerOverride.x + 0.5f, rotationEulerOverride.y, rotationEulerOverride.z);
                }
                if (Input.GetKeyUp(KeyCode.Keypad8))
                {
                    rotationEulerOverride = new Vector3(rotationEulerOverride.x, rotationEulerOverride.y + 0.5f, rotationEulerOverride.z);
                }
                if (Input.GetKeyUp(KeyCode.Keypad9))
                {
                    rotationEulerOverride = new Vector3(rotationEulerOverride.x, rotationEulerOverride.y, rotationEulerOverride.z + 0.5f);
                }
                if (Input.GetKeyUp(KeyCode.Keypad1))
                {
                    rotationEulerOverride = new Vector3(rotationEulerOverride.x - 0.5f, rotationEulerOverride.y, rotationEulerOverride.z);
                }
                if (Input.GetKeyUp(KeyCode.Keypad2))
                {
                    rotationEulerOverride = new Vector3(rotationEulerOverride.x, rotationEulerOverride.y - 0.5f, rotationEulerOverride.z);
                }
                if (Input.GetKeyUp(KeyCode.Keypad3))
                {
                    rotationEulerOverride = new Vector3(rotationEulerOverride.x, rotationEulerOverride.y, rotationEulerOverride.z - 0.5f);
                }
                if (Input.GetKeyUp(KeyCode.PageUp))
                {
                    ModConfig.PlayerWorldScale.Value += 0.01f;
                }
                if (Input.GetKeyUp(KeyCode.PageDown))
                {
                    ModConfig.PlayerWorldScale.Value -= 0.01f;
                }
            }
            if (CannonFix)
            {
                FixCannonBall();
            }

            if (isInCG)
            {
                Origin.position = CGCamera.position - Vector3.up * GetPlayerHeight();
                Origin.rotation = Quaternion.LookRotation(CGCamera.forward.GetFlatDirection(), Vector3.up);
                Origin.rotation = Quaternion.Euler(0, tempAngleCG, 0) * Origin.rotation;
                if (LeftHandMesh)
                    LeftHandMesh.gameObject.active = false;
            }
            else if (!isUIMode && !isHome && IsReadyForBattle && HeroCameraManager.HeroTran)
            {
                // 时刻检查并禁用FPSCam的镜头控制脚本
                fpsCamMotionCtrl.enabled = false;

                // 默认大小在第一人称下看着太大了，需要缩小一点
                CameraManager.MainCamera.localScale = new Vector3(0.67f, 0.67f, 0.67f);
                Origin.localScale = Vector3.one * ModConfig.PlayerWorldScale.Value;
                LeftHand.model.localScale = Vector3.one / ModConfig.PlayerWorldScale.Value;
                RightHand.model.localScale = Vector3.one / ModConfig.PlayerWorldScale.Value;

                isInThirdPerson = TPSCamBoom ? TPSCamBoom.gameObject.active : false;

                // 检测是否使用了松鼠的主动技能，进入第三人称
                if(isInThirdPersonLastFrame != isInThirdPerson)
                {
                    isInThirdPersonLastFrame = isInThirdPerson;
                    if (isInThirdPerson)
                    {
                        tempAngleTPS = Vector3.Angle(Head.forward.GetFlatDirection(), Origin.forward.GetFlatDirection());
                        var smoother = Origin.gameObject.GetOrAddComponent<CameraSmoother>();
                        smoother.target = OriginTarget;
                        smoother.enableRotationSmoothing = true;
                        smoother.enablePositionSmoothing = true;
                        smoother.smoothTime = 0.15f;
                        smoother.enabled = true;
                    }
                    else
                    {
                        var smoother = Origin.gameObject.GetOrAddComponent<CameraSmoother>();
                        smoother.target = null;
                        smoother.enabled = false;
                    }
                }

                if (isInThirdPerson)
                {
                    // 将游玩空间的位置跟第三人称相机的位置同步
                    var targetPosition = TPSCam.position + TPSCam.forward.normalized * Vector3.Distance(TPSCam.position, HeroCameraManager.HeroTran.position) * 0.5f;
                    OriginTarget.position = targetPosition - Vector3.up * GetPlayerHeight();
                    OriginTarget.rotation = Quaternion.LookRotation(TPSCam.forward.GetFlatDirection(), Vector3.up);
                    OriginTarget.rotation = Quaternion.Euler(0, tempAngleTPS, 0) * OriginTarget.rotation;
                }
                else
                {
                    // 将游玩空间的位置跟英雄的位置同步
                    Origin.position = HeroCameraManager.HeroTran.position;

                    // 强制英雄的朝向与玩家头部朝向同步
                    var headRotation = Quaternion.LookRotation(GetFlatForwardDirection());
                    HeroCameraManager.HeroTran.rotation = headRotation;
                    etcTouchPad.axisX.directTransform.rotation = headRotation;
                    etcTouchPad.axisY.directTransform.rotation = headRotation;
                }

                var heroData = HeroDatas.GetHeroData(HeroCameraManager.HeroObj.PlayerCom.SID);

                var hand = RightHand;

                // 左手用技能时转为左手瞄准，松开后延迟0.5秒变回右手
                bool isSkillButtonPressed = VRInputManager.Device.RB_SecondarySkill.state || VRInputManager.Device.LB_PrimarySkill.state;

                bool isSecondarySkill = HeroSkillHandAnimator.IsPlaying(heroData.secondarySkillHash);

                bool isPrimarySkill = false;
                isInIdleState = HeroSkillHandAnimator.IsInState(heroData.idleHash);
                isEnteringPrimarySkillState = heroData.primarySkillEnterHash != 0 && HeroSkillHandAnimator.IsPlaying(heroData.primarySkillEnterHash);
                isExitingPrimarySkillState = heroData.primarySkillExitHash != 0 && HeroSkillHandAnimator.IsPlaying(heroData.primarySkillExitHash);
                if (heroData.primarySkillHash != 0)
                    isInPrimarySkillState = HeroSkillHandAnimator.IsInState(heroData.primarySkillHash);
                else
                {
                    if (isEnteringPrimarySkillState)
                        isInPrimarySkillState = true;
                    if (isInPrimarySkillState && isInIdleState)
                        isInPrimarySkillState = false;
                }

                if (isEnteringPrimarySkillState || isInPrimarySkillState || isExitingPrimarySkillState)
                    isPrimarySkill = true;

                if (isSkillButtonPressed)
                    aimSwitchDelay = 0.5f;
                if (aimSwitchDelay > 0 || isSecondarySkill || isPrimarySkill)
                {
                    aimSwitchDelay -= 0.013f;
                    hand = LeftHand;
                }

                // 瞄准当前手柄指向的位置
                CameraManager.MainCamera.position = Head.position;
                Vector3 dir = hand.GetRayHitPosition() - CameraManager.MainCamera.position;
                CameraManager.MainCamera.rotation = Quaternion.LookRotation(dir);

                bool hideIdleLeftHand = false;
                // 不放技能时默认隐藏技能手
                if (HeroSkillHand)
                {
                    bool isPlaying = aimSwitchDelay > 0 || isSecondarySkill || isPrimarySkill;
                    HeroSkillHand.localScale = isPlaying ? Vector3.one : Vector3.zero;
                    hideIdleLeftHand = isPlaying;
                }
                if (HeroWeaponHand)
                {
                    bool isPlaying = HeroWeaponHandAnimator.IsPlaying() || isSecondarySkill || isPrimarySkill;
                    HeroWeaponHand.localScale = HeroWeaponHandAnimator.IsPlaying() ? Vector3.one : Vector3.zero;
                    hideIdleLeftHand |= isPlaying;
                }

                // Only for debugging
                //if (WeaponAnimator)
                //{
                //    var state = WeaponAnimator.GetCurrentAnimatorStateInfo(0);
                //    if (states.ContainsKey(state.fullPathHash))
                //        states[state.fullPathHash] = WeaponAnimator.GetCurrentAnimatorStateInfo(0).m_NormalizedTime;
                //    else
                //        states.Add(state.fullPathHash, WeaponAnimator.GetCurrentAnimatorStateInfo(0).m_NormalizedTime);
                //}

                var currWeapon = HeroCameraManager.HeroObj.PlayerCom.GetCurWeapon(HeroCameraManager.HeroObj.PlayerCom.CurWeaponID);
                
                var weaponIsShow = true;
                switch (heroData.sid)
                {
                    case 206:
                    case 207:
                    case 213:
                    case 215:
                        // 鸟、老虎、龟龟、狐狸
                        // 放主要技能时隐藏武器双手
                        if (currWeapon != null)
                        {
                            if (isPrimarySkill)
                            {
                                weaponIsShow = false;
                                currWeapon.MainWeapon.localScale = Vector3.zero;
                                if (heroData.sid == 215 && !vrBattleUI.foxChargingAim)
                                    vrBattleUI.foxChargingAim = GameObject.Find("hub_for_fireball(Clone)")?.transform;
                            }
                            else
                                currWeapon.MainWeapon.localScale = Vector3.one;
                        }
                        break;
                    case 216:
                        if (!HeroSkillHandHide)
                            HeroSkillHandHide = HeroSkillHand.Find("113_A_L");
                        if (HeroSkillHandHide)
                            HeroSkillHandHide.gameObject.active = false;
                        break;
                    case 217:
                        if (!HeroSkillHandHide)
                            HeroSkillHandHide = HeroSkillHand.Find("hero_fpp_114_A_R");
                        if (HeroSkillHandHide)
                            HeroSkillHandHide.gameObject.active = false;
                        break;
                    case 219:
                        // 松鼠
                        // 放主要技能时进入第三人称模式，隐藏双手
                        if (TPSCamBoom != null && isInThirdPerson)
                        {
                            weaponIsShow = false;
                            hideIdleLeftHand = true;
                            currWeapon.MainWeapon.localScale = Vector3.zero;
                        }
                        break;
                }

                // 强制游戏自带的手和武器模型与玩家手同步
                if (currWeapon != null)
                {
                    var weaponSID = HeroCameraManager.HeroObj.PlayerCom.CurWeaponSID;
                    var weaponData = WeaponDatas.GetWeaponData(weaponSID);
                    if (ModConfig.EnableDebugMode.Value)
                    {
                        if (Input.GetKeyUp(KeyCode.Keypad5))
                        {
                            if (isParentOverride)
                            {
                                weaponData.parentOffset = offsetOverride;
                                weaponData.parentRotationEuler = rotationEulerOverride;
                                offsetOverride = weaponData.modelOffset;
                                rotationEulerOverride = weaponData.modelRotationEuler;
                            }
                            else
                            {
                                weaponData.modelOffset = offsetOverride;
                                weaponData.modelRotationEuler = rotationEulerOverride;
                                offsetOverride = weaponData.parentOffset;
                                rotationEulerOverride = weaponData.parentRotationEuler;
                            }
                            WeaponDatas.SetWeaponData(weaponData.id, weaponData);
                            isParentOverride = !isParentOverride;
                        }
                        if (Input.GetKeyUp(KeyCode.Keypad0))
                        {
                            if (isParentOverride)
                            {
                                weaponData.parentOffset = offsetOverride;
                                weaponData.parentRotationEuler = rotationEulerOverride;
                            }
                            else
                            {
                                weaponData.modelOffset = offsetOverride;
                                weaponData.modelRotationEuler = rotationEulerOverride;
                            }
                            WeaponDatas.SetWeaponData(weaponData.id, weaponData);
                            Log.Message("  Weapon id: " + weaponData.id);
                            Log.Message("  Weapon: " + weaponData.name);
                            Log.Message("  parentOffset: (" + weaponData.parentOffset.x.ToString("0.00") + "f, " + weaponData.parentOffset.y.ToString("0.00") + "f, " + weaponData.parentOffset.z.ToString("0.00") + "f)");
                            Log.Message("  modelOffset: (" + weaponData.modelOffset.x.ToString("0.00") + "f, " + weaponData.modelOffset.y.ToString("0.00") + "f, " + weaponData.modelOffset.z.ToString("0.00") + "f)");
                            Log.Message("  parentRotationEuler: (" + weaponData.parentRotationEuler.x.ToString("0.00") + "f, " + weaponData.parentRotationEuler.y.ToString("0.00") + "f, " + weaponData.parentRotationEuler.z.ToString("0.00") + "f)");
                            Log.Message("  modelRotationEuler: (" + weaponData.modelRotationEuler.x.ToString("0.00") + "f, " + weaponData.modelRotationEuler.y.ToString("0.00") + "f, " + weaponData.modelRotationEuler.z.ToString("0.00") + "f)");
                            Log.Message("  useParentTrans: " + isParentOverride);
                        }
                    }


                    // 如果使用如律令要把左手柄的空手隐藏
                    hideIdleLeftHand |= weaponData.weaponType == WeaponDatas.WeaponType.Talisman;

                    if (LeftHandMesh)
                    {
                        LeftHandMesh.localScale = hideIdleLeftHand || isDualWield ? Vector3.zero : new Vector3(0.67f, 0.67f, 0.67f);
                    }


                    // 切枪时初始化枪械相关引用
                    if (HeroCameraManager.HeroObj.PlayerCom.CurWeaponID != lastWeaponID)
                    {
                        lastWeaponID = HeroCameraManager.HeroObj.PlayerCom.CurWeaponID;
                        WeaponLeftHand = currWeapon.MainWeapon.Find("Home/hero_fpp_101_A_L");
                        WeaponRightHand = currWeapon.MainWeapon.Find("Home/hero_fpp_101_A_R");
                        WeaponAnimator = currWeapon.MainWeapon.GetComponentInChildren<Animator>();
                        Muzzle = currWeapon.MainWeapon.Find("muzzle");
                        if (weaponData.hideMuzzle)
                        {
                            if (Muzzle)
                                Muzzle.gameObject.active = false;
                        }

                        // 临时处理
                        offsetOverride = isParentOverride ? weaponData.parentOffset : weaponData.modelOffset;
                        rotationEulerOverride = isParentOverride ? weaponData.parentRotationEuler : weaponData.modelRotationEuler;

                        // 如果是狙击枪，则要加上VR里可用的狙击瞄具
                        if (weaponData.weaponType == WeaponDatas.WeaponType.SniperRifle)
                        {
                            var rifleWeaponData = (WeaponDatas.RifleWeaponData)weaponData;
                            var scopeRoot = currWeapon.MainWeapon.DeepFindChild(rifleWeaponData.scopeParent);
                            if (scopeRoot != null)
                            {
                                vrScope = scopeRoot.gameObject.GetOrAddComponent<VRScope>();
                                vrScope.Setup(rifleWeaponData);
                            }
                        }

                        if (weaponData.weaponType == WeaponDatas.WeaponType.Talisman)
                        {
                            var talismanWeaponData = (WeaponDatas.TalismanWeaponData)weaponData;
                            TalismanLeft = currWeapon.MainWeapon.Find(talismanWeaponData.leftWeaponName);
                            TalismanAmuletLeft = currWeapon.MainWeapon.Find(talismanWeaponData.leftAmuletName);
                        }

                        vrBattleUI.UpdateCrossHair();
                    }

                    if (weaponData.id == 1417 && currWeapon.OtherShowWeapon != null)
                    {
                        currWeapon.OtherShowWeapon.gameObject.active = false;
                    }

                    if (weaponData.weaponType == WeaponDatas.WeaponType.Helmet)
                    {
                        var helmetWeaponData = (WeaponDatas.HelmetWeaponData)weaponData;
                        if (WeaponAnimator.IsInState(helmetWeaponData.aimingHash))
                        {
                            CameraManager.MainCamera.position = Head.position;
                            CameraManager.MainCamera.rotation = Quaternion.LookRotation(GetFlatForwardDirection());
                        }
                    }

                    if (weaponIsShow)
                    {
                        // UI模式隐藏武器
                        currWeapon.MainWeapon.localScale = isUIMode ? Vector3.zero : Vector3.one;
                        // 扔炸弹时隐藏如律令的左手
                        bool forceOneHanded = hideIdleLeftHand;
                        bool forceModelTrans = !weaponData.useParentTrans || isSecondarySkill || isPrimarySkill;
                        forceModelTrans = false;
                        AttachWeaponToHand(currWeapon, weaponData, HandType.Right, forceOneHanded, forceModelTrans);
                        if (isDualWield)
                        {
                            //只有狗会双持
                            var deputyWeapon = HeroCameraManager.HeroObj.PlayerCom.GetCurWeapon(HeroCameraManager.HeroObj.PlayerCom.DeputyWeaponID);
                            if (deputyWeapon != null)
                            {
                                var weaponData2 = WeaponDatas.GetWeaponData(HeroCameraManager.HeroObj.PlayerCom.DeputyWeaponSID);
                                AttachWeaponToHand(deputyWeapon, weaponData2, HandType.Left, true, forceModelTrans);
                                //扔炸弹时隐藏左手武器
                                deputyWeapon.MainWeapon.parent.localScale = isSecondarySkill ? Vector3.zero : Vector3.one;
                            }
                        }
                    }
                }
            }
            else if (!isHome && CameraManager.MainCamera != null)
            {
                CameraManager.MainCamera.localScale = Vector3.zero;
                RightHand.muzzle.localEulerAngles = new Vector3(36, 0, 0);
            }
            if (isHome)
            {
                LeftHand.model.localScale = Vector3.one;
                RightHand.model.localScale = Vector3.one;
            }
        }

        public Dictionary<int, WeaponDatas.WeaponData> weaponDatas = WeaponDatas.weaponDatas;

        void AttachWeaponToHand(Weapon weapon, WeaponDatas.WeaponData weaponData, HandType handType, bool forceOneHanded = false, bool forceModelTrans = false)
        {
            bool twohanded = false;

            var parentOffset = isParentOverride ? offsetOverride : weaponData.parentOffset;
            var modelOffset = !isParentOverride ? offsetOverride : weaponData.modelOffset;
            var parentRotationEuler = isParentOverride ? rotationEulerOverride : weaponData.parentRotationEuler;
            var modelRotationEuler = !isParentOverride ? rotationEulerOverride : weaponData.modelRotationEuler;

            var parentTrans = weapon.MainWeapon.parent;
            var modelTrans = !isParentOverride || forceModelTrans ? weapon.MainWeapon : null;
            if (weaponData.useParentTrans)
                modelTrans = null;

            //如果是非单手武器,则进行双持判定
            if (!isDualWield && ModConfig.EnableTwoTwoHanded.Value && handType == HandType.Right && !forceOneHanded && weaponData.HoldingStyle == WeaponDatas.HoldingStyle.TwoHanded)
            {
                var LeftHandPos = LeftHand.model.position;
                if((weaponData.weaponType == WeaponDatas.WeaponType.Minigun))
                {
                    var minigunData = (WeaponDatas.MinigunWeaponData) weaponData;
                    LeftHandPos = LeftHand.model.position 
                        + Vector3.Cross(Vector3.up, RightHand.model.forward) * minigunData.leftHandOffset.x 
                        + Vector3.up * minigunData.leftHandOffset.y 
                        + RightHand.model.forward * minigunData.leftHandOffset.z;
                }
                var guidingVector = LeftHandPos - RightHand.model.position;
                float angleBetweenHands = Vector3.Angle(RightHand.model.forward, guidingVector);

                float angle = (isTwoHanded) ? 65f : 30f;

                // 根据角度判定双持，小角度贴上后要较大角度才会分开
                if (angleBetweenHands <= angle)
                {
                    // 弓箭主手是左手
                    if (weaponData.weaponType == WeaponDatas.WeaponType.Bow)
                    {
                        var bowData = (WeaponDatas.BowWeaponData)weaponData;
                        var lookRotation = Quaternion.LookRotation(guidingVector, LeftHand.model.up);
                        parentTrans.rotation = lookRotation;
                        RightHand.muzzle.rotation = parentTrans.rotation;
                        parentTrans.Rotate(0, 0, bowData.zAngle, Space.Self);
                        parentTrans.position = LeftHand.model.position + parentTrans.right * parentOffset.x + parentTrans.up * parentOffset.y + parentTrans.forward * (parentOffset.z - bowData.leftHandForwardDistance);
                        if(modelTrans != null)
                        {
                            modelTrans.rotation = lookRotation;
                            modelTrans.position = LeftHand.model.position + modelTrans.right * modelOffset.x + modelTrans.up * modelOffset.y + modelTrans.forward * (modelOffset.z - bowData.leftHandForwardDistance);
                        }
                    }
                    else
                    {
                        var upVector = (weaponData.weaponType == WeaponDatas.WeaponType.Minigun)? Vector3.up : RightHand.model.up;
                        var lookRotation = Quaternion.LookRotation(guidingVector, upVector);
                        parentTrans.rotation = lookRotation;
                        RightHand.muzzle.rotation = parentTrans.rotation;
                        parentTrans.Rotate(parentRotationEuler, Space.Self);
                        parentTrans.position = RightHand.model.position + parentTrans.right * parentOffset.x + parentTrans.up * parentOffset.y + parentTrans.forward * parentOffset.z;
                        if (modelTrans != null)
                        {
                            modelTrans.rotation = lookRotation;
                            modelTrans.Rotate(modelRotationEuler, Space.Self);
                            modelTrans.position = RightHand.model.position + modelTrans.right * modelOffset.x + modelTrans.up * modelOffset.y + modelTrans.forward * modelOffset.z;
                        }
                    }
                    if (WeaponLeftHand != null)
                        WeaponLeftHand.gameObject.active = true;
                    if (LeftHandMesh != null)
                        LeftHandMesh.gameObject.active = false;
                    twohanded = true;
                    isTwoHanded = true;
                    LeftHand.hideLaser = true;
                }
            }

            if(!twohanded)
            {
                LeftHand.hideLaser = false;
                isTwoHanded = false;
                if(handType == HandType.Right)
                {
                    // 还原双持时可能造成的瞄准线偏转
                    RightHand.muzzle.localEulerAngles = new Vector3(36, 0, 0);

                    // 弓和近战武器需要不同的处理，才能得到更自然的手感
                    if (weaponData.weaponType == WeaponDatas.WeaponType.Bow)
                    {
                        var bowData = (WeaponDatas.BowWeaponData)weaponData;
                        parentTrans.rotation = RightHand.model.rotation;
                        parentTrans.Rotate(0, 0, bowData.zAngle, Space.Self);
                        parentTrans.position = RightHand.model.position + parentTrans.right * parentOffset.x + parentTrans.up * parentOffset.y + parentTrans.forward * parentOffset.z;
                        if (modelTrans != null)
                        {
                            modelTrans.rotation = parentTrans.rotation;
                            modelTrans.position = LeftHand.model.position + modelTrans.right * modelOffset.x + modelTrans.up * modelOffset.y + modelTrans.forward * (modelOffset.z - bowData.leftHandForwardDistance);
                        }
                    }
                    else if (weaponData.weaponType == WeaponDatas.WeaponType.Melee)
                    {
                        var meleeData = (WeaponDatas.MeleeWeaponData)weaponData;
                        if(WeaponAnimator && WeaponAnimator.IsInState(meleeData.idleHash))
                        {
                            parentTrans.rotation = RightHand.model.rotation;
                            parentTrans.Rotate(meleeData.idleEuler, Space.Self);
                            parentTrans.position = RightHand.model.position + parentTrans.right * parentOffset.x + parentTrans.up * parentOffset.y + parentTrans.forward * parentOffset.z;
                            if (modelTrans != null)
                            {
                                modelTrans.rotation = parentTrans.rotation;
                                modelTrans.position = RightHand.model.position + modelTrans.right * modelOffset.x + modelTrans.up * modelOffset.y + modelTrans.forward * modelOffset.z;
                            }
                            if (WeaponRightHand != null)
                                WeaponRightHand.gameObject.active = false;
                        }
                        else
                        {
                            parentTrans.rotation = RightHand.model.rotation;
                            //parentTrans.position = RightHand.model.position + parentTrans.right * meleeData.attackOffset.x + parentTrans.up * meleeData.attackOffset.y + parentTrans.forward * meleeData.attackOffset.z;
                            if (modelTrans != null)
                            {
                                modelTrans.rotation = parentTrans.rotation;
                                modelTrans.position = RightHand.model.position + modelTrans.right * meleeData.attackOffset.x + modelTrans.up * meleeData.attackOffset.y + modelTrans.forward * meleeData.attackOffset.z;
                            }
                            if (WeaponRightHand != null)
                                WeaponRightHand.gameObject.active = true;
                        }
                    }
                    else if (weaponData.weaponType == WeaponDatas.WeaponType.Split)
                    {
                        var splitData = (WeaponDatas.SplitWeaponData)weaponData;
                        var splitPartTrans = parentTrans.Find(splitData.splitPartName + "right");
                        if (splitPartTrans == null)
                            splitPartTrans = parentTrans.parent.Find(splitData.splitPartName + "right");
                        if (splitPartTrans != null)
                        {
                            splitPartTrans.parent = parentTrans;
                            splitPartTrans.gameObject.active = true;
                            splitPartTrans.GetComponent<WeaponFollowHero>().enabled = false;
                            splitPartTrans.localRotation = Quaternion.identity;
                            splitPartTrans.localPosition = new Vector3 (1, -1, 1);
                        }
                        parentTrans.rotation = RightHand.model.rotation;
                        parentTrans.Rotate(parentRotationEuler, Space.Self);
                        parentTrans.position = RightHand.model.position + parentTrans.right * parentOffset.x + parentTrans.up * parentOffset.y + parentTrans.forward * parentOffset.z;
                        if (modelTrans != null)
                        {
                            modelTrans.rotation = RightHand.model.rotation;
                            parentTrans.Rotate(modelRotationEuler, Space.Self);
                            modelTrans.position = RightHand.model.position + modelTrans.right * modelOffset.x + modelTrans.up * modelOffset.y + modelTrans.forward * modelOffset.z;
                        }
                    }
                    else
                    {

                        parentTrans.rotation = RightHand.model.rotation;
                        parentTrans.Rotate(parentRotationEuler, Space.Self);
                        parentTrans.position = RightHand.model.position + parentTrans.right * parentOffset.x + parentTrans.up * parentOffset.y + parentTrans.forward * parentOffset.z;
                        if (modelTrans != null)
                        {
                            modelTrans.rotation = RightHand.model.rotation;
                            parentTrans.Rotate(modelRotationEuler, Space.Self);
                            modelTrans.position = RightHand.model.position + modelTrans.right * modelOffset.x + modelTrans.up * modelOffset.y + modelTrans.forward * modelOffset.z;
                        }
                    }
                    if (WeaponLeftHand != null && weaponData.weaponType != WeaponDatas.WeaponType.Talisman)
                        WeaponLeftHand.gameObject.active = false;
                }
                else
                {
                    parentOffset = weaponData.parentOffset;
                    modelOffset = weaponData.modelOffset;
                    parentRotationEuler = weaponData.parentRotationEuler;
                    modelRotationEuler = weaponData.modelRotationEuler;
                    parentTrans = weapon.DeputyWeapon.parent;
                    modelTrans = !isParentOverride || forceModelTrans ? weapon.DeputyWeapon : null;
                    if (weaponData.useParentTrans)
                        modelTrans = null;
                    LeftHand.muzzle.localEulerAngles = new Vector3(36, 0, 0);
                    // 只有狗的双持会需要在左手附上武器
                    if (weaponData.weaponType == WeaponDatas.WeaponType.Split)
                    {
                        var splitData = (WeaponDatas.SplitWeaponData)weaponData;
                        var splitPartTrans = parentTrans.Find(splitData.splitPartName + "left");
                        if (splitPartTrans == null)
                            splitPartTrans = parentTrans.parent.Find(splitData.splitPartName + "right");
                        if (splitPartTrans != null)
                        {
                            splitPartTrans.parent = parentTrans;
                            splitPartTrans.gameObject.active = true;
                            splitPartTrans.GetComponent<WeaponFollowHero>().enabled = false;
                            splitPartTrans.localRotation = Quaternion.identity;
                            splitPartTrans.localPosition = new Vector3(-1, -1, 1);
                        }
                    } 

                    {
                        parentTrans.rotation = LeftHand.model.rotation;
                        parentTrans.Rotate(parentRotationEuler.x, -parentRotationEuler.y, parentRotationEuler.z, Space.Self);
                        parentTrans.position = LeftHand.model.position + parentTrans.right * -parentOffset.x + parentTrans.up * parentOffset.y + parentTrans.forward * parentOffset.z;
                        if (modelTrans != null)
                        {
                            modelTrans.rotation = LeftHand.model.rotation;
                            modelTrans.Rotate(modelRotationEuler.x, -modelRotationEuler.y, modelRotationEuler.z, Space.Self);
                            modelTrans.position = LeftHand.model.position + modelTrans.right * -modelOffset.x + modelTrans.up * modelOffset.y + modelTrans.forward * modelOffset.z;
                        }
                    }
                    var weaponRightHand = weapon.DeputyWeapon.Find("Home/hero_fpp_101_A_R");
                    if (weaponRightHand != null && weaponData.weaponType != WeaponDatas.WeaponType.Talisman)
                        weaponRightHand.gameObject.active = false;
                }
                if (LeftHandMesh != null && !isDualWield)
                    LeftHandMesh.gameObject.active = true;
            }
        }

        #endregion

        public void SetUIMode(bool uiMode)
        {
            isUIMode = uiMode;
            LeftHand.SetUIMode(uiMode);
            RightHand.SetUIMode(uiMode);
            LeftHand.model.Find("cat_hand_model").gameObject.active = isHome;
            RightHand.model.Find("cat_hand_model").gameObject.active = isHome;
            LeftHand.model.gameObject.active = !uiMode || isHome;
            RightHand.model.gameObject.active = !uiMode || isHome;
            // 还原双持时可能造成的瞄准线偏转
            if (uiMode)
            {
                RightHand.muzzle.localEulerAngles = new Vector3(36, 0, 0);
                RightHand.muzzle.gameObject.layer = Layer.UI;
            }
            else
                RightHand.muzzle.gameObject.layer = Layer.Default;
            VRBattleUI.SetUIMode(uiMode);
        }

        public void FixDieScreen()
        {
            var hub_die = GameObject.Find("hub_die(Clone)");
            hub_die.transform.localEulerAngles = Vector3.zero;
            //hub_die.active = false;
        }

        public void SetCGCamera(bool isInCG, Camera cgCamera = null)
        {
            this.isInCG = isInCG;
            SetUIMode(isInCG);
            if (isInCG)
            {
                this.CGCamera = cgCamera.transform;
                //Head.parent = cgCamera.transform;
                //Head.localPosition = Vector3.zero;
                //Head.localRotation = Quaternion.identity;
                //ScreenCam.cullingMask = cgCamera.cullingMask;
                StereoRender.SetCameraMask(cgCamera.cullingMask);
                tempAngleCG = Vector3.Angle(Head.forward.GetFlatDirection(), Origin.forward.GetFlatDirection());
            }
            else
            {
                //Head.parent = gameObject.transform;
                //Head.localPosition = Vector3.zero;
                //Head.localRotation = Quaternion.identity;
                //ScreenCam.cullingMask = -1;
                StereoRender.SetCameraMask(StereoRender.defaultCullingMask);
            }
        }

        public Vignette vignette;

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log.Debug("VRPlayer.OnSceneLoaded: " + scene.name);
            if (scene.name?.ToLower() == "home")
                SetupHome();
            else
                isHome = false;

            if (RegexManager.IsNumber(scene.name))
            {
                // Spirital Assault Fix
                if (scene.name == "1410101" || scene.name == "1410103"){
                    MelonCoroutines.Start(BattlePrep(1));
                }
                else
                    MelonCoroutines.Start(BattlePrep(0));
            }
        }
        static int count = 0;

        private void SetupHome()
        {
            Log.Debug("SetupHome: " + (++count));
            isHome = true;
            isHomeFixed = false;
            IsReadyForBattle = false;
            lastWeaponID = 0;
            effectRoot = null;
            SetOriginHome();
            if (vrBattleUI)
            {
                Destroy(vrBattleUI);
                vrBattleUI = null;
            }
            if (LeftHandMesh)
            {
                Destroy(LeftHandMesh.gameObject);
                LeftHandMesh = null;
            }
            if (OriginTarget)
            {
                Destroy(OriginTarget.gameObject);
                OriginTarget = null;
            }
            bezierLineRenderers.Clear();
            Head.gameObject.active = false;
            MelonCoroutines.Start(DOFFix());
        }

        public IEnumerator DOFFix()
        {
            yield return new WaitForSeconds(0.1f);
            Head.gameObject.active = true;
            int i = 0;
            while(!CUIManager.instance.UICamera.GetComponent<PostProcessVolume>().profile.HasSettings<DepthOfField>() && isHome)
            {
                if (i++ < 10)
                    yield return new WaitForSeconds(0.1f);
                else
                    yield break;
            }
            // 在VR里景深效果只会让你变成近视眼
            CUIManager.instance.UICamera.GetComponent<PostProcessVolume>().profile.RemoveSettings<DepthOfField>();
            vignette = CUIManager.instance.UICamera.GetComponent<PostProcessVolume>().profile.GetSetting<Vignette>();
        }

        public IEnumerator BattlePrep(int gamemode)
        {
            Log.Debug("BattlePrep gamemode=" + gamemode);
            while (HeroCameraManager.HeroObj == null || HeroCameraManager.HeroTran == null)
                yield return new WaitForSeconds(0.1f);
            // UI
            ToggleEventCamera(true);
            SetUIMode(false);

            StereoRender.LeftCam.gameObject.active = false;
            StereoRender.RightCam.gameObject.active = false;
            yield return new WaitForSeconds(0.1f);
            StereoRender.LeftCam.gameObject.active = true;
            StereoRender.RightCam.gameObject.active = true;

            // Spirital Assault Fix
            if (gamemode == 1) {
                var effectRoot = GameObject.Find("CUIManager/Canvas_PC(Clone)/MainRoot/PanelPopup/lay_survival_new/lay_schedule/FightingEffect/UI_progress_bar/Position/Fire");
                if (effectRoot != null)
                    effectRoot.gameObject.active = false;
            }

            //vignette = CUIManager.instance.UICamera.GetComponent<PostProcessVolume>().profile.GetSetting<Vignette>();
            Camera[] cams = FindObjectsOfType<Camera>();
            foreach (Camera c in cams)
            {
                c.stereoTargetEye = StereoTargetEyeMask.None;
            }
            Camera.stereoTargetEye = StereoTargetEyeMask.Both;
            vrBattleUI = Head.gameObject.GetOrAddComponent<VRBattleUI>();
            vrBattleUI.Setup();

            etcTouchPad = FindObjectOfType<ETCTouchPad>();

            etcTransformCtrl_Hero = HeroCameraManager.HeroTran.GetComponent<ETCTransformCtrl>();
            etcTransformCtrl_FPSCam = CameraManager.MainCamera.GetComponent<ETCTransformCtrl>();
            //if(etcTransformCtrl_Hero)
            //    Destroy(etcTransformCtrl_Hero);
            if (etcTransformCtrl_FPSCam)
                Destroy(etcTransformCtrl_FPSCam);
            Origin.position = HeroCameraManager.HeroTran.position;
            Origin.rotation = HeroCameraManager.HeroTran.rotation;
            fpsCamMotionCtrl = CameraManager.MainCamera.GetComponent<FPSCamMotionCtrl>();
            fpsCamMotionCtrl.enabled = false;
            cams = CameraManager.MainCamera.GetComponentsInChildren<Camera>();
            foreach (Camera c in cams)
            {
                c.enabled = false;
            }
            if (cams.Length > 0)
            {
                StereoRender.defaultCullingMask = cams[0].cullingMask;
                StereoRender.defaultCullingMask |= 1 << Layer.Weapon;
            }
            StereoRender.SetCameraMask(StereoRender.defaultCullingMask);

            // 目标点使用UI相机来进行位置计算
            HeroWarSign.UISignManager.signCamera = CUIManager.instance.UICamera;

            // 储存技能手Transform的引用
            HeroSkillHand = HeroCameraManager.HeroObj.PlayerCom.GetHeroHandTran();
            HeroSkillHandAnimator = HeroSkillHand.GetComponent<Animator>();

            var LeftHandRenderers = HeroSkillHand.GetComponentsInChildren<SkinnedMeshRenderer>();

            LeftHandMesh = LeftHand.model.Find("LeftHandMesh");
            if (!LeftHandMesh)
            {
                LeftHandMesh = new GameObject("LeftHandMesh").transform;
                LeftHandMesh.parent = LeftHand.model;
                LeftHand.model.localScale = Vector3.one / ModConfig.PlayerWorldScale.Value;
            }
            LeftHandMesh.localScale = new Vector3(0.67f, 0.67f, 0.67f);
            LeftHandMesh.localPosition = new Vector3(0.5631f, -0.0083f, -0.35f);
            LeftHandMesh.localEulerAngles = new Vector3(0f, 202.2307f, 180f);

            // Zi Xiao left hand fix
            if (HeroCameraManager.HeroObj.PlayerCom.SID == 216) 
            {
                HeroSkillHandHide = HeroSkillHand.Find("113_A_L");
                LeftHandMesh.gameObject.GetOrAddComponent<MeshFilter>().mesh = LeftHandRenderers[1].sharedMesh;
                LeftHandMesh.gameObject.GetOrAddComponent<MeshRenderer>().material = LeftHandRenderers[1].sharedMaterial;
            } else {
                LeftHandMesh.gameObject.GetOrAddComponent<MeshFilter>().mesh = LeftHandRenderers[0].sharedMesh;
                LeftHandMesh.gameObject.GetOrAddComponent<MeshRenderer>().material = LeftHandRenderers[0].sharedMaterial;
            }
            if (HeroCameraManager.HeroObj.PlayerCom.SID == 217)
            {
                HeroSkillHandHide = HeroSkillHand.Find("hero_fpp_114_A_R");
            }
            if (HeroCameraManager.HeroObj.PlayerCom.SID == 219)
            {
                TPSCamBoom = HeroCameraManager.HeroTran.Find("TPSCamBoom");
                TPSCam = TPSCamBoom?.Find("TPSCam");
                if(OriginTarget == null)
                    OriginTarget = new GameObject("OriginTarget").transform;
            }

            foreach (var child in CameraManager.MainCamera)
            {
                if (child.Cast<Transform>().gameObject.name.Contains("_WeaponHands"))
                {
                    HeroWeaponHand = child.Cast<Transform>();
                    HeroWeaponHandAnimator = HeroWeaponHand.GetComponent<Animator>();
                }
            }
            ScreenCam.gameObject.active = true;
            IsReadyForBattle = true;
        }

        // public static void SetEventState(string state)
        // {
        //     Debug.Log("### LOGGING STATE ###");
        //     Debug.Log(state.ToString());
        //     Debug.Log("### STATE LOGGED ###");
        // }

        public void OnDestroy()
        {
            HarmonyPatches.onSceneLoaded -= OnSceneLoaded;
        }

        private void SetOriginHome()
        {
            Log.Debug("SetOriginHome");
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
