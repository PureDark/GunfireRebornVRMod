using HarmonyLib;
using UI;
using UnityEngine;
using VRMod.Player;
using VRMod.UI;
using GameCoder.Engine;
using VRMod.Core;

namespace VRMod.Patches
{
    //[HarmonyPatch(typeof(DYResourceManager), nameof(DYResourceManager.Load))]
    //internal class InjectDYResourceManagerLoad
    //{
    //    private static void Postfix(ref Object __result, Il2CppSystem.Type systemTypeInstance)
    //    {
    //        //if (__result)
    //        //{
    //        //    if(systemTypeInstance == Il2CppType.Of<GameObject>())
    //        //    {
    //        //        Log.Info("result.name = " + __result.name);
    //        //        var renderers = __result.TryCast<GameObject>().GetComponentsInChildren<Renderer>(true);
    //        //        ShaderFixer.FixRenderers(renderers);
    //        //    }
    //        //}
    //    }
    //}


}
