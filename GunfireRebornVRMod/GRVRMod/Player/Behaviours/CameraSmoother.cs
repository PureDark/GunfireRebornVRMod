using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VRMod.Player.MotionControlls;
using VRMod.Settings;
using static Cinemachine.CinemachineTargetGroup;

namespace VRMod.Player.Behaviours
{

    internal class CameraSmoother : MonoBehaviour
    {
        public CameraSmoother(IntPtr value) : base(value) { }
        public Transform target;

        public float smoothTime = 0.05f;
        private Quaternion deriv = Quaternion.identity;

        void Awake()
        {
            smoothTime = ModConfig.FPCamSmoothTime.Value;
        }

        void LateUpdate()
        {
            if(target != null)
            {
                transform.position = target.position;
                transform.rotation = transform.rotation.SmoothDamp(target.rotation, ref deriv, smoothTime);
            }
        }
    }
}
