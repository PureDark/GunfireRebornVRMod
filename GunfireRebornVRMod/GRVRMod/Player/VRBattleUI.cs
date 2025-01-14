using HeroCameraName;
using System;
using UI;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using VRMod.Player.MotionControlls;
using VRMod.Settings;
using static RootMotion.FinalIK.RagdollUtility;
using static VRMod.VRMod;

namespace VRMod.Player
{
    public class VRBattleUI : MonoBehaviour
    {
        public VRBattleUI(IntPtr value) : base(value) { }
        public Transform Head;
        public HandController LeftHand, RightHand;
        public Transform? canvasRoot;
        public Transform? PC_Panel_war;
        public Transform? minimap;
        public Transform? hp;
        public Transform? curweapon;
        public Transform? Fastmove_tips;
        public Transform? button_grenade;
        public Transform? button_aim;
        public Transform? hero_skill_1;
        public Transform? leftCrossHair;
        public Transform? rightCrossHair;
        public Transform? foxChargingAim;
        public Transform? doomFistInterface;

        public Transform coinmessage;
        public Transform cashmessage;

        public Transform Target_tips;
        public Transform Shockwave1, Shockwave2;

        public Transform img_target;
        public Transform img_chest;
        public Transform img_buffnpc;
        public Transform img_shop;
        public Transform img_weaponshop;
        public Transform img_gscashshop;
        public Transform img_firepile;

        public Transform SimLineStart;

        private Transform canvasRootTarget;
        private Transform hpTarget;

        public float distance = 3.5f;
        public float smoothTime = 0.3f;
        private float minimapScaleLerp = 0f;
        private float coinScaleLerp = 0f;
        private Vector3 canvasRootVelocity = Vector3.zero;
        private Vector3 hpVelocity = Vector3.zero;
        private Quaternion canvasRootDeriv = Quaternion.identity;
        private Quaternion hpDeriv = Quaternion.identity;
        private bool setup = false;

        public void Awake()
        {
            Head = VRPlayer.Instance.Head;
            LeftHand = VRPlayer.Instance.LeftHand;
            RightHand = VRPlayer.Instance.RightHand;
        }

        public void Start()
        {
        }

        public void Setup()
        {
            Reset();
            setup = true;
        }

        public void Reset()
        {
            canvasRoot = null;
            PC_Panel_war = null;
            curweapon = null;
            leftCrossHair = null;
            rightCrossHair = null;
            hp = null;
            hero_skill_1 = null;
            minimap = null;
            Fastmove_tips = null;
            button_grenade = null;
            button_aim = null;
            coinmessage = null;
            cashmessage = null;
        }

        public static void SetUIMode(bool uiMode)
        {
            if(uiMode)
                CUIManager.instance.MenuCanvas.sortingOrder = CUIManager.instance.MainDialogCanvas.sortingOrder - 1;
            else
                CUIManager.instance.MenuCanvas.sortingOrder = CUIManager.instance.MainDialogCanvas.sortingOrder + 1;
        }

        public void FixCrossHair(Transform trans)
        {
            var crossHair = trans.Find("ScaleCurDis");
            if (crossHair != null)
            {
                crossHair.localRotation = Quaternion.identity;
            }
        }

        public void FixSplitWeaponCrossHair(Transform trans)
        {
            var crossHair = trans.Find("Mainsight/position/open/static/point_loop");
            if (crossHair != null)
            {
                crossHair.localScale = new Vector3(0.002f, 0.002f, 0.002f);
            }
        }

