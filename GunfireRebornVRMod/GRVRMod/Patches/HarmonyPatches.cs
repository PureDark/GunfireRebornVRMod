using BoltBehavior;
using GameCoder.Engine;
using HarmonyLib;
using SkillBolt;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UI;
using UIScript;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using VRMod.Player;
using VRMod.Player.VRInput;
using VRMod.UI;
using static UnityEngine.UI.Image;
using static VRMod.VRMod;

namespace VRMod.Patches
{
    internal static class HarmonyPatches
    {
        public static Harmony Instance { get; set; }

        public delegate void OnSceneLoadedEvent(Scene scene, LoadSceneMode mode);
        public static OnSceneLoadedEvent onSceneLoaded;

        public static void PatchAll()
        {
            if(Instance == null)
                Instance = new Harmony("com.PureDark.GRVRMod");
            Instance.PatchAll();
            Instance.Patch(
                typeof(UnhollowerBaseLib.LogSupport).GetMethod("Warning", AccessTools.all),
                new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(HarmonyPatches.UnhollowerWarningPrefix))));
        }

        public static bool DontRunMe() => false;

        public static bool UnhollowerWarningPrefix(string __0) => !__0.Contains("unsupported return type") && !__0.Contains("unsupported parameter");

        //[HarmonyPatch(typeof(CUIManager), nameof(CUIManager.Init))]
        //internal class InjectVRStart
        //{
        //    private static void Postfix()
        //    {
        //        if (!VRSystems.Instance)
        //        {
        //            //VR相关的入口，在CUIManager初始化时进行注入
        //            new GameObject("VR_Globals").AddComponent<VRSystems>();
        //            MenuFix.Prefix();
        //        }
        //    }
        //}

        #region UI界面开启关闭

        public static List<string> UIModePathes = new List<string>
        {
            UIFormName.HOME_D_PANEL,
            UIFormName.DAYCHALLENGE_PANEl,
            UIFormName.CHARATER_PANEL,
            UIFormName.SETTING_PANEL,
            UIFormName.COLLECTION_PCPANEL,
            UIFormName.TEAM_PANEL,
            UIFormName.FRIEND_PANEL,
            UIFormName.INVITE_PANEL,
            UIFormName.PCESCSETTING_PANEL,
            UIFormName.TALENT_CHOOSE,
            UIFormName.TALENT_CHOOSE_LEVEL4,
            UIFormName.WAR_SHOP_PANEL,
            UIFormName.CASH_SHOP_PANEL,
            UIFormName.INSCIPRTION_SHOP,
            UIFormName.WAREND_PANEL,
            UIFormName.PC_CashBuffChoose_Panel,
            UIFormName.PC_Panel_teaminfo,
            UIFormName.PACKAGE_PANEL,
            UIFormName.WARDROPWEAPONCONTAST,
            UIFormName.NPC_EVENT_PANAL,
            UIFormName.NPC_EVENT_WEAPON_PANEL,
            UIFormName.NPC_EVENT_CHOOSE_PANEL,
            UIFormName.ASK_RESURGENCE_PANEL,
            UIFormName.ASK_RELICRESURGENCE_PANEL,
            UIFormName.PC_PANEL_GROWTHCHOOSE,
            UIFormName.PACKAGE_PANEL
        };

        public static List<string> BattleModePathes = new List<string>
        {
            UIFormName.PC_Panel_HeroState,
            UIFormName.PANEL_CRAZE,
            UIFormName.WAR_PANEL
        };

        [HarmonyPatch(typeof(CUIManager), nameof(CUIManager.showUI))]
        internal class InjectShowUI
        {
            private static void Postfix(string uiFormPath, bool playAni)
            {
                Log.Info("InjectShowUI: uiFormPath=" + uiFormPath);
                if (VRPlayer.Instance)
                {
                    if (UIModePathes.Contains(uiFormPath))
                        VRPlayer.Instance.SetUIMode(true);
                    else if (BattleModePathes.Contains(uiFormPath))
                        VRPlayer.Instance.SetUIMode(false);
                    if (uiFormPath == UIFormName.WAREND_PANEL)
                    {
                        var winTextGO = CUIManager.instance.MainPopUpCanvas.transform.DeepFindChild("arttext_Win_1");
                        if (winTextGO)
                            Object.Destroy(winTextGO.gameObject);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CUIManager), nameof(CUIManager.hideUI))]
        internal class InjectHideUI
        {
            private static void Postfix(string uiFormPath)
            {
                Log.Info("InjectHideUI: uiFormPath=" + uiFormPath);
                if (UIModePathes.Contains(uiFormPath) && uiFormPath != UIFormName.ASK_RESURGENCE_PANEL && VRPlayer.Instance && !VRPlayer.Instance.isHome)
                    VRPlayer.Instance.SetUIMode(false);
            }
        }

        //[HarmonyPatch(typeof(s2cnetwar), nameof(s2cnetwar.GS2CWarPaused))]
        //internal class InjectGS2CWarPaused
        //{
        //    private static void Postfix(s2cnetwar_GS2CWarPausedClass data)
        //    {
        //        if (VRPlayer.Instance)
        //            VRPlayer.Instance.SetUIMode(data.iPaused == 1);
        //    }
        //}

        // 禁用原版的狙击开镜action
        [HarmonyPatch(typeof(CartoonAction700), nameof(CartoonAction700.Trigger))]
        internal class InjectSniperZoomingAction
        {
            private static bool Prefix(BehaviorNode node)
            {
                return false;
            }
        }

        #endregion

        #region UI效果修改

        [HarmonyPatch(typeof(DYSceneManager), nameof(DYSceneManager.OnSceneLoaded))]
        internal class InjectDYSceneManager
        {
            private static void Postfix(DYSceneManager __instance, Scene scene, LoadSceneMode mode)
            {
                Log.Info("Scene Loaded: " + scene.name);
                if (onSceneLoaded != null)
                    onSceneLoaded.Invoke(scene, mode);
                if (scene.name == "1030102")
                    VRPlayer.Instance.CannonFix = true;
                else
                    VRPlayer.Instance.CannonFix = false;
            }
        }

        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.CreateContiEffectOffset))]
        internal class InjectCreateCreateContiEffectOffset
        {
            private static void Postfix(BehaviorNode node, Object original, Define.POSITION_TYPE posType, Define.TARGET_TYPE targetType, Vector3 offset, string parent, bool createOnce, string effectname, bool isHeroVoiceSwitch, bool isNeedLimitScale, float scaleThres)
            {
                Log.Info("InjectCreateCreateContiEffectOffset: original.name=" + original.name + "  posType=" + posType + "  targetType=" + targetType + "  parent=" + parent + "  effectname=" + effectname);
                if (original.name == "60632")
                    original.Cast<Transform>().gameObject.active = false;
            }
        }

        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.CreateEffectOffSet))]
        internal class InjectCreateCreateEffectOffSet
        {
            private static void Postfix(BehaviorNode node, Object original, Define.POSITION_TYPE posType, Define.TARGET_TYPE targetType, float livetime, float deletedelay, string parent, Vector3 offSet)
            {
                Log.Info("InjectCreateCreateEffectOffSet: original.name=" + original.name + "  posType=" + posType + "  targetType=" + targetType + "  parent=" + parent + "  livetime=" + livetime);
                if (original.name == "60632")
                    original.Cast<Transform>().gameObject.active = false;
            }
        }

        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.CreateOnceEffect))]
        internal class InjectCreateCreateOnceEffect
        {
            private static void Postfix(BehaviorNode node, Object original, Define.POSITION_TYPE posType, Define.TARGET_TYPE targetType, float livetime, float deletedelay, string parent)
            {
                Log.Info("InjectCreateCreateOnceEffect: original.name=" + original.name + "  posType=" + posType + "  targetType=" + targetType + "  parent=" + parent + "  livetime=" + livetime);
                if (original.name == "60632")
                    original.Cast<Transform>().gameObject.active = false;
            }
        }

        //修正死亡画面不面向玩家
        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.CreateOnceUIEffect))]
        internal class InjectCreateOnceUIEffect
        {
            private static void Postfix(BehaviorNode node, Object original, float livetime, float deletedelay)
            {
                Log.Info("InjectCreateOnceUIEffect: original.name=" + original.name + "  livetime=" + livetime + "  deletedelay=" + deletedelay);
                if (original.name == "hub_die(Clone)")
                    original.Cast<Transform>().localEulerAngles = Vector3.zero;
                if (original.name == "60632")
                    original.Cast<Transform>().gameObject.active = false;
            }
        }

        //修正所有界面角度错误
        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.CreateUIEffect))]
        internal class InjectCreateUIEffect
        {
            private static void Postfix(BehaviorNode node, Object original, string effectname = "")
            {
                Log.Info("InjectCreateUIEffect: original.name=" + original.name + "  effectname=" + effectname);
                original.Cast<Transform>().localEulerAngles = Vector3.zero;
                //if (original.name == "0" && effectname == "shieldbreak")
                //    original.Cast<Transform>().Find("postion").gameObject.active = false;
                if (original.name == "60632")
                    original.Cast<Transform>().gameObject.active = false;
            }
        }

        // 让血条显示到怪物上方而不是canvas上
        [HarmonyPatch(typeof(OCBloodBar), nameof(OCBloodBar.UpdatePos))]
        internal class InjectOCBloodBarUpdatePos
        {
            private static bool Prefix(OCBloodBar __instance)
            {
                __instance.m_UpdatePos = __instance.m_CalposTran.position;
                return false;
            }
        }

        [HarmonyPatch(typeof(OCBloodBar), nameof(OCBloodBar.LateUpdate))]
        internal class InjectOCBloodBarLateUpdate
        {
            private static void Postfix(OCBloodBar __instance)
            {
                if (__instance.isShowBloodBar)
                {
                    __instance.hpbartrans.position = __instance.m_UpdatePos;
                    if (__instance.BloodBar.ARTrans)
                        __instance.BloodBar.ARTrans.localRotation = Quaternion.identity;
                    if (__instance.BloodBar.SHTrans)
                        __instance.BloodBar.SHTrans.localRotation = Quaternion.identity;
                    if (__instance.BloodScale)
                        __instance.BloodScale.m_RealMaxScale = 3;
                }
            }
        }

        // 取消新更新的血条受伤特效
        [HarmonyPatch(typeof(BarHurtEffectController), nameof(BarHurtEffectController.AddHurtEffect))]
        internal class InjectBarHurtEffectControllerAddHurtEffect
        {
            private static bool Prefix(int aniType)
            {
                return false;
            }
        }

        // 取消护盾回复时的特效，因为无法缩放会导致整个屏幕闪一下
        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.CreateEffectOnUINode))]
        internal class InjectCreateEffectOnUINode
        {
            private static void Postfix(BehaviorNode node, string uiname, string nodename, Object original, float livetime = 0f)
            {
                Log.Info("CreateEffectOnUINode: original.name=" + original.name + "  uiname=" + uiname + "  nodename=" + nodename + "  livetime=" + livetime);
                if (original.name == "60632")
                    original.Cast<Transform>().gameObject.active = false;
                if (original.name == "0" && uiname == "PanelWar" && nodename == "hp")
                    original.Cast<Transform>().Find("postion").gameObject.active = false;
            }
        }


        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.CreateOnceEffectOnUINode))]
        internal class InjectCreateOnceEffectOnUINode
        {
            private static void Postfix(BehaviorNode node, string uiname, string nodename, Object original, float livetime, float deletedelay)
            {
                Log.Info("InjectCreateOnceEffectOnUINode: original.name=" + original.name + "  uiname=" + uiname + "  nodename=" + nodename + "  livetime=" + livetime + "  deletedelay=" + deletedelay);
                if (original.name == "60632")
                    original.Cast<Transform>().gameObject.active = false;
                if (original.name == "shieldrecover_206_UIHub(Clone)" && uiname == "PanelWar" && nodename == "hp")
                    original.Cast<Transform>().Find("postion").gameObject.active = false;
            }
        }


        [HarmonyPatch(typeof(PC_Home_Panel_logic), nameof(PC_Home_Panel_logic.OnMonsterInfo))]
        internal class InjectMainMenuCollection
        {
            private static void Postfix()
            {
                //修复点图鉴后粒子特效无法缩小导致闪爆屏幕的问题
                MenuFix.FixCollectionMenu();
            }
        }

        [HarmonyPatch(typeof(PCCharaterPanelManager), nameof(PCCharaterPanelManager.ShowCharaterPanel))]
        internal class InjectPCCharaterPanelManager
        {
            private static void Postfix()
            {
                //修复选人界面的粒子特效朝向
                MenuFix.FixCharacterMenu();
            }
        }

        #endregion


        #region 双持相关修改/射线检测

        // 通过双持UI的启用来判定是否开启双持模式
        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.CreateEffect))]
        internal class InjectCreateEffect
        {
            private static void Postfix(BehaviorNode node, Object original, Define.POSITION_TYPE posType, Define.TARGET_TYPE targetType, string parent = "", bool createOnce = false, string effectname = "", bool isHeroVoiceSwitch = false)
            {
                Log.Info("InjectCreateEffect: original.name=" + original.name + "  effectname=" + effectname + "  parent=" + parent);
                if (original.name == "60632")
                    original.Cast<Transform>().gameObject.active = false;
                if (original.name == "HeroSkill_1301_Caster(Clone)" && VRPlayer.Instance)
                {
                    VRPlayer.Instance.SetDualWield(original.Cast<Transform>());
                }
            }
        }

        //[HarmonyPatch(typeof(Game.CUnityUtility), nameof(Game.CUnityUtility.RayCastByScreenCenterPos))]
        //internal class InjectRayCastByScreenCenterPos
        //{
        //    private static void Prefix(Camera camera, float dis, int mask, int filterMask)
        //    {
        //        Log.Info("InjectRayCastByScreenCenterPos: camera.name=" + camera.name + "  dis=" + dis + "  mask=" + mask + "  filterMask=" + filterMask);
        //    }
        //}

        // 狗的双持需要特别注入来修改用来检测的射线

        public static bool isCrtArgSightAccPos = false;
        public static bool isRightRay = false;

        [HarmonyPatch(typeof(CArgBase), nameof(CArgBase.CrtArgSightAccPos))]
        internal class InjectCrtArgSightAccPos
        {
            private static void Prefix(CSkillBase skill, CCartoonBase cartoon)
            {
                //Log.Info("InjectCrtArgSightAccPos: skill.WeaponTran.name=" + skill.WeaponTran.name + "  cartoon.animatorTrans.name=" + cartoon.animatorTrans);
                isCrtArgSightAccPos = true;
                isRightRay = (skill.WeaponTran.name == HeroCtrlMgr.HeroObj.PlayerCom.GetCurWeaponTran(HeroCtrlMgr.HeroObj.PlayerCom.CurWeaponID).name);
            }
            private static void Postfix()
            {
                isCrtArgSightAccPos = false;
            }
        }

        [HarmonyPatch(typeof(Game.CUnityUtility), nameof(Game.CUnityUtility.GetRayByScreenPos))]
        internal class InjectGetRayByScreenPos
        {
            private static bool Prefix(ref Ray __result, Camera camera, float offsetx, float offsety)
            {
                //Log.Info("InjectGetRayByScreenPos: camera.name=" + camera.name + "  offsetx=" + offsetx + "  offsety=" + offsety);
                if (isCrtArgSightAccPos)
                {
                    __result = isRightRay ? VRPlayer.Instance.RightHand.aimRay : VRPlayer.Instance.LeftHand.aimRay;
                    if(isRightRay)
                        VRInputManager.Instance.VibrateRight(0.1f);
                    else
                        VRInputManager.Instance.VibrateLeft(0.1f);
                    //Log.Info("InjectGetRayByScreenPos: isCrtArgSightAccPos=" + isCrtArgSightAccPos + "  isRightRay=" + isRightRay);
                    return false;
                }
                return true;
            }
        }


        //[HarmonyPatch(typeof(CArgBase), nameof(CArgBase.CrtArgSightFakerAnglePos))]
        //internal class InjectCrtArgSightFakerAnglePos
        //{
        //    private static void Prefix(CSkillBase skill, CCartoonBase cartoon)
        //    {
        //        Log.Info("InjectCrtArgSightFakerAnglePos: skill.WeaponTran.name=" + skill.WeaponTran.name + "  cartoon.animatorTrans.name=" + cartoon.animatorTrans);
        //    }
        //}


        public static bool isCrtArgCameraCenterPos = false;

        [HarmonyPatch(typeof(CArgBase), nameof(CArgBase.CrtArgCameraCenterPos))]
        internal class InjectCrtArgCameraCenterPos
        {
            private static void Prefix(CSkillBase skill)
            {
                if(skill.WeaponTran!= null)
                {
                    //Log.Info("InjectCrtArgCameraCenterPos: skill.WeaponTran.name=" + skill.WeaponTran.name);
                    isCrtArgCameraCenterPos = true;
                    if (isRightRay)
                        VRInputManager.Instance.VibrateRight(0.3f);
                    else
                        VRInputManager.Instance.VibrateLeft(0.3f);
                    isRightRay = (skill.WeaponTran.name == HeroCtrlMgr.HeroObj.PlayerCom.GetCurWeaponTran(HeroCtrlMgr.HeroObj.PlayerCom.CurWeaponID).name);
                }
            }
            private static void Postfix()
            {
                isCrtArgCameraCenterPos = false;
            }
        }

        [HarmonyPatch(typeof(SkillFunction), nameof(SkillFunction.GetCameraCenterRay))]
        internal class InjectGetCameraCenterRay
        {
            private static bool Prefix(ref Ray __result)
            {
                //Log.Info("InjectGetCameraCenterRay");
                if (isCrtArgCameraCenterPos)
                {
                    __result = isRightRay ? VRPlayer.Instance.RightHand.aimRay : VRPlayer.Instance.LeftHand.aimRay;
                    //Log.Info("InjectGetCameraCenterRay: isCrtArgCameraCenterPos=" + isCrtArgCameraCenterPos + "  isRightRay=" + isRightRay);
                    return false;
                }
                return true;
            }
        }

        //[HarmonyPatch(typeof(HeroWarSign.SightLineSign), nameof(HeroWarSign.SightLineSign.CrtArgSightAccPos))]
        //internal class InjectSightLineSign
        //{
        //    private static void Prefix(CSkillBase skill)
        //    {
        //        Log.Info("InjectSightLineSign: skill.WeaponTran.name=" + skill.WeaponTran.name);
        //    }
        //}


        public static bool isCastingRayCartoon = false;

        [HarmonyPatch(typeof(CastingRayCartoon), nameof(CastingRayCartoon.UpdateRay))]
        internal class InjectUpdateRay
        {
            private static void Prefix(CSkillBase skill)
            {
                if (skill.WeaponTran != null)
                {
                    //Log.Info("InjectUpdateRay: skill.WeaponTran.name=" + skill.WeaponTran.name);
                    isCastingRayCartoon = true;
                    isRightRay = (skill.WeaponTran.name == HeroCtrlMgr.HeroObj.PlayerCom.GetCurWeaponTran(HeroCtrlMgr.HeroObj.PlayerCom.CurWeaponID).name);
                }
            }
            private static void Postfix()
            {
                isCastingRayCartoon = false;
            }
        }

        [HarmonyPatch(typeof(Game.CUnityUtility), nameof(Game.CUnityUtility.GetRayByScreenCenterPos))]
        internal class InjectGetRayByScreenCenterPos
        {
            private static bool Prefix(ref Ray __result, Camera camera)
            {
                //Log.Info("InjectGetRayByScreenCenterPos: camera.name=" + camera.name + "  offsetx=" + offsetx + "  offsety=" + offsety);
                if (isCastingRayCartoon)
                {
                    __result = isRightRay ? VRPlayer.Instance.RightHand.aimRay : VRPlayer.Instance.LeftHand.aimRay;
                    //Log.Info("InjectGetRayByScreenCenterPos: isCastingRayCartoon=" + isCastingRayCartoon + "  isRightRay=" + isRightRay);
                    return false;
                }
                return true;
            }
        }

        #endregion

        #region 彩虹射线修复

        //[HarmonyPatch(typeof(BezierLineRenderer), nameof(BezierLineRenderer.Awake))]
        //internal class InjectBezierLineRendererAwake
        //{
        //    private static void Postfix(BezierLineRenderer __instance)
        //    {
        //        VRPlayer.Instance.bezierLineRenderers.Add(__instance);
        //    }
        //}

        //[HarmonyPatch(typeof(BezierLineRenderer), nameof(BezierLineRenderer.Update))]
        //internal class InjectBezierLineRendererUpdate
        //{
        //    private static bool Prefix()
        //    {
        //        return false;
        //    }
        //}

        #endregion

        #region 手套射线修复

        [HarmonyPatch(typeof(RayLineLaser), nameof(RayLineLaser.Awake))]
        internal class InjectRayLineLaserAwake
        {
            private static void Postfix(RayLineLaser __instance)
            {
                VRPlayer.Instance.rayLineLasers.Add(__instance);
            }
        }

        [HarmonyPatch(typeof(RayLineLaser), nameof(RayLineLaser.Update))]
        internal class InjectRayLineLaserUpdate
        {
            private static bool Prefix()
            {
                return VRPlayer.Instance.allowUpdateRayLineLaser;
            }
        }

        #endregion

        #region CG相机修复
        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.ActiveCGCamera))]
        internal class InjectActiveCGCamera
        {
            private static void Prefix(BehaviorNode node, string cgname, bool isactive)
            {
                Log.Info("ActiveCGCamera: cgname=" + cgname + " isactive=" + isactive);
            }
        }

        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.CreateCGCamera))]
        internal class InjectCreateCGCamera
        {
            private static void Prefix(BehaviorNode node, Object original, string cgname, Vector3 stratpos)
            {
                Camera cam = original.Cast<Transform>().GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    cam.stereoTargetEye = StereoTargetEyeMask.None;
                    cam.enabled = false;
                    VRPlayer.Instance.SetCGCamera(true, cam);
                }
                Log.Info("CreateCGCamera: cgname=" + cgname);

                if (original != null)
                    Log.Info("CreateCGCamera: original.name=" + original.name);
            }
        }

        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.DestoryCGCamera))]
        internal class InjectDestoryCGCamera
        {
            private static void Prefix(BehaviorNode node)
            {
                VRPlayer.Instance.SetCGCamera(false);
                Log.Info("DestoryCGCamera");
                if (node != null && node.Own != null)
                    Log.Info("DestoryCGCamera: " + node.Own.name);
            }
        }

        #endregion

        #region 教程相机修复

        [HarmonyPatch(typeof(GuideManager), nameof(GuideManager.CreateCGCamera))]
        internal class InjectGuideManagerCreateCGCamera
        {
            private static void Prefix(string cgname)
            {
                Transform cameraRoot = GuideManager.GetCameraRoot();
                Transform transform = cameraRoot.Find(cgname);
                Camera cam = transform.GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    cam.stereoTargetEye = StereoTargetEyeMask.None;
                    cam.enabled = false;
                    VRPlayer.Instance.SetCGCamera(true, cam);
                    Log.Info("GuideManagerCreateCGCamera: cgname=" + cgname);
                    VRSystems.Instance.SetGuideCanvas(GuideManager.GetGuideRoot().GetComponentInChildren<Canvas>());
                }
            }
        }

        [HarmonyPatch(typeof(GuideManager), nameof(GuideManager.CGCameraStopCallBack))]
        internal class InjectGuideManagerCGCameraStopCallBack
        {
            private static void Prefix()
            {
                VRPlayer.Instance.SetCGCamera(false);
            }
        }

        #endregion

        #region MISC

        [HarmonyPatch(typeof(CameraCtrl), nameof(CameraCtrl.Recoil))]
        internal class InjectCameraCtrl
        {
            private static bool Prefix()
            {
                //取消镜头晃动
                return false;
            }
        }

        [HarmonyPatch(typeof(DYResourceManager), nameof(DYResourceManager.Load))]
        internal class InjectDYResourceManagerLoad
        {
            private static void Postfix(ref Object __result, Il2CppSystem.Type systemTypeInstance)
            {
                // 开启VR模式时所有Camera都会默认变成Stereo，然后可能是因为IL2CPP的原因哪怕再改回None，相机也会定死在原地死活不随Transform移动
                // 必须在Unity接管前把stereoTargetEye改成None，才能解决
                if (__result)
                {
                    if (systemTypeInstance == UnhollowerRuntimeLib.Il2CppType.Of<GameObject>())
                    {
                        var cameras = __result.TryCast<GameObject>().GetComponentsInChildren<Camera>(true);
                        foreach (Camera c in cameras)
                        {
                            c.stereoTargetEye = StereoTargetEyeMask.None;
                            XRDevice.DisableAutoXRCameraTracking(c, true);
                        }
                    }
                }
            }
        }

        public static IEnumerator ClickStartScreenContinue()
        {
            yield return new WaitForSeconds(0.5f);
            //在VR里要帮玩家把开头的更新路线提示点掉，才能加载进酒馆主界面
            //CUIManager.instance.transform.Find("Canvas_PC(Clone)/MenuRoot")?.gameObject.GetComponentInChildren<M1Button>()?.onClick?.Invoke();
        }

        #endregion
    }
}
