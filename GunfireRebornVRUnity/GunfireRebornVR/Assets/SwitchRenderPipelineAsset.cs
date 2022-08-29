using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public static class SwitchRenderPipelineAsset
{

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject == null)
            throw new ArgumentNullException("GetOrAddComponent: gameObject is null!");

        T comp = gameObject.GetComponent<T>();
        if (comp == null)
            comp = gameObject.AddComponent<T>();

        return comp;
    }
}
