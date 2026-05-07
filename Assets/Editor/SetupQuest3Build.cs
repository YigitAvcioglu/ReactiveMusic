using UnityEditor;
using UnityEngine;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;

/// <summary>
/// Quest 3 APK ayarlarini otomatik yapar.
/// Menu: Tools > Setup Quest 3 Build
/// </summary>
public static class SetupQuest3Build
{
    [MenuItem("Tools/Setup Quest 3 Build")]
    public static void Run()
    {
        Debug.Log("=== Quest 3 Build Setup Basliyor ===");

        // 1) Android platform
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            Debug.Log("[Q3] Android platformuna gecildi.");
        }

        // 2) IL2CPP + ARM64
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        Debug.Log("[Q3] IL2CPP + ARM64 ayarlandi.");

        // 3) Minimum API 29 (Quest gereksinimi), Target API 32
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)32;
        Debug.Log("[Q3] Min SDK: 29, Target SDK: 32");

        // 4) Internet izni
        PlayerSettings.Android.forceInternetPermission = false;

        // 5) Grafik API: Vulkan oncelikli (Quest 3 destekler)
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,
            new[] { UnityEngine.Rendering.GraphicsDeviceType.Vulkan, UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
        Debug.Log("[Q3] Grafik API: Vulkan + OpenGLES3");

        // 6) Display resolution - Quest 3 display
        PlayerSettings.defaultIsNativeResolution = true;

        // 7) Stereo rendering
        PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
        Debug.Log("[Q3] Stereo Rendering: Single Pass Instanced");

        // 8) Color space Linear
        PlayerSettings.colorSpace = ColorSpace.Linear;
        Debug.Log("[Q3] Color Space: Linear");

        // 9) Bundle ID duzelt (nokta olamaz bas harflerle)
        if (PlayerSettings.applicationIdentifier.Contains("com.UnityTechnologies"))
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.yigit.vrgame");
            Debug.Log("[Q3] Bundle ID: com.yigit.vrgame");
        }

        // 10) Orientation - Landscape
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

        AssetDatabase.SaveAssets();
        Debug.Log("=== Quest 3 Build Setup Tamamlandi! ===");
        Debug.Log("Eksik: XR Plugin Management'ta OpenXR'i Android icin aktive etmeyi unutma!");
        EditorUtility.DisplayDialog("Quest 3 Setup Tamamlandi",
            "Tum Android ayarlari yapildi!\n\n" +
            "MANUEL KONTROL:\n" +
            "Edit > Project Settings > XR Plugin Management > Android sekmesinde:\n" +
            "- OpenXR kutucugunu isaretleyin\n" +
            "- OpenXR > Meta Quest feature grubunu ekleyin\n\n" +
            "KONTROL SEMASI (Quest 3):\n" +
            "Sol Stick: Hareket\nSag Stick X: Donus\nSag Stick Y: Dikey Bakis\n" +
            "Trigger: Interact / Grip: Grab",
            "Tamam");
    }
}