        public void UpdateCrossHair()
        {
            leftCrossHair = null;
            rightCrossHair = null;
            if (HeroCameraManager.HeroObj == null || HeroCameraManager.HeroObj.PlayerCom == null)
                return;
            var mainWeapon = HeroCameraManager.HeroObj.PlayerCom.GetCurWeapon(HeroCameraManager.HeroObj.PlayerCom.CurWeaponID);
            var deputyWeapon = HeroCameraManager.HeroObj.PlayerCom.GetCurWeapon(HeroCameraManager.HeroObj.PlayerCom.DeputyWeaponID);
            foreach (var child in CUIManager.instance.SightCanvas.transform)
            {
                var childTrans = child.Cast<Transform>();
                if (mainWeapon != null && childTrans.name == "Panel_Sight_" + HeroCameraManager.HeroObj.PlayerCom.CurWeaponSID + "_" + mainWeapon.ClientPos)
                    rightCrossHair = childTrans;
                else if(deputyWeapon != null && childTrans.name.StartsWith("Panel_Sight_" + HeroCameraManager.HeroObj.PlayerCom.DeputyWeaponSID + "_" + deputyWeapon.ClientPos))
                    leftCrossHair = childTrans;

                if(rightCrossHair != null)
                    FixCrossHair(rightCrossHair);
                if (leftCrossHair != null)
                    FixCrossHair(leftCrossHair);

                //修复璇玑的准心过大的问题
                if (HeroCameraManager.HeroObj.PlayerCom.CurWeaponSID == 1703 && rightCrossHair != null)
                    FixSplitWeaponCrossHair(rightCrossHair);
                if (HeroCameraManager.HeroObj.PlayerCom.DeputyWeaponSID == 1703 && leftCrossHair != null)
                    FixSplitWeaponCrossHair(leftCrossHair);

                if (HeroCameraManager.HeroObj.PlayerCom.CurWeaponSID == 1417)
                {
                    var obj = GameObject.Find("Panel_Sight_1417_9600(Clone)");
                    if (obj != null)
                        doomFistInterface = obj.transform;
                }
            }
        }

        public void Update()
        {
            if (setup)
            {
                distance = ModConfig.UIDistance.Value;
                canvasRoot = CUIManager.instance.m_UIRoot;
                canvasRoot.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f) * ModConfig.UIScale.Value / ModConfig.PlayerWorldScale.Value;
                PC_Panel_war = CUIManager.instance.MenuCanvas.transform.Find("PC_Panel_war");
                if (PC_Panel_war)
                {
                    setup = false;
                    PC_Panel_war.GetComponent<Animator>().enabled = false;
                    PC_Panel_war.Find("bullet_tips").gameObject.active = false;
                    hp = PC_Panel_war.Find("hp");
                    Fastmove_tips = PC_Panel_war.Find("lay_skill/lay_fastmove");
                    button_grenade = PC_Panel_war.Find("lay_skill/lay_grenade");
                    button_aim = PC_Panel_war.Find("lay_skill/button_aim");
                    curweapon = PC_Panel_war.Find("curweapon");
                    hero_skill_1 = PC_Panel_war.Find("hero_skill_1");
                    coinmessage = PC_Panel_war.Find("coinmessage");
                    cashmessage = PC_Panel_war.Find("cashmessage");
                    minimap = CUIManager.instance.MenuCanvas.transform.Find("minimap");

                    hp.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                    Fastmove_tips.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                    button_grenade.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                    button_aim.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                    curweapon.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    coinmessage.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                    cashmessage.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                    minimap.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                    canvasRootTarget = VRPlayer.Instance.Origin.Find("canvasRootTarget");
                    if (!canvasRootTarget)
                    {
                        canvasRootTarget = new GameObject("canvasRootTarget").transform;
                        canvasRootTarget.parent = VRPlayer.Instance.Origin;
                        canvasRootTarget.position = canvasRoot.position;
                        canvasRootTarget.rotation = canvasRoot.rotation;
                    }

                    hpTarget = VRPlayer.Instance.Origin.Find("hpTarget");
                    if (!hpTarget)
                    {
                        hpTarget = new GameObject("hpTarget").transform;
                        hpTarget.parent = VRPlayer.Instance.Origin;
                        hpTarget.position = hp.position;
                        hpTarget.rotation = hp.rotation;
                    }

                    // 是狗或者猫的话要把技能上的粒子特效删了，无法缩放会挡住视野
                    if(HeroCameraManager.HeroObj.SID == 201)
                    {
                        var skillIcon = hero_skill_1.Find("Lay_Ani/skill_loop_201");
                        if (skillIcon)
                        {
                            var temp = skillIcon.Find("postion");
                            foreach (var child in temp)
                            {
                                if(child.Cast<Transform>().name != "Img_logo")
                                    Destroy(child.Cast<Transform>().gameObject);
                            }
                        }
                    }
                    else if (HeroCameraManager.HeroObj.SID == 205)
                    {
                        var skillIcon = hero_skill_1.Find("Lay_Ani/skill_loop_205");
                        if (skillIcon)
                        {
                            var temp = skillIcon.Find("postion");
                            foreach (var child in temp)
                            {
                                if (child.Cast<Transform>().name != "icon_205")
                                    Destroy(child.Cast<Transform>().gameObject);
                            }
                        }
                    }
                    else if (HeroCameraManager.HeroObj.SID == 219)
                    {
                        var skillIcon = hero_skill_1.Find("Lay_Ani/skill_loop_219");
                        if (skillIcon)
                        {
                            var temp = skillIcon.Find("postion");
                            foreach (var child in temp)
                            {
                                if (child.Cast<Transform>().name != "Img_skill_219")
                                    Destroy(child.Cast<Transform>().gameObject);
                            }
                        }
                    }

                    Target_tips = PC_Panel_war.Find("Target_Tips_Tar");
                    SimLineStart = CameraManager.MainCamera.DeepFindChild("SimLineStart");
                    if (hero_skill_1)
                    {
                        var psrs = hero_skill_1.GetComponentsInChildren<ParticleSystemRenderer>(true);
                        foreach (var psr in psrs)
                        {
                            psr.alignment = ParticleSystemRenderSpace.Local;
                        }
                    }

                    // 重新排列UI的渲染顺序
                    hero_skill_1.SetAsFirstSibling();
                    hp.SetAsLastSibling();
                    Fastmove_tips.SetAsLastSibling();
                    button_grenade.SetAsLastSibling();
                    button_aim.SetAsLastSibling();
                    curweapon.SetAsLastSibling();
                    coinmessage.SetAsLastSibling();
                    cashmessage.SetAsLastSibling();
                }
                UpdateCrossHair();
            }
        }

