using UnityEngine;
using UI;
using UnityEngine.Rendering.PostProcessing;
using VRMod.Patches;
using VRMod.Core;

namespace VRMod.UI
{
    public static class MenuFix
    {
        public static void Prefix()
        {
            var canvasRoot = CUIManager.instance.transform.Find("Canvas_PC(Clone)");
            var UICamera = canvasRoot.Find("Camera");
            if(UICamera != null)
                UICamera.GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;
        }


        public static void Apply()
        {
            var canvasRoot = CUIManager.instance.transform.Find("Canvas_PC(Clone)");
            canvasRoot.transform.localScale = new Vector3(0.002f,0.002f,0.002f);
            canvasRoot.transform.position = Vector3.zero;

            // 原来的UICamera可以禁用掉了，也可以留着，屏幕上会多一层后处理滤镜
            var UICamera = canvasRoot.Find("Camera");
            UICamera.gameObject.active = false;
            UICamera.GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;

            // 将主菜单转为世界坐标，才能在VR里使用菜单
            var canvases = canvasRoot.gameObject.GetComponentsInChildren<Canvas>();
            foreach(var canvas in canvases)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Camera.main;
            }
            canvasRoot.gameObject.SetLayerRecursively(Layer.Default);
            canvasRoot.position = new Vector3(28f, 3.7f, 23.15f);
            //var settingRoot = canvasRoot.Find("SettingRoot");
            //settingRoot.localPosition = new Vector3(-627f, 219f, 219f);
            //settingRoot.eulerAngles = new Vector3(0, 327, 0);
            Transform mask;
            while(mask = canvasRoot.Find("Ani_mask")){
                mask.gameObject.active = false;
            }
        }
        public static void FixCollectionMenu()
        {
            var collectionList = CUIManager.instance.MainDialogCanvas.transform.Find("PC_Panel_collection/MainPage/Collection_list/");
            Object.Destroy(collectionList.Find("MonsterCollection_Enter/book_01").gameObject);
            Object.Destroy(collectionList.Find("RelicCollection_Enter/book_01").gameObject);
        }


        private static bool UECameraSet = false;

        public static void SetDebugUICamera()
        {
            if (UECameraSet)
                return;
            var go = GameObject.Find("UE_Freecam");
            if (go)
            {
                var UEcam = go.GetComponentInChildren<Camera>();
                var canvases = CUIManager.instance.gameObject.GetComponentsInChildren<Canvas>();
                foreach (var canvas in canvases)
                {
                    canvas.worldCamera = UEcam;
                }
                UECameraSet = true;
            }
        }

    }
}
