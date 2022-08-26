using GameCoder.Engine;
using HarmonyLib;
using InControl;
using System.Collections;
using UI;
using UIScript;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRMod.Core;
using VRMod.Player;
using VRMod.UI;

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
                if (onSceneLoaded != null)
                {
                    onSceneLoaded.Invoke(scene, mode);
                }

                Log.Info("Scene Loaded: " + scene.name);
                if (scene.name?.ToLower() == "start")
                {
                    //很可惜注入后已经进入start场景了，这条永远不会被调用到
                    //ClickStartScreenContinue();
                }
                else if (scene.name?.ToLower() == "home")
                {
                    //首次进入home要修复UI，改造为适合VR的方式
                    MenuFix.Apply();
                }
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

        [HarmonyPatch(typeof(PC_Home_Panel_logic), nameof(PC_Home_Panel_logic.OnMonsterInfo))]
        internal class InjectMainMenuCollection
        {
            private static void Postfix()
            {
                //修复点图鉴后粒子特效无法缩小导致闪瞎屏幕的问题
                MenuFix.FixCollectionMenu();
            }
        }

        public static IEnumerator ClickStartScreenContinue()
        {
            yield return new WaitForSeconds(0.5f);
            //在VR里要帮玩家把开头的更新路线提示点掉，才能加载进酒馆主界面
            //CUIManager.instance.transform.Find("Canvas_PC(Clone)/MenuRoot")?.gameObject.GetComponentInChildren<M1Button>()?.onClick?.Invoke();
        }


        public static void SetLayerRecursively(this GameObject inst, int layer)
        {
            inst.layer = layer;
            int children = inst.transform.childCount;
            for (int i = 0; i < children; ++i)
                inst.transform.GetChild(i).gameObject.SetLayerRecursively(layer);
        }
    }
}