        public void OnDestroy()
        {
            if (canvasRootTarget)
                Destroy(canvasRootTarget.gameObject);
            if (hpTarget)
                Destroy(hpTarget.gameObject);
        }

        //public static Vector3 overridePos = new Vector3(0.15f, -0.15f, -0.1f);
        //public static Vector3 overridePos2 = new Vector3(0.15f, -0.15f, -0.1f);

        public void LateUpdate()
        {
            if (VRPlayer.Instance.isHome)
            {
                return;
            }
            if (PC_Panel_war == null || PC_Panel_war.parent != CUIManager.instance.MenuCanvas.transform)
            {
                setup = true;
                return;
            }
            if (canvasRoot && !VRPlayer.Instance.isUIMode)
            {
                var targetPosition = Head.position + VRPlayer.Instance.GetFlatForwardDirection() * distance;
                canvasRootTarget.position = Vector3.SmoothDamp(canvasRootTarget.position, targetPosition, ref canvasRootVelocity, smoothTime);
                canvasRoot.position = canvasRootTarget.position;

                var smoothHUDRotation = canvasRootTarget.rotation.SmoothDamp(Quaternion.LookRotation(VRPlayer.Instance.GetFlatForwardDirection()), ref canvasRootDeriv, smoothTime);
                canvasRootTarget.rotation = smoothHUDRotation;
                canvasRoot.rotation = canvasRootTarget.rotation;
            }

            // 小地图显示到左手手腕，像手表一样查看
            if (minimap)
            {
                minimap.position = LeftHand.model.position - LeftHand.model.right*0.1f - LeftHand.model.forward * 0.16f - LeftHand.model.up * 0.1f;
                minimap.rotation = Quaternion.LookRotation(LeftHand.model.right, -LeftHand.model.up);
                bool show = Vector3.Angle(minimap.forward, minimap.position - Head.transform.position) <= 45;
                if (minimapScaleLerp != (show ? 1f : 0f))
                    minimapScaleLerp = Mathf.Clamp(minimapScaleLerp + (show ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime) * 6, 0f, 1f);
                minimap.localScale = Vector3.Lerp(Vector3.zero, new Vector3(0.1f, 0.1f, 0.1f), minimapScaleLerp);
            }

            // 金钱显示到右手手腕，像手表一样查看
            if (coinmessage && cashmessage)
            {
                var rotation = Quaternion.LookRotation(-RightHand.model.right, -RightHand.model.up);
                coinmessage.position = RightHand.model.position + RightHand.model.right * 0.1f + RightHand.model.up * -0.04f + RightHand.model.forward * -0.1f;
                coinmessage.rotation = rotation;
                cashmessage.position = RightHand.model.position + RightHand.model.right * 0.1f + RightHand.model.up * -0.08f + RightHand.model.forward * -0.09f;
                cashmessage.rotation = rotation;
                bool show = Vector3.Angle(coinmessage.forward, coinmessage.position - Head.transform.position) <= 45;
                if (coinScaleLerp != (show ? 1f : 0f))
                    coinScaleLerp = Mathf.Clamp(coinScaleLerp + (show ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime) * 6, 0f, 1f);
                coinmessage.localScale = Vector3.Lerp(Vector3.zero, new Vector3(0.1f, 0.1f, 0.1f), coinScaleLerp);
                cashmessage.localScale = Vector3.Lerp(Vector3.zero, new Vector3(0.1f, 0.1f, 0.1f), coinScaleLerp);
            }

            // 血量相关UI显示在视野左下角，并进行有延迟的平滑跟随
            if (hp)
            {
                var targetPosition = Head.position + VRPlayer.Instance.GetFlatForwardDirection() * 2f - Head.right * 0.8f - Head.up * 1.5f;
                hpTarget.position = Vector3.SmoothDamp(hpTarget.position, targetPosition, ref hpVelocity, smoothTime);
                hp.position = hpTarget.position;

                var smoothHUDRotation = hpTarget.rotation.SmoothDamp(Quaternion.LookRotation(hp.position - (Head.position - Head.right * 0.4f)), ref hpDeriv, smoothTime);
                hpTarget.rotation = smoothHUDRotation;
                hp.rotation = hpTarget.rotation;
                //hp.rotation = Quaternion.LookRotation(hp.position - Head.position);
            }

            // 残弹量显示在枪旁
            if (curweapon)
            {
                curweapon.position = RightHand.model.position - RightHand.model.right * 0.1f + RightHand.model.forward * 0.1f - RightHand.model.up * 0.1f;
                curweapon.rotation = RightHand.model.rotation;
            }

            // 技能显示在左手旁
            if (Fastmove_tips && button_grenade)
            {
                Fastmove_tips.position = LeftHand.model.position + LeftHand.model.right * 0.1f + LeftHand.model.up * -0.05f + LeftHand.model.forward * -0.1f;
                Fastmove_tips.rotation = LeftHand.model.rotation;
                button_grenade.position = LeftHand.model.position + LeftHand.model.right * 0.095f + LeftHand.model.up * -0.085f + LeftHand.model.forward * -0.1f;
                button_grenade.rotation = LeftHand.model.rotation;
            }

            // 武器技能显示在右手旁
            if (button_aim)
            {
                button_aim.position = RightHand.model.position - RightHand.model.right * 0.07f - RightHand.model.forward * 0.15f - RightHand.model.up * 0.125f;
                button_aim.rotation = RightHand.model.rotation;
                if(button_aim.localScale == Vector3.one)
                    button_aim.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            }

            // 主武器准心显示在右手射线瞄准处
            if (rightCrossHair)
            {
                rightCrossHair.position = RightHand.GetRayHitPosition(8);
                rightCrossHair.rotation = Quaternion.LookRotation(rightCrossHair.position - RightHand.muzzle.position);
            }
            // 副武器准心显示在左手射线瞄准处（仅限狗的双持）
            if (leftCrossHair)
            {
                leftCrossHair.position = LeftHand.GetRayHitPosition(8);
                leftCrossHair.rotation = Quaternion.LookRotation(leftCrossHair.position - LeftHand.muzzle.position);
            }

            // 狐狸的蓄力技能准心需要额外处理
            if (foxChargingAim)
            {
                foxChargingAim.position = LeftHand.GetRayHitPosition(8);
                foxChargingAim.rotation = Quaternion.LookRotation(foxChargingAim.position - LeftHand.muzzle.position);
            }

            // 手雷投掷抛物线需要一些偏移
            if (SimLineStart)
            {
                SimLineStart.position = Head.position + Head.right * -0.3f + Head.up * 0.1f;
            }

            // 目标点需要重置一下朝向
            if (!Target_tips && PC_Panel_war) {
                Target_tips = PC_Panel_war.Find("Target_tips(Clone)");
            }
            if (Target_tips)
            {
                if (Target_tips.parent != PC_Panel_war)
                {
                    Target_tips = null;
                    Shockwave1 = null;
                    Shockwave2 = null;
                    img_target = null;
                    img_chest = null;
                    img_buffnpc = null;
                    img_shop = null;
                    img_weaponshop = null;
                    img_gscashshop = null;
                    img_firepile = null;
                }
                else
                {
                    if (!Shockwave1)
                        Shockwave1 = Target_tips.Find("UICourse_ponit/position");
                    if (!Shockwave2)
                        Shockwave2 = Target_tips.Find("UICourse_ponit (1)/position");
                    if (Shockwave1)
                        Shockwave1.gameObject.active = false;
                    if (Shockwave2)
                        Shockwave2.gameObject.active = false;

                    // Resize target tips labels
                    if (!img_target)
                        img_target = Target_tips.Find("img_target");
                        img_target.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                    if (!img_chest)
                        img_chest = Target_tips.Find("img_chest");
                        img_chest.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                    if (!img_buffnpc)
                        img_buffnpc = Target_tips.Find("img_buffnpc");
                        img_buffnpc.localScale = new Vector3(0.009f, 0.009f, 0.009f);
                    if (!img_shop)
                        img_shop = Target_tips.Find("img_shop");
                        img_shop.localScale = new Vector3(0.009f, 0.009f, 0.009f);
                    if (!img_weaponshop)
                        img_weaponshop = Target_tips.Find("img_weaponshop");
                        img_weaponshop.localScale = new Vector3(0.009f, 0.009f, 0.009f);
                    if (!img_gscashshop)
                        img_gscashshop = Target_tips.Find("img_gscashshop");
                        img_gscashshop.localScale = new Vector3(0.009f, 0.009f, 0.009f);
                    if (!img_firepile)
                        img_firepile = Target_tips.Find("img_firepile");
                        img_firepile.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                    Target_tips.localRotation = Quaternion.identity;
                }
            }

            //if (hero_skill_1)
            //{
            //    var parent = hero_skill_1.parent;
            //    hero_skill_1.parent = RightHand;
            //    hero_skill_1.localPosition = new Vector3(-0.5f, 0f, 0.5f);
            //    hero_skill_1.localEulerAngles = new Vector3(0f, 0f, 0f);
            //    hero_skill_1.parent = parent;
            //}

            if (HeroCameraManager.HeroObj.PlayerCom.CurWeaponSID == 1417)
            {
                var obj = GameObject.Find("Panel_Sight_1417_9600(Clone)");
                if (obj != null)
                    doomFistInterface = obj.transform;
            }

            if (doomFistInterface)
            {
                doomFistInterface.localRotation = Quaternion.identity;
            }
            else 

            if (!rightCrossHair)
                UpdateCrossHair();
        }

    }
}
