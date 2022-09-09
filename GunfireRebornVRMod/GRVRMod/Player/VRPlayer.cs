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
using VRMod.UI.Pointers;
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
        public VRBattleUI vrBattleUI { get; private set; }

        public bool IsReadyForBattle { get; private set; }

        public bool isHome = false;
        public bool isUIMode = false;

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

            HarmonyPatches.onSceneLoaded += OnSceneLoaded;

            // 新增一个新手柄，输入源是自己设定的SteamVR action
            FindObjectOfType<InControlManager>()?.gameObject.GetOrAddComponent<VRInputManager>();

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

                LeftHand = transform.Find("LeftHand").gameObject.AddComponent<HandController>();
                RightHand = transform.Find("RightHand").gameObject.AddComponent<HandController>();
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

        void FixedUpdate()
        {
            if (!isHome)
            {
                // 每秒转50度，太快再后期调整吧
                if (VRInputManager.Instance.vrDevice.RightStick.State)
                    Origin.Rotate(Vector3.up, VRInputManager.Instance.vrDevice.RightStick.X);
                else if(VRInputManager.Instance.vrDevice.SnapTurnLeft().stateDown)
                    Origin.Rotate(Vector3.up, -60);
                else if (VRInputManager.Instance.vrDevice.SnapTurnRight().stateDown)
                    Origin.Rotate(Vector3.up, 60);
            }
        }

        public Vector3 offsetOverride = new Vector3(0, -0.08f, -0.15f);
        private NewPlayerObject heroObj;
        public List<BezierLineRenderer> bezierLineRenderers = new List<BezierLineRenderer>();

        private int lastWeaponSID;
        private float aimSwitchDelay = 1f;

        private Transform HeroSkillHand;
        private Animator HeroSkillHandAnimator;
        private Transform HeroWeaponHand;
        private Animator HeroWeaponHandAnimator;
        private Transform WeaponLeftHand;
        private Animator WeaponAnimator;

        private bool isInIdleState = false;
        private bool isEnteringPrimarySkillState = false;
        private bool isInPrimarySkillState = false;
        private bool isExitingPrimarySkillState = false;

        public bool isDualWield { get { return dualWieldAKGameObj? dualWieldAKGameObj.enabled : false; } }

        private AkGameObj dualWieldAKGameObj;

        public Dictionary<int, float> states = new Dictionary<int, float>();


        public void SetDualWield(Transform dualWieldTrans)
        {
            this.dualWieldAKGameObj = dualWieldTrans.GetComponent<AkGameObj>();
            vrBattleUI.UpdateCrossHair();
            VRInputManager.Instance.vrDevice.dualWieldDelay = 0.5f;
        }


        // 在LateUpdate里才能覆盖英雄身体的位置和朝向
        void LateUpdate()
        {
            
            if (Input.GetKeyUp(KeyCode.J))
            {
                ToggleEventCamera();
            }
            if (Input.GetKeyUp(KeyCode.P))
            {
                ScreenCam.enabled = !ScreenCam.enabled;
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
            if (!isUIMode && !isHome && IsReadyForBattle && HeroCameraManager.HeroTran)
            {
                // 默认大小在第一人称下看着太大了，需要缩小一点
                CameraManager.MainCamera.localScale = new Vector3(0.67f, 0.67f, 0.67f);

                // 将游玩空间的位置跟英雄的位置同步
                Origin.position = HeroCameraManager.HeroTran.position;

                // 强制英雄的朝向与玩家头部朝向同步
                HeroCameraManager.HeroTran.rotation = Quaternion.LookRotation(GetFlatForwardDirection());

                var heroData = HeroDatas.GetHeroData(HeroCameraManager.HeroObj.PlayerCom.SID);

                var hand = RightHand;

                // 左手用技能时转为左手瞄准，松开后延迟0.5秒变回右手
                bool isLeftHandPressed = VRInputManager.Instance.vrDevice.RB_SecondarySkill().state || VRInputManager.Instance.vrDevice.LB_PrimarySkill().state;

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

                if (isLeftHandPressed)
                    aimSwitchDelay = 0.5f;
                if (aimSwitchDelay > 0 || isSecondarySkill || isPrimarySkill)
                {
                    aimSwitchDelay -= Time.deltaTime;
                    hand = LeftHand;
                }

                // 瞄准当前手柄指向的位置
                CameraManager.MainCamera.position = Head.position;
                Vector3 dir = hand.GetRayHitPosition() - CameraManager.MainCamera.position;
                CameraManager.MainCamera.rotation = Quaternion.LookRotation(dir);

                bool hideWeaponLeftHand = false;

                // 不放技能时默认隐藏技能手
                if (HeroSkillHand != null)
                {
                    bool isPlaying = aimSwitchDelay > 0 || isSecondarySkill || isPrimarySkill;
                    HeroSkillHand.localScale = isPlaying ? Vector3.one : Vector3.zero;
                    hideWeaponLeftHand = isPlaying;
                }
                if (HeroWeaponHand != null)
                {
                    bool isPlaying = HeroWeaponHandAnimator.IsPlaying() || isSecondarySkill || isPrimarySkill;
                    HeroWeaponHand.localScale = HeroWeaponHandAnimator.IsPlaying() ? Vector3.one : Vector3.zero;
                    hideWeaponLeftHand |= isPlaying;
                }

                if (WeaponAnimator)
                {
                    var state = WeaponAnimator.GetCurrentAnimatorStateInfo(0);
                    if (states.ContainsKey(state.fullPathHash))
                        states[state.fullPathHash] = WeaponAnimator.GetCurrentAnimatorStateInfo(0).m_NormalizedTime;
                    else
                        states.Add(state.fullPathHash, WeaponAnimator.GetCurrentAnimatorStateInfo(0).m_NormalizedTime);
                }

                var currWeapon = HeroCameraManager.HeroObj.PlayerCom.GetCurWeapon(HeroCameraManager.HeroObj.PlayerCom.CurWeaponID);

                var weaponIsShow = true;
                switch (heroData.sid)
                {
                    case 206:
                    case 213:
                    case 215:
                        // 鸟、狐狸、龟龟
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
                }

                // 强制游戏自带的手和武器模型与玩家手同步
                heroObj = HeroCameraManager.HeroObj;
                if (currWeapon != null)
                {

                    var weaponSID = HeroCameraManager.HeroObj.PlayerCom.CurWeaponSID;
                    var weaponData = WeaponDatas.GetWeaponData(weaponSID);
                    // 切枪时初始化枪械相关引用
                    if (weaponSID != lastWeaponSID)
                    {
                        lastWeaponSID = weaponSID;
                        WeaponLeftHand = currWeapon.MainWeapon.Find("Home/hero_fpp_101_A_L");
                        WeaponAnimator = currWeapon.MainWeapon.GetComponentInChildren<Animator>();
                        if (weaponData.hideMuzzle)
                        {
                            var muzzle = currWeapon.MainWeapon.Find("Home/muzzle");
                            if (muzzle)
                                muzzle.gameObject.active = false;
                        }

                        // 如果是狙击枪，则要加上VR里可用的狙击瞄具
                        if (weaponData.weaponType == WeaponDatas.WeaponType.SniperRifle)
                        {
                            var rifleWeaponData = (WeaponDatas.RifleWeaponData)weaponData;
                            var scopeRoot = currWeapon.MainWeapon.DeepFindChild(rifleWeaponData.scopeParent);
                            if (scopeRoot != null)
                                scopeRoot.gameObject.GetOrAddComponent<VRScope>().Setup(rifleWeaponData);
                        }
                        vrBattleUI.UpdateCrossHair();
                    }
                    if (weaponIsShow)
                    {
                        // UI模式隐藏武器
                        currWeapon.MainWeapon.localScale = isUIMode ? Vector3.zero : Vector3.one;
                        AttachWeaponToHand(currWeapon.MainWeapon, weaponData, HandType.Right, hideWeaponLeftHand);
                        if (isDualWield)
                        {
                            //只有狗会双持
                            var deputyWeapon = HeroCameraManager.HeroObj.PlayerCom.GetCurWeaponTran(HeroCameraManager.HeroObj.PlayerCom.DeputyWeaponID);
                            if (deputyWeapon)
                            {
                                var weaponData2 = WeaponDatas.GetWeaponData(HeroCameraManager.HeroObj.PlayerCom.DeputyWeaponSID);
                                AttachWeaponToHand(deputyWeapon, weaponData2, HandType.Left, true);
                                //扔炸弹时隐藏左手武器
                                deputyWeapon.localScale = isSecondarySkill ? Vector3.zero : Vector3.one;
                            }
                        }
                    }
                }
            }
            else if (!isHome && CameraManager.MainCamera != null)
            {
                CameraManager.MainCamera.localScale = Vector3.zero;
                // 还原双持时可能造成的瞄准线偏转
                RightHand.muzzle.localEulerAngles = new Vector3(36, 0, 0);
            }

            // 需要在LateUpdate里更新彩虹的射线，才能正常显示
            foreach(var lineRenderer in bezierLineRenderers)
            {
                if (lineRenderer && !lineRenderer.playSight)
                    lineRenderer.Draw();
            }
        }

        public Dictionary<int, WeaponDatas.WeaponData> weaponDatas = WeaponDatas.weaponDatas;

        void AttachWeaponToHand(Transform weapon, WeaponDatas.WeaponData weaponData, HandType handType, bool forceOneHanded = false)
        {
            bool twohanded = false;
            //如果是非单手武器,则进行双持判定
            if (handType == HandType.Right && !forceOneHanded && weaponData.HoldingStyle == WeaponDatas.HoldingStyle.TwoHanded)
            {
                var LeftHandPos = LeftHand.model.position;
                if((weaponData.weaponType == WeaponDatas.WeaponType.Minigun))
                {
                    var minigunData = (WeaponDatas.MinigunWeaponData) weaponData;
                    LeftHandPos = LeftHand.model.position 
                        + weapon.right * minigunData.leftHandOffset.x 
                        + weapon.up * minigunData.leftHandOffset.y 
                        + weapon.forward * minigunData.leftHandOffset.z;
                }
                var guidingVector = LeftHandPos - RightHand.model.position;
                float angleBetweenHands = Vector3.Angle(RightHand.model.forward, guidingVector);

                // 角度小于50度则进行双持
                if (angleBetweenHands <= 50f)
                {
                    var offset = weaponData.offset;
                    if (offset == Vector3.zero)
                        offset = offsetOverride;
                    // 弓箭主手是左手
                    if (weaponData.weaponType == WeaponDatas.WeaponType.Bow)
                    {
                        var bowData = (WeaponDatas.BowWeaponData)weaponData;
                        weapon.rotation = Quaternion.LookRotation(guidingVector, LeftHand.model.up);
                        weapon.Rotate(0, 0, bowData.zAngle, Space.Self);
                        RightHand.muzzle.rotation = weapon.rotation;
                        weapon.position = LeftHand.model.position + weapon.right * offset.x + weapon.up * offset.y + weapon.forward * (offset.z - bowData.leftHandForwardDistance);
                    }
                    else
                    {
                        if ((weaponData.weaponType == WeaponDatas.WeaponType.Minigun))
                            weapon.rotation = Quaternion.LookRotation(guidingVector, Vector3.up);
                        else
                            weapon.rotation = Quaternion.LookRotation(guidingVector, RightHand.model.up);
                        RightHand.muzzle.rotation = weapon.rotation;
                        weapon.position = RightHand.model.position + weapon.right * offset.x + weapon.up * offset.y + weapon.forward * offset.z;
                    }
                    if (WeaponLeftHand != null)
                        WeaponLeftHand.gameObject.active = true;
                    twohanded = true;
                }
            }

            if(!twohanded)
            {
                var offset = weaponData.offset;
                if (offset == Vector3.zero)
                    offset = offsetOverride;
                if(handType == HandType.Right)
                {
                    // 还原双持时可能造成的瞄准线偏转
                    RightHand.muzzle.localEulerAngles = new Vector3(36, 0, 0);

                    // 弓和近战武器需要不同的处理，才能得到更自然的手感
                    if (weaponData.weaponType == WeaponDatas.WeaponType.Bow)
                    {
                        var bowData = (WeaponDatas.BowWeaponData)weaponData;
                        weapon.rotation = RightHand.model.rotation;
                        weapon.Rotate(0, 0, bowData.zAngle, Space.Self);
                    }
                    else if (weaponData.weaponType == WeaponDatas.WeaponType.Melee)
                    {
                        var meleeData = (WeaponDatas.MeleeWeaponData)weaponData;
                        if(WeaponAnimator && WeaponAnimator.IsInState(meleeData.idleHash))
                        {
                            weapon.rotation = RightHand.model.rotation;
                            weapon.Rotate(meleeData.idleEuler.x, meleeData.idleEuler.y, meleeData.idleEuler.z, Space.Self);
                            weapon.position = RightHand.model.position + weapon.right * offset.x + weapon.up * offset.y + weapon.forward * offset.z;
                        }
                        else
                        {
                            weapon.rotation = RightHand.model.rotation;
                            weapon.position = RightHand.model.position + weapon.right * meleeData.attackOffset.x + weapon.up * meleeData.attackOffset.y + weapon.forward * meleeData.attackOffset.z;
                        }
                    }
                    else
                    {
                        weapon.rotation = RightHand.model.rotation;
                        weapon.Rotate(weaponData.rotationEuler, Space.Self);
                        weapon.position = RightHand.model.position + weapon.right * offset.x + weapon.up * offset.y + weapon.forward * offset.z;
                    }
                    if (WeaponLeftHand != null && weaponData.weaponType != WeaponDatas.WeaponType.Talisman)
                        WeaponLeftHand.gameObject.active = false;
                }
                else
                {
                    // 只有狗的双持会需要在左手附上武器
                    LeftHand.muzzle.localEulerAngles = new Vector3(36, 0, 0);
                    weapon.rotation = LeftHand.model.rotation;
                    weapon.position = LeftHand.model.position + weapon.right * -offset.x + weapon.up * offset.y + weapon.forward * offset.z;
                    var weaponRightHand = weapon.Find("Home/hero_fpp_101_A_R");
                    if (weaponRightHand != null && weaponData.weaponType != WeaponDatas.WeaponType.Talisman)
                        weaponRightHand.gameObject.active = false;
                }
            }
        }

        public void SetUIMode(bool uiMode)
        {
            isUIMode = uiMode;
            LeftHand.uiMode = uiMode;
            RightHand.uiMode = uiMode;
            LeftHand.model.gameObject.active = uiMode && !IsReadyForBattle;
            RightHand.model.gameObject.active = uiMode && !IsReadyForBattle;
            VRBattleUI.SetUIMode(uiMode);
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name?.ToLower() == "home")
            {
                isHome = true;
                IsReadyForBattle = false;
                lastWeaponSID = 0;
                SetOriginHome();
                Destroy(vrBattleUI);
                MelonCoroutines.Start(HomePre());
            }
            else
            {
                isHome = false;
            }

            if(RegexManager.IsNumber(scene.name))
            {
                MelonCoroutines.Start(BattlePrep());
            }
        }

        public IEnumerator HomePre()
        {
            yield return new WaitForSeconds(0.5f);

            SetUIMode(true);

            // 在VR里景深效果只会让你变成近视眼
            CUIManager.instance.UICamera.GetComponent<PostProcessVolume>().profile.RemoveSettings<DepthOfField>();
            ToggleEventCamera(true);
            ScreenCam.gameObject.active = true;
            ScreenCam.enabled = true;
        }

        public IEnumerator BattlePrep()
        {
            while (HeroCameraManager.HeroObj == null || HeroCameraManager.HeroTran == null)
                yield return new WaitForSeconds(0.2f);

            // UI
            SetUIMode(false);
            Camera[] cams = FindObjectsOfType<Camera>();
            foreach (Camera c in cams)
            {
                c.stereoTargetEye = StereoTargetEyeMask.None;
            }
            Camera.stereoTargetEye = StereoTargetEyeMask.Both;
            vrBattleUI = Head.gameObject.GetOrAddComponent<VRBattleUI>();
            vrBattleUI.Setup();
            Origin.position = HeroCameraManager.HeroTran.position;

            CameraManager.MainCamera.GetComponent<FPSCamMotionCtrl>().enabled = false;
            cams = CameraManager.MainCamera.GetComponentsInChildren<Camera>();
            foreach (Camera c in cams)
            {
                c.enabled = false;
            }

            // 储存技能手Transform的引用
            HeroSkillHand = HeroCameraManager.HeroObj.PlayerCom.GetHeroHandTran();
            HeroSkillHandAnimator = HeroSkillHand.GetComponent<Animator>();

            foreach (var child in CameraManager.MainCamera)
            {
                if (child.Cast<Transform>().gameObject.name.Contains("_WeaponHands"))
                {
                    HeroWeaponHand = child.Cast<Transform>();
                    HeroWeaponHandAnimator = HeroWeaponHand.GetComponent<Animator>();
                }
            }
            ScreenCam.gameObject.active = true;
            ScreenCam.enabled = true;
            IsReadyForBattle = true;
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
