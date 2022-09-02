using GameCoder.Engine;
using HarmonyLib;
using System.Collections;
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

        //[HarmonyPatch(typeof(CUIManager), nameof(CUIManager.SetWaterImage))]
        //internal class InjectWaterImage
        //{
        //    private static bool Prefix(ref bool active)
        //    {
        //        active = false;
        //        return true;
        //    }
        //}

        [HarmonyPatch(typeof(CUIManager), nameof(CUIManager.SetUIEffectParent))]
        internal class InjectSetUIEffectParent
        {
            private static void Postfix(Transform effect)
            {
                Log.Info("InjectSetUIEffectParent : " + effect.name);
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
