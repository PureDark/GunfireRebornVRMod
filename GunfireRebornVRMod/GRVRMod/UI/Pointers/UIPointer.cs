using UnityEngine.EventSystems;
using UnityEngine;
using VRMod.Assets;
using Mathf = Valve.VR.Mathf;
using Il2CppSystem.Collections.Generic;

namespace VRMod.UI.Pointers
{
    public class UIPointer : MonoBehaviour
    {
        public float defaultLength = 3.0f;

        public EventSystem eventSystem = null;
        public StandaloneInputModule inputModule = null;


        private LineRenderer lineRenderer = null;

        private void Awake()
        {
            lineRenderer = gameObject.GetOrAddComponent<LineRenderer>();
        }
        private void Update()
        {
            UpdateLength();
        }

        private void UpdateLength()
        {
            lineRenderer.SetPosition(0, transform.localPosition);
            lineRenderer.SetPosition(1, GetEnd());
        }

        private Vector3 GetEnd()
        {
            float distance = GetCanavsDistance();
            Vector3 endPostition = CalculateEnd(defaultLength);

            if (distance != 0)
                endPostition = CalculateEnd(distance);

            return endPostition;
        }

        private float GetCanavsDistance()
        {
            // Get data
            PointerEventData eventData = new PointerEventData(eventSystem);
            eventData.position = inputModule.inputOverride.mousePosition;

            // Raycast using data
            List<RaycastResult> results = new List<RaycastResult>();
            eventSystem.RaycastAll(eventData, results);

            // Get closest
            RaycastResult cloestResult = FindFirstRaycast(results);
            float distance = cloestResult.distance;

            // Clamp
            distance = Mathf.Clamp(distance, 0.0f, defaultLength);

            return distance;
        }

        private RaycastResult FindFirstRaycast(List<RaycastResult> results)
        {
            foreach (var result in results)
            {
                if (!result.gameObject)
                    continue;
                return result;
            }
            return new RaycastResult();
        }

        private Vector3 CalculateEnd(float length)
        {
            return transform.localPosition + new Vector3(0, 0, length);
        }
    }

}
