using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRMod.UI
{
    public class SmoothHUD : MonoBehaviour
    {
        public SmoothHUD(IntPtr value) : base(value) { }
        public Camera camera;
        public float distance = 3.0F;
        public float smoothTime = 0.5F;
        private Vector3 velocity = Vector3.zero;


        void LateUpdate()
        {
            Vector3 targetPosition = camera.transform.TransformPoint(new Vector3(0, 0, distance));

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            var lookAtPos = new Vector3(camera.transform.position.x, transform.position.y, camera.transform.position.z);
            transform.LookAt(lookAtPos);
        }
    }
}
