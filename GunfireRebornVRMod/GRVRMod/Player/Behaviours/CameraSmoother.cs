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
        public bool enablePositionSmoothing = false;
        public bool enableRotationSmoothing = false;

        public float smoothTime = 0.05f;
        private Quaternion deriv = Quaternion.identity;
        private Vector3 currentVelocity = Vector3.zero;

        public void Awake()
        {
        }

        public void LateUpdate()
        {
            if(target != null)
            {
                if (enablePositionSmoothing)
                    transform.position = Vector3.SmoothDamp(transform.position, target.position, ref currentVelocity, smoothTime);
                else
                    transform.position = target.position;

                if (enableRotationSmoothing)
                    transform.rotation = transform.rotation.SmoothDamp(target.rotation, ref deriv, smoothTime);
                else
                    transform.rotation = target.rotation;
            }
        }
    }
}
