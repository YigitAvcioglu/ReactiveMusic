using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Reflection;

public class EnableOpaqueTexture
{
    [MenuItem("Tools/Enable Opaque Texture")]
    static void Enable()
    {
        var asset = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
        if (asset == null) { Debug.LogError("No URP asset!"); return; }
        var field = typeof(UniversalRenderPipelineAsset).GetField("m_RequireOpaqueTexture", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null) { Debug.LogError("Field not found!"); return; }
        field.SetValue(asset, true);
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        Debug.Log("Opaque Texture ENABLED on " + asset.name);
    }
}
