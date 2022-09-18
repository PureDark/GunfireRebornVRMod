using UnityEngine;
using UI;
using static VRMod.VRMod;

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


        public static void HomeFix()
        {
            // 第一次初始化一定是在Home场景，处理一下相机
            var CamPoint = GameObject.Find("CamPoint_Camera");
            if (CamPoint)
            {
                var cameras = CamPoint.GetComponentsInChildren<Camera>(true);
                foreach (var cam in cameras)
                {
                    cam.stereoTargetEye = StereoTargetEyeMask.None;
                    cam.enabled = false;
                }
            }

            var canvasRoot = CUIManager.instance.transform.Find("Canvas_PC(Clone)");
            canvasRoot.transform.localScale = new Vector3(0.002f,0.002f,0.002f);

            // 原来的UICamera可以禁用掉了，但是gameObject不能禁用，需要上面的PostProcessingVolume
            var UICamera = canvasRoot.Find("Camera");
            UICamera.GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;
            //UICamera.GetComponent<Camera>().enabled = false;

            // 将主菜单转为世界坐标，才能在VR里使用菜单
            var canvases = canvasRoot.gameObject.GetComponentsInChildren<Canvas>(true);
            foreach(var canvas in canvases)
            {
                canvas.renderMode = RenderMode.WorldSpace;
            }
            //canvasRoot.gameObject.SetLayerRecursively(Layer.Default);
            canvasRoot.position = new Vector3(32.8f, 3.7f, 16f);
            canvasRoot.localEulerAngles = new Vector3(0, 90, 0);

            //蜡烛的烟要改成世界朝向，否则歪头时会跟着一起歪
            var effectRoot = GameObject.Find("Effectroot");
            foreach (var child in effectRoot.transform)
            {
                if (child.Cast<Transform>().gameObject.name.Contains("candle"))
                {
                    var psrs = child.Cast<Transform>().GetComponentsInChildren<ParticleSystemRenderer>(true);
                    foreach (var psr in psrs)
                    {
                        psr.alignment = ParticleSystemRenderSpace.World;
                    }
                }
            }

            // 如果已经有上一次的武器图鉴和怪物图鉴存在，销毁掉
            var weapon = CUIManager.instance.MainDialogCanvas.transform.Find("Weapon");
            if (weapon)
                Object.Destroy(weapon.gameObject);
            var monster = CUIManager.instance.MainDialogCanvas.transform.Find("Monster");
            if (monster)
                Object.Destroy(monster.gameObject);
            //武器图鉴和怪物图鉴的模型要显示到UI所在的位置
            var ModelRoot = GameObject.Find("3000101");
            weapon = ModelRoot.transform.Find("Weapon");
            if (weapon != null)
            {
                weapon.parent = CUIManager.instance.MainDialogCanvas.transform;
                weapon.localPosition = new Vector3(0, -215, 0);
                weapon.localEulerAngles = new Vector3(0, 90, 0);
                weapon.localScale = new Vector3(1000, 1000, 1000);
            }

            monster = ModelRoot.transform.Find("Monster");
            if (monster != null)
            {
                monster.parent = CUIManager.instance.MainDialogCanvas.transform;
                monster.localPosition = new Vector3(0, -600, 0);
                monster.localEulerAngles = new Vector3(0, 180, 0);
                monster.localScale = new Vector3(450, 450, 450);
            }
        }

        public static void FixCollectionMenu()
        {
            //部分粒子特效无法缩放，太过巨大遮挡视野，直接销毁
            var collectionList = CUIManager.instance.MainDialogCanvas.transform.Find("PC_Panel_collection/MainPage/Collection_list/");
            Object.Destroy(collectionList?.Find("MonsterCollection_Enter/book_01")?.gameObject);
            Object.Destroy(collectionList?.Find("RelicCollection_Enter/book_01")?.gameObject);
        }

        public static void FixCharacterMenu()
        {
            var heroList = CUIManager.instance.MainDialogCanvas.transform.Find("PC_Panel_character/lay_mainpage/lay_base/list_area/HeroTable/Viewport/Content/");

            //部分粒子特效无法缩放，太过巨大遮挡视野，直接销毁
            foreach(var child in heroList)
            {
                Object.Destroy(child.Cast<Transform>().Find("state/hero_index_bg/")?.gameObject);
                Object.Destroy(child.Cast<Transform>().Find("state/hero_index_txt/")?.gameObject);
            }

            //让选中英雄的发光特效不会随着视角移动
            var psrs = heroList.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (var psr in psrs)
            {
                psr.alignment = ParticleSystemRenderSpace.Local;
            }
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
