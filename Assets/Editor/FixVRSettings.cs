using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;

public class FixVRSettings
{
    [MenuItem("Tools/Fix VR Settings for Android")]
    public static void ApplySettings()
    {
        BuildTargetGroup targetGroup = BuildTargetGroup.Android;
        XRGeneralSettings settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(targetGroup);
        
        if (settings == null)
        {
            Debug.Log("Creating XRGeneralSettings for Android...");
            // As a fallback, we'll just log an error if we can't get it, since creation involves internal API.
            // But usually opening the XR Plugin Management window creates it.
            // Let's open the window so Unity creates the default assets if missing.
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
            return;
        }

        settings.InitManagerOnStart = true;
        Debug.Log("Set InitManagerOnStart to true.");

        var loaders = settings.Manager.activeLoaders;
        bool hasOpenXR = false;
        foreach (var loader in loaders)
        {
            if (loader.name.Contains("OpenXR"))
            {
                hasOpenXR = true;
                break;
            }
        }

        if (!hasOpenXR)
        {
            Debug.Log("Assigning OpenXR Loader...");
            XRPackageMetadataStore.AssignLoader(settings.Manager, "Unity.XR.OpenXR.OpenXRLoader", targetGroup);
        }

        // Apply Oculus Touch interaction profile
#if UNITY_OPENXR
        var openXRSettings = UnityEditor.XR.OpenXR.OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
        if (openXRSettings != null)
        {
            var profileType = typeof(UnityEditor.XR.OpenXR.Features.Interactions.OculusTouchControllerProfile);
            if (profileType != null)
            {
                var feature = openXRSettings.GetFeature(profileType);
                if (feature != null && !feature.enabled)
                {
                    feature.enabled = true;
                    Debug.Log("Enabled Oculus Touch Controller Profile.");
                }
            }
            
            var metaQuestType = typeof(UnityEditor.XR.OpenXR.Features.MetaQuestSupport.MetaQuestFeature);
            if (metaQuestType != null)
            {
                var feature = openXRSettings.GetFeature(metaQuestType);
                if (feature != null && !feature.enabled)
                {
                    feature.enabled = true;
                    Debug.Log("Enabled Meta Quest Support Feature.");
                }
            }
        }
#endif
        
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        Debug.Log("VR Settings for Android have been successfully applied!");
    }
}