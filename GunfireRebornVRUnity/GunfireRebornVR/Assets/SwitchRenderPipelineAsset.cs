using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class SwitchRenderPipelineAsset : MonoBehaviour
{
    public RenderPipelineAsset exampleAssetA;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GraphicsSettings.renderPipelineAsset = exampleAssetA;
            Debug.Log("Active render pipeline asset is: " + GraphicsSettings.renderPipelineAsset.name);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            GraphicsSettings.renderPipelineAsset = null;
            Debug.Log("Active render pipeline asset is: None");
        }
    }
}
