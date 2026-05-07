using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PatchScrollbarsMenuItem
{
    [MenuItem("Tools/Patch VR Scrollbars")]
    public static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Spotify4Unity/Examples/Prefabs" });
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root = editingScope.prefabContentsRoot;
                var scrollbars = root.GetComponentsInChildren<Scrollbar>(true);
                bool modified = false;
                foreach (var sb in scrollbars)
                {
                    if (sb.gameObject.GetComponent<XRSimpleInteractable>() == null)
                    {
                        sb.gameObject.AddComponent<XRSimpleInteractable>();
                        modified = true;
                    }
                    if (sb.gameObject.GetComponent("VRScrollbarLinker") == null)
                    {
                        var type = System.Type.GetType("VRScrollbarLinker, Assembly-CSharp");
                        if (type != null) {
                            sb.gameObject.AddComponent(type);
                            modified = true;
                        } else {
                            Debug.LogError("VRScrollbarLinker type not found in Assembly-CSharp.");
                        }
                    }
                }
                if (modified) count++;
            }
        }
        Debug.Log($"Patched {count} prefabs with Scrollbars.");
    }
}
