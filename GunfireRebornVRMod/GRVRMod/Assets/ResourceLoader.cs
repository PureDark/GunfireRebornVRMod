using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace VRMod.Assets
{
    public static class ResourceLoader
    {
        private static AssetBundle assetBundle;
        private static Il2CppReferenceArray<Object> assets;

        public static AssetBundle LoadAssetBundle(string name)
        {
            assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/../../BepInEx/plugins//PureDark.VRMod/" + name);
            assets = assetBundle.LoadAllAssets();
            foreach (var asset in assets)
            {
                asset.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            return assetBundle;
        }

        public static T LoadAsset<T>(string name) where T : Object
        {
            foreach (var asset in assets)
            {
                if (asset.name == name)
                {
                    return asset.Cast<T>();
                }
            }
            return null;
        }
    }
}
