using BoltBehavior;
using DYPublic.Duonet;
using GameCoder.Engine;
using HarmonyLib;
using SkillBolt;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UI;
using UIScript;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using VRMod.Player;
using VRMod.UI;
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

        [HarmonyPatch(typeof(DYSceneManager), nameof(DYSceneManager.OnSceneLoaded))]
        internal class InjectDYSceneManager
        {
            private static void Postfix(DYSceneManager __instance, Scene scene, LoadSceneMode mode)
            {
                Log.Info("Scene Loaded: " + scene.name);
                if (scene.name?.ToLower() == "home")
                {
                    //进入home要修复UI，改造为适合VR的方式
                    MenuFix.HomeFix();
                }

                if (onSceneLoaded != null)
                    onSceneLoaded.Invoke(scene, mode);
            }
        }

        [HarmonyPatch(typeof(CUIManager), nameof(CUIManager.Init))]
        internal class InjectVRStart
        {
            private static void Postfix()
            {
                if (!VRSystems.Instance)
                {
                    //VR相关的入口，在CUIManager初始化时进行注入
                    new GameObject("VR_Globals").AddComponent<VRSystems>();
                    MenuFix.Prefix();
                }
            }
        }

        public static List<string> UIModePathes = new List<string>
        {
            UIFormName.HOME_D_PANEL,
            UIFormName.DAYCHALLENGE_PANEl,
            UIFormName.CHARATER_PANEL,
            UIFormName.PC_Panel_HeroState,
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
            "PC_Panel_teaminfo",
            "PC_panel_dorpweaponcontrast"
        };

        public static List<string> HideUIModePathes = new List<string>
        {
            "PC_Panel_teaminfo",
            "PC_panel_dorpweaponcontrast"
        };

        [HarmonyPatch(typeof(CUIManager), nameof(CUIManager.showUI))]
        internal class InjectShowUI
        {
            private static void Postfix(string uiFormPath, bool playAni)
            {
                //Log.Info("InjectShowUI: uiFormPath=" + uiFormPath);
                if (UIModePathes.Contains(uiFormPath) && VRPlayer.Instance)
                    VRPlayer.Instance.SetUIMode(true);
            }
        }

        [HarmonyPatch(typeof(CUIManager), nameof(CUIManager.hideUI))]
        internal class InjectHideUI
        {
            private static void Postfix(string uiFormPath)
            {
                if (HideUIModePathes.Contains(uiFormPath) && VRPlayer.Instance)
                    VRPlayer.Instance.SetUIMode(false);
            }
        }

        [HarmonyPatch(typeof(s2cnetwar), nameof(s2cnetwar.GS2CWarPaused))]
        internal class InjectGS2CWarPaused
        {
            private static void Postfix(s2cnetwar_GS2CWarPausedClass data)
            {
                if (VRPlayer.Instance)
                    VRPlayer.Instance.SetUIMode(data.iPaused==1);
            }
        }

        // 禁用原版的狙击开镜action
        [HarmonyPatch(typeof(CartoonAction700), nameof(CartoonAction700.Trigger))]
        internal class InjectSniperZoomingAction
        {
            private static bool Prefix(BehaviorNode node)
            {
                return false;
            }
        }

        //[HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.Behav_HideWeaponExceptMuzzle))]
        //internal class InjectBehav_HideWeaponExceptMuzzle
        //{
        //    private static bool Prefix(BehaviorNode node)
        //    {
        //        return false;
        //    }
        //}

        #region UI效果修改

        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.CreateOnceUIEffect))]
        internal class InjectCreateOnceUIEffect
        {
            private static void Postfix(BehaviorNode node, Object original, float livetime, float deletedelay)
            {
                //Log.Info("InjectCreateOnceUIEffect: original.name=" + original.name + "  livetime=" + livetime + "  deletedelay=" + deletedelay);
                if (original.name == "hub_die(Clone)")
                    original.Cast<Transform>().localEulerAngles = Vector3.zero;
            }
        }

        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.CreateUIEffect))]
        internal class InjectCreateUIEffect
        {
            private static void Postfix(BehaviorNode node, Object original, string effectname = "")
            {
                //Log.Info("InjectCreateUIEffect: original.name=" + original.name + "  effectname=" + effectname);
                original.Cast<Transform>().localEulerAngles = Vector3.zero;
            }
        }

        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.CreateEffect))]
        internal class InjectCreateEffect
        {
            private static void Postfix(BehaviorNode node, Object original, Define.POSITION_TYPE posType, Define.TARGET_TYPE targetType, string parent = "", bool createOnce = false, string effectname = "", bool isHeroVoiceSwitch = false)
            {
                //Log.Info("InjectCreateEffect: original.name=" + original.name + "  effectname=" + effectname + "  parent=" + parent);
                if (original.name == "HeroSkill_1301_Caster(Clone)" && VRPlayer.Instance)
                {
                    VRPlayer.Instance.SetDualWield(original.Cast<Transform>());
                }
            }
        }

        #endregion

        //[HarmonyPatch(typeof(Game.CUnityUtility), nameof(Game.CUnityUtility.RayCastByScreenCenterPos))]
        //internal class InjectRayCastByScreenCenterPos
        //{
        //    private static void Prefix(Camera camera, float dis, int mask, int filterMask)
        //    {
        //        //Log.Info("InjectRayCastByScreenCenterPos: camera.name=" + camera.name + "  dis=" + dis + "  mask=" + mask + "  filterMask=" + filterMask);
        //    }
        //}

        #region 双持射线检测

        // 狗的双持需要特别注入来修改用来检测的射线

        public static bool isCrtArgSightAccPos = false;
        public static bool isRightRay = false;

        [HarmonyPatch(typeof(CArgBase), nameof(CArgBase.CrtArgSightAccPos))]
        internal class InjectCrtArgSightAccPos
        {
            private static void Prefix(CSkillBase skill, CCartoonBase cartoon)
            {
                Log.Info("InjectCrtArgSightAccPos: skill.WeaponTran.name=" + skill.WeaponTran.name + "  cartoon.animatorTrans.name=" + cartoon.animatorTrans);
                if (VRPlayer.Instance.isDualWield)
                {
                    isCrtArgSightAccPos = true;
                    isRightRay = (skill.WeaponTran.name == HeroCtrlMgr.HeroObj.PlayerCom.GetCurWeaponTran(HeroCtrlMgr.HeroObj.PlayerCom.CurWeaponID).name);
                }
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
                    Log.Info("InjectGetRayByScreenPos: isCrtArgSightAccPos=" + isCrtArgSightAccPos + "  isRightRay=" + isRightRay);
                    return false;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(CArgBase), nameof(CArgBase.CrtArgSightFakerAnglePos))]
        internal class InjectCrtArgSightFakerAnglePos
        {
            private static void Prefix(CSkillBase skill, CCartoonBase cartoon)
            {
                Log.Info("InjectCrtArgSightFakerAnglePos: skill.WeaponTran.name=" + skill.WeaponTran.name + "  cartoon.animatorTrans.name=" + cartoon.animatorTrans);
            }
        }


        public static bool isCrtArgCameraCenterPos = false;

        [HarmonyPatch(typeof(CArgBase), nameof(CArgBase.CrtArgCameraCenterPos))]
        internal class InjectCrtArgCameraCenterPos
        {
            private static void Prefix(CSkillBase skill)
            {
                if(skill.WeaponTran!= null)
                {
                    Log.Info("InjectCrtArgCameraCenterPos: skill.WeaponTran.name=" + skill.WeaponTran.name);
                    isCrtArgCameraCenterPos = true;
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
                    Log.Info("InjectGetCameraCenterRay: isCrtArgCameraCenterPos=" + isCrtArgCameraCenterPos + "  isRightRay=" + isRightRay);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(HeroWarSign.SightLineSign), nameof(HeroWarSign.SightLineSign.CrtArgSightAccPos))]
        internal class InjectSightLineSign
        {
            private static void Prefix(CSkillBase skill)
            {
                Log.Info("InjectSightLineSign: skill.WeaponTran.name=" + skill.WeaponTran.name);
            }
        }


        public static bool isCastingRayCartoon = false;

        [HarmonyPatch(typeof(CastingRayCartoon), nameof(CastingRayCartoon.UpdateRay))]
        internal class InjectUpdateRay
        {
            private static void Prefix(CSkillBase skill)
            {
                if (skill.WeaponTran != null)
                {
                    Log.Info("InjectUpdateRay: skill.WeaponTran.name=" + skill.WeaponTran.name);
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
                    Log.Info("InjectGetRayByScreenCenterPos: isCastingRayCartoon=" + isCastingRayCartoon + "  isRightRay=" + isRightRay);
                    return false;
                }
                return true;
            }
        }

        #endregion




        [HarmonyPatch(typeof(BezierLineRenderer), nameof(BezierLineRenderer.Awake))]
        internal class InjectBezierLineRendererAwake
        {
            private static void Postfix(BezierLineRenderer __instance)
            {
                VRPlayer.Instance.bezierLineRenderers.Add(__instance);
            }
        }

        [HarmonyPatch(typeof(BezierLineRenderer), nameof(BezierLineRenderer.Update))]
        internal class InjectBezierLineRendererUpdate
        {
            private static bool Prefix()
            {
                return false;
            }
        }




        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.ActiveCGCamera))]
        internal class InjectActiveCGCamera
        {
            private static void Prefix(BehaviorNode node, string cgname, bool isactive)
            {
                Log.Info("ActiveCGCamera");
                if (node != null && node.Own != null)
                    Log.Info("ActiveCGCamera: " + node.Own.name);

                Log.Info("ActiveCGCamera: cgname=" + cgname);
                Log.Info("ActiveCGCamera: original.name=" + isactive);
            }
        }

        [HarmonyPatch(typeof(CBehaviorAction), nameof(CBehaviorAction.CreateCGCamera))]
        internal class InjectCreateCGCamera
        {
            private static void Prefix(BehaviorNode node, Object original, string cgname, Vector3 stratpos)
            {
                Log.Info("CreateCGCamera");
                if (node!=null&& node.Own!=null)
                    Log.Info("CreateCGCamera: " + node.Own.name);

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
                Log.Info("DestoryCGCamera");
                if (node != null && node.Own != null)
                    Log.Info("DestoryCGCamera: " + node.Own.name);
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
                    if (systemTypeInstance == Il2CppType.Of<GameObject>())
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
    }
}
