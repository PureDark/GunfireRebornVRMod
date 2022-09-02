using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRMod
{
    internal static class Utils
    {

        public static void SetLayerRecursively(this GameObject inst, int layer)
        {
            inst.layer = layer;
            int children = inst.transform.childCount;
            for (int i = 0; i < children; ++i)
                inst.transform.GetChild(i).gameObject.SetLayerRecursively(layer);
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null)
                throw new ArgumentNullException("GetOrAddComponent: gameObject is null!");

            T comp = gameObject.GetComponent<T>();
            if (comp == null)
                comp = gameObject.AddComponent<T>();

            return comp;
        }

        public static bool IsPlaying(this Animator animator)
        {
            return animator.GetCurrentAnimatorStateInfo(0).normalizedTime <1;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }


        /// <summary>
        /// Determine the signed angle between two vectors, with normal 'n'
        /// as the rotation axis.
        /// </summary>
        public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
        {
            return Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
        }


        //Made by maxattack on GitHub
        public static Quaternion SmoothDamp(this Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
        {
            if (Time.unscaledDeltaTime < Mathf.Epsilon) return rot;
            if (time == 0f) return target;

            var Dot = Quaternion.Dot(rot, target);
            var Multi = Dot > 0f ? 1f : -1f;
            target.x *= Multi;
            target.y *= Multi;
            target.z *= Multi;
            target.w *= Multi;

            var Result = new Vector4(
                Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time, int.MaxValue, Time.unscaledDeltaTime),
                Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time, int.MaxValue, Time.unscaledDeltaTime),
                Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time, int.MaxValue, Time.unscaledDeltaTime),
                Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time, int.MaxValue, Time.unscaledDeltaTime)
            ).normalized;

            var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
            deriv.x -= derivError.x;
            deriv.y -= derivError.y;
            deriv.z -= derivError.z;
            deriv.w -= derivError.w;

            return new Quaternion(Result.x, Result.y, Result.z, Result.w);
        }
    }
}
