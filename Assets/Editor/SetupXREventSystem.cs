using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEditor;

public class SetupXREventSystem {
    [MenuItem("Spotify/Fix Event System")]
    public static void Run() {
        var es = GameObject.Find("EventSystem");
        if (es != null) {
            GameObject.DestroyImmediate(es);
        }

        es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        var xrUI = es.AddComponent<XRUIInputModule>();

        var assetPaths = AssetDatabase.FindAssets("t:InputActionAsset XRI Default Input Actions");
        if (assetPaths.Length > 0) {
            string path = AssetDatabase.GUIDToAssetPath(assetPaths[0]);
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            var map = asset.FindActionMap("XRI UI");
            if (map != null) {
                xrUI.pointAction = InputActionReference.Create(map.FindAction("Point"));
                xrUI.leftClickAction = InputActionReference.Create(map.FindAction("Click"));
                xrUI.scrollWheelAction = InputActionReference.Create(map.FindAction("ScrollWheel"));
                xrUI.navigateAction = InputActionReference.Create(map.FindAction("Navigate"));
                xrUI.submitAction = InputActionReference.Create(map.FindAction("Submit"));
                xrUI.cancelAction = InputActionReference.Create(map.FindAction("Cancel"));
                Debug.Log("✅ EventSystem yeniden olusturuldu ve XRI Input Aksiyonlari atandi!");
            } else {
                Debug.LogError("XRI UI map bulunamadi!");
            }
        } else {
            Debug.LogError("XRI Default Input Actions asset'i bulunamadi!");
        }
    }
}
