using HeroCameraName;
using System;
using UI;
using UnityEngine;
using VRMod.Player.MotionControlls;

namespace VRMod.Player
{
    public class VRBattleUI : MonoBehaviour
    {
        public VRBattleUI(IntPtr value) : base(value) { }
        public Transform Head;
        public HandController LeftHand, RightHand;
        public Transform canvasRoot;
        public Transform PC_Panel_war;
        public Transform minimap;
        public Transform hp;
        public Transform curweapon;
        public Transform Fastmove_tips;
        public Transform button_grenade;
        public Transform hero_skill_1;
        public Transform crossHair;

        private Transform canvasRootTarget;
        private Transform minimapTarget;
        private Transform hpTarget;

        public float distance = 3.0f;
        public float smoothTime = 0.3f;
        private Vector3 canvasRootVelocity = Vector3.zero;
        private Vector3 minimapVelocity = Vector3.zero;
        private Vector3 hpVelocity = Vector3.zero;
        private Quaternion canvasRootDeriv = Quaternion.identity;
        private Quaternion minimapDeriv = Quaternion.identity;
        private Quaternion hpDeriv = Quaternion.identity;
        private bool setup = false;

        void Awake()
        {
            Head = VRPlayer.Instance.Head;
            LeftHand = VRPlayer.Instance.LeftHand;
            RightHand = VRPlayer.Instance.RightHand;
        }

        void Start()
        {
        }

        public void Setup()
        {
            Reset();
            setup = true;
        }

        public void Reset()
        {
            PC_Panel_war = null;
            curweapon = null;
            crossHair = null;
            hp = null;
            hero_skill_1 = null;
        }

        void Update()
        {
            if (setup)
            {
                canvasRoot = CUIManager.instance.m_UIRoot;
                PC_Panel_war = CUIManager.instance.MenuCanvas.transform.Find("PC_Panel_war");
                if (PC_Panel_war)
                {
                    setup = true;
                    PC_Panel_war.GetComponent<Animator>().enabled = false;
                    PC_Panel_war.Find("bullet_tips").gameObject.active = false;
                    hp = PC_Panel_war.Find("hp");
                    hp.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    Fastmove_tips = PC_Panel_war.Find("Fastmove_tips");
                    Fastmove_tips.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    button_grenade = PC_Panel_war.Find("button_grenade");
                    button_grenade.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    curweapon = PC_Panel_war.Find("curweapon");
                    curweapon.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    hero_skill_1 = PC_Panel_war.Find("hero_skill_1");
                    minimap = CUIManager.instance.MenuCanvas.transform.Find("minimap");
                    minimap.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    crossHair = CUIManager.instance.SightCanvas.transform;
                    if (!canvasRootTarget)
                    {
                        canvasRootTarget = new GameObject("canvasRootTarget").transform;
                        canvasRootTarget.parent = VRPlayer.Instance.Origin;
                        canvasRootTarget.position = canvasRoot.position;
                        canvasRootTarget.rotation = canvasRoot.rotation;
                    }
                    if (!minimapTarget)
                    {
                        minimapTarget = new GameObject("minimapTarget").transform;
                        minimapTarget.parent = VRPlayer.Instance.Origin;
                        minimapTarget.position = minimap.position;
                        minimapTarget.rotation = minimap.rotation;
                    }
                    if (!hpTarget)
                    {
                        hpTarget = new GameObject("hpTarget").transform;
                        hpTarget.parent = VRPlayer.Instance.Origin;
                        hpTarget.position = hp.position;
                        hpTarget.rotation = hp.rotation;
                    }
                }
            }
        }

        void LateUpdate()
        {
            if (VRPlayer.Instance.isHome)
            {
                Reset();
                return;
            }
            if (canvasRoot && Time.timeScale > 0)
            {
                var targetPosition = Head.position + VRPlayer.Instance.GetFlatForwardDirection() * 4;
                canvasRootTarget.position = Vector3.SmoothDamp(canvasRootTarget.position, targetPosition, ref canvasRootVelocity, smoothTime);
                canvasRoot.position = canvasRootTarget.position;

                var smoothHUDRotation = canvasRootTarget.rotation.SmoothDamp(Quaternion.LookRotation(VRPlayer.Instance.GetFlatForwardDirection()), ref canvasRootDeriv, smoothTime);
                canvasRootTarget.rotation = smoothHUDRotation;
                canvasRoot.rotation = canvasRootTarget.rotation;
            }

            // 小地图显示在视野左上角，并进行有延迟的平滑跟随
            if (minimap)
            {
                var targetPosition = Head.position + VRPlayer.Instance.GetFlatForwardDirection() * 2 - Head.right * 1f + Head.up * 1f;
                minimapTarget.position = Vector3.SmoothDamp(minimapTarget.position, targetPosition, ref minimapVelocity, smoothTime);
                minimap.position = minimapTarget.position;

                //var smoothHUDRotation = minimapTarget.rotation.SmoothDamp(Quaternion.LookRotation(VRPlayer.Instance.GetFlatForwardDirection()), ref minimapDeriv, smoothTime);
                //minimapTarget.rotation = smoothHUDRotation;
                minimap.rotation = Quaternion.LookRotation(minimap.position - Head.position);
            }

            // 血量相关UI显示在视野左上角，并进行有延迟的平滑跟随
            if (hp)
            {
                var targetPosition = Head.position + VRPlayer.Instance.GetFlatForwardDirection() * 0.5f - Head.right * 0.5f - Head.up * 1.5f;
                hpTarget.position = Vector3.SmoothDamp(hpTarget.position, targetPosition, ref hpVelocity, smoothTime);
                hp.position = hpTarget.position;

                //var smoothHUDRotation = hpTarget.rotation.SmoothDamp(Quaternion.LookRotation(VRPlayer.Instance.GetFlatForwardDirection()), ref hpDeriv, smoothTime);
                //hpTarget.rotation = smoothHUDRotation;
                //hp.rotation = hpTarget.rotation;
                hp.rotation = Quaternion.LookRotation(hp.position - Head.position);
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
                Fastmove_tips.position = LeftHand.model.position + LeftHand.model.right * 0.2f + LeftHand.model.forward * 0f - LeftHand.model.up * 0.1f;
                Fastmove_tips.rotation = LeftHand.model.rotation;
                button_grenade.position = LeftHand.model.position + LeftHand.model.right * 0.12f + LeftHand.model.forward * 0f - LeftHand.model.up * 0.8f;
                button_grenade.rotation = LeftHand.model.rotation;
            }

            // 准心显示在射线瞄准处
            if (crossHair)
            {
                crossHair.position = RightHand.GetRayHitPosition(10);
                crossHair.rotation = Quaternion.LookRotation(crossHair.position - RightHand.muzzle.position);
            }
            //if (hero_skill_1)
            //{
            //    var parent = hero_skill_1.parent;
            //    hero_skill_1.parent = RightHand;
            //    hero_skill_1.localPosition = new Vector3(-0.5f, 0f, 0.5f);
            //    hero_skill_1.localEulerAngles = new Vector3(0f, 0f, 0f);
            //    hero_skill_1.parent = parent;
            //}
        }

    }
}
