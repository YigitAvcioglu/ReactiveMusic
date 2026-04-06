using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEditor;

public class MakeUIPhysicalVR {
    [MenuItem("Spotify/Hardcore VR Fix")]
    public static void Run() {
        // 1. Isinlanmayi tamamen devre disi birak!
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(var go in allObjects) {
            // Teleportasyon sistemini kapat
            var provider = go.GetComponent("TeleportationProvider") as Behaviour;
            if (provider != null) {
                provider.enabled = false;
            }
        }
        
        // 2. Butonlari fiziksel bir nesneye (Duvar gibi cikan/vurulan) donustur
        var canvas = GameObject.Find("SpotifyCanvas");
        if (canvas) {
            // UI Raycasterina bel baglamak yerine normal objeye donusturuyoruz
            canvas.layer = 0; // Default layer'a geri cek
            foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true)) {
                t.gameObject.layer = 0;
            }

            var buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons) {
                var rect = btn.GetComponent<RectTransform>();
                
                // BoxCollider ekle (Hassas fiziksel algilama)
                var col = btn.gameObject.GetComponent<BoxCollider>();
                if (!col) col = btn.gameObject.AddComponent<BoxCollider>();
                col.size = new Vector3(rect.rect.width, rect.rect.height, 5f); 
                
                // Fiziksel XR Etkilesim nesnesi ekle
                var interactable = btn.gameObject.GetComponent("XRSimpleInteractable") as UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable;
                if (!interactable) {
                    interactable = btn.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
                }
                
                // Linker scripti ekle (Çalışma zamanında dinleyiciyi korur)
                var linker = btn.gameObject.GetComponent("VRButtonLinker");
                if (!linker) {
                    btn.gameObject.AddComponent<VRButtonLinker>();
                }
            }
            Debug.Log("✅ Tamamdir! Lazer her yere isabet alabilir, butonlara da VRButtonLinker eklendi!");
        } else {
            Debug.LogError("SpotifyCanvas sahnede bulunamadi!");
        }
    }
}
