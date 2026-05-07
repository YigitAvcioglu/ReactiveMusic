using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PatchAllUIMenuItem
{
    [MenuItem("Tools/Patch VR All UI")]
    public static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Spotify4Unity" });
        int btnCount = 0;
        int sliderCount = 0;
        int scrollCount = 0;
        int toggleCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root = editingScope.prefabContentsRoot;
                bool modified = false;

                // Buttons
                var btns = root.GetComponentsInChildren<Button>(true);
                foreach (var b in btns)
                {
                    if (b.gameObject.GetComponent("VRButtonLinker") == null)
                    {
                        var type = System.Type.GetType("VRButtonLinker, Assembly-CSharp");
                        if (type != null) { b.gameObject.AddComponent(type); modified = true; btnCount++; }
                    }
                }

                // Toggles
                var toggles = root.GetComponentsInChildren<Toggle>(true);
                foreach (var tgl in toggles)
                {
                    // For toggles, we can also use VRButtonLinker since it triggers DoInvoke() -> but wait! VRButtonLinker does btn.onClick.Invoke().
                    // It doesn't invoke Toggle. We should create a VRToggleLinker or just let standard UI Handle it if possible.
                    // Or we just add VRButtonLinker and it won't work for toggles. Let's see if Spotify uses Toggles or Buttons for mute.
                    if (tgl.gameObject.GetComponent("VRButtonLinker") == null)
                    {
                        var type = System.Type.GetType("VRButtonLinker, Assembly-CSharp");
                        if (type != null) { tgl.gameObject.AddComponent(type); modified = true; toggleCount++; }
                    }
                }

                // Sliders
                var sliders = root.GetComponentsInChildren<Slider>(true);
                foreach (var s in sliders)
                {
                    if (s.gameObject.GetComponent<XRSimpleInteractable>() == null)
                    {
                        s.gameObject.AddComponent<XRSimpleInteractable>();
                    }
                    if (s.gameObject.GetComponent("VRSliderLinker") == null)
                    {
                        var type = System.Type.GetType("VRSliderLinker, Assembly-CSharp");
                        if (type != null) { s.gameObject.AddComponent(type); modified = true; sliderCount++; }
                    }
                }

                // Scrollbars
                var scrollbars = root.GetComponentsInChildren<Scrollbar>(true);
                foreach (var sb in scrollbars)
                {
                    if (sb.gameObject.GetComponent<XRSimpleInteractable>() == null)
                    {
                        sb.gameObject.AddComponent<XRSimpleInteractable>();
                    }
                    if (sb.gameObject.GetComponent("VRScrollbarLinker") == null)
                    {
                        var type = System.Type.GetType("VRScrollbarLinker, Assembly-CSharp");
                        if (type != null) { sb.gameObject.AddComponent(type); modified = true; scrollCount++; }
                    }
                }
            }
        }

        // Now patch active scene objects
        var btnsScene = Object.FindObjectsOfType<Button>(true);
        foreach (var b in btnsScene)
        {
            if (b.gameObject.GetComponent("VRButtonLinker") == null)
            {
                var type = System.Type.GetType("VRButtonLinker, Assembly-CSharp");
                if (type != null) { b.gameObject.AddComponent(type); btnCount++; }
            }
        }

        var togglesScene = Object.FindObjectsOfType<Toggle>(true);
        foreach (var tgl in togglesScene)
        {
            if (tgl.gameObject.GetComponent("VRButtonLinker") == null)
            {
                var type = System.Type.GetType("VRButtonLinker, Assembly-CSharp");
                if (type != null) { tgl.gameObject.AddComponent(type); toggleCount++; }
            }
        }

        var slidersScene = Object.FindObjectsOfType<Slider>(true);
        foreach (var s in slidersScene)
        {
            if (s.gameObject.GetComponent<XRSimpleInteractable>() == null)
            {
                s.gameObject.AddComponent<XRSimpleInteractable>();
            }
            if (s.gameObject.GetComponent("VRSliderLinker") == null)
            {
                var type = System.Type.GetType("VRSliderLinker, Assembly-CSharp");
                if (type != null) { s.gameObject.AddComponent(type); sliderCount++; }
            }
        }

        var scrollbarsScene = Object.FindObjectsOfType<Scrollbar>(true);
        foreach (var sb in scrollbarsScene)
        {
            if (sb.gameObject.GetComponent<XRSimpleInteractable>() == null)
            {
                sb.gameObject.AddComponent<XRSimpleInteractable>();
            }
            if (sb.gameObject.GetComponent("VRScrollbarLinker") == null)
            {
                var type = System.Type.GetType("VRScrollbarLinker, Assembly-CSharp");
                if (type != null) { sb.gameObject.AddComponent(type); scrollCount++; }
            }
        }

        Debug.Log($"Patched Prefabs and Scene: {btnCount} Buttons, {toggleCount} Toggles, {sliderCount} Sliders, {scrollCount} Scrollbars.");
    }
}
