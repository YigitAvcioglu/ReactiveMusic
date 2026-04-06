using UnityEngine;
using UnityEditor;

public class FixLayers {
    [MenuItem("Spotify/Fix UI Layers")]
    public static void Run() {
        var canvas = GameObject.Find("SpotifyCanvas");
        if (canvas) {
            int uiLayer = LayerMask.NameToLayer("UI");
            canvas.layer = uiLayer;
            foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true)) {
                t.gameObject.layer = uiLayer;
            }
            Debug.Log("✅ Canvas ve butonlar UI katmanina tasindi! Artik isinlanma hedefi olmayacaklar.");
        } else {
            Debug.LogError("Canvas bulunamadi.");
        }
    }
}
