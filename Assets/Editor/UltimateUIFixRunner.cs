using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEditor;

public class UltimateUIFixRunner : MonoBehaviour
{
    [MenuItem("Spotify/Fix Everything And Restore UI")]
    public static void Restore()
    {
        // 1. Restore the Canvas properly
        var canvas = GameObject.Find("SpotifyCanvas");
        if (canvas != null)
        {
            var graphicRaycaster = canvas.GetComponent<TrackedDeviceGraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                graphicRaycaster = canvas.AddComponent<TrackedDeviceGraphicRaycaster>();
            }
            graphicRaycaster.ignoreReversedGraphics = true;
            graphicRaycaster.checkFor2DOcclusion = false;
            graphicRaycaster.checkFor3DOcclusion = false;

            // Remove box colliders, simple interactables, and VRButtonLinkers from buttons
            var buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                var linker = btn.GetComponent("VRButtonLinker");
                if (linker) DestroyImmediate(linker);
                
                var simple = btn.GetComponent("XRSimpleInteractable");
                if (simple) DestroyImmediate(simple);
                
                var col = btn.GetComponent<BoxCollider>();
                if (col) DestroyImmediate(col);
                // Set layers to UI
                btn.gameObject.layer = LayerMask.NameToLayer("UI");
                
                // Add the custom click listener for VR Hover events
                var hoverOverride = btn.GetComponent<HoverClickOverride>();
                if (hoverOverride == null)
                {
                    btn.gameObject.AddComponent<HoverClickOverride>();
                }
            }
            canvas.layer = LayerMask.NameToLayer("UI");
        }

        // 2. Fix the Interactors (Teleport or Ray Interactors)
        var interactors = Object.FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var interactor in interactors)
        {
            interactor.enableUIInteraction = true;
            
            // VERY IMPORTANT: By default, if UIPress isn't assigned, UI cannot be clicked!
            // We can just share the Select reference to UI Press if it exists.
            
            // Force it via serialized object
                SerializedObject so = new SerializedObject(interactor);
                var uiPressProp = so.FindProperty("m_UIPressInput.m_InputActionReferencePerformed");
                var selectProp = so.FindProperty("m_SelectInput.m_InputActionReferencePerformed");
                if (uiPressProp != null && selectProp != null && uiPressProp.objectReferenceValue == null)
                {
                    uiPressProp.objectReferenceValue = selectProp.objectReferenceValue;
                }
                
                var uiPressValProp = so.FindProperty("m_UIPressInput.m_InputActionReferenceValue");
                var selectValProp = so.FindProperty("m_SelectInput.m_InputActionReferenceValue");
                if (uiPressValProp != null && selectValProp != null && uiPressValProp.objectReferenceValue == null)
                {
                    uiPressValProp.objectReferenceValue = selectValProp.objectReferenceValue;
                }
                so.ApplyModifiedProperties();
                Debug.Log($"Serialized properties synced for {interactor.name}");
        }
        
        // 3. Make sure the Event System is configured for XRI
        var eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem != null)
        {
            var oldInput = eventSystem.GetComponent("StandaloneInputModule");
            if (oldInput) DestroyImmediate(oldInput);
            
            var xrInput = eventSystem.GetComponent<XRUIInputModule>();
            if (xrInput == null)
            {
                xrInput = eventSystem.gameObject.AddComponent<XRUIInputModule>();
            }
        }

        // 4. Disable the InputTest console spammer
        var inputTest = GameObject.Find("Input Test");
        if (inputTest != null)
        {
            inputTest.SetActive(false);
            Debug.Log("Disabled InputTest to prevent console spam.");
        }

        Debug.Log("✅ NİHAİ DÜZELTME UYGULANDI! Lazer artık UI butonlarını MAVİ/KOYU yapacak ve tıklayacak!");
    }
}
