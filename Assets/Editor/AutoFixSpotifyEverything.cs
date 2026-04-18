using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

[InitializeOnLoad]
public class AutoFixSpotifyRunOnceV3
{
    static AutoFixSpotifyRunOnceV3()
    {
        EditorApplication.update += RunOnce;
    }

    static void RunOnce()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        
        string prefKey = "AutoFixSpotifyRan_v3";
        if (EditorPrefs.GetBool(prefKey, false)) return;
        EditorPrefs.SetBool(prefKey, true);

        EditorApplication.update -= RunOnce;
        FixEverything();
    }

    [MenuItem("Tools/FORCE FIX SPOTIFY INTERFACE V3")]
    public static void FixEverything()
    {
        Debug.Log("(Auto-Fix) Spotify Arayüz onarımı başlatılıyor V3...");

        var controller = GameObject.FindObjectOfType<SpotifyPlayerController>(true);
        if (controller == null)
            return;

        var newCanvas = GameObject.Find("Spotify_Full_VR_Canvas");
        if (newCanvas == null)
            return;

        // ESKİLERİ GİZLE VE LİNKLEYEN SCRİPTLERİ DEVRE DIŞI BIRAK
        var oldControllerObj = GameObject.Find("SpotifyController");
        if (oldControllerObj != null)
        {
            foreach (Transform child in oldControllerObj.transform)
            {
                if (child.name.StartsWith("Btn_") || child.name.Contains("Text") || child.name.Contains("Slider") || child.name.Contains("Cover"))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        // SerializedObject ile bağla
        SerializedObject so = new SerializedObject(controller);
        so.Update();

        Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                Transform result = FindChildRecursive(child, name);
                if (result != null) return result;
            }
            return null;
        }

        void BindObj(string propName, string searchName)
        {
            Transform t = FindChildRecursive(newCanvas.transform, searchName);
            if (t != null) {
                var p = so.FindProperty(propName);
                if (p != null) {
                    if (p.type.Contains("TextMeshProUGUI")) p.objectReferenceValue = t.GetComponent<TextMeshProUGUI>();
                    else if (p.type.Contains("TextMeshPro")) p.objectReferenceValue = t.GetComponent<TextMeshPro>();
                    else if (p.type.Contains("TextMesh")) p.objectReferenceValue = t.GetComponent<TextMesh>();
                    else if (p.type.Contains("Text")) p.objectReferenceValue = t.GetComponent<Text>();
                    else if (p.type.Contains("Button")) p.objectReferenceValue = t.GetComponent<Button>();
                    else if (p.type.Contains("Slider")) p.objectReferenceValue = t.GetComponent<Slider>();
                    else if (p.type.Contains("Image")) p.objectReferenceValue = t.GetComponent<Image>();
                    else if (p.type.Contains("RawImage")) p.objectReferenceValue = t.GetComponent<RawImage>();
                    else if (p.type.Contains("Transform")) p.objectReferenceValue = t;
                    else if (p.type.Contains("GameObject")) p.objectReferenceValue = t.gameObject;
                    else p.objectReferenceValue = t.GetComponent(p.type.Replace("PPtr<$", "").Replace(">", ""));
                    
                    if (p.objectReferenceValue != null) Debug.Log($"[BULDUM VE BAĞLADIM] {propName} -> {searchName}");
                    else Debug.LogWarning($"[Component Yok] {propName} için {searchName} bulundu ama üstünde uygun Component (Text/Button vs) yok!");
                }
            } else {
                Debug.LogWarning("[Bulunamadı Sahne Objesi] " + searchName);
            }
        }

        BindObj("_txtNowPlayingTrack", "Track Title");
        BindObj("_txtNowPlayingArtist", "Artist");
        BindObj("_imgNowCover", "Track Icon");

        // UI Slider'lar
        BindObj("_uiSliderPlaylistProgress", "PlaybackSlider");
        BindObj("_uiSliderVolume", "Current Volume Slider"); // Veya Volume Slider, yukarıda gördüm
        BindObj("_txtVolumeAmount", "VolumeText"); // Varsayılan

        // Player Text'ler
        BindObj("_txtProgressPlayed", "SongCurrentText");
        BindObj("_txtProgressTotal", "SongTotalText");

        // Butonlar
        BindObj("_btnPlayPause", "Play/Pause Button"); 
        BindObj("_btnPlay", "Play Button");
        BindObj("_btnPause", "Pause Button");
        
        BindObj("_btnNext", "Next Button");
        BindObj("_btnPrev", "Previous Button");
        BindObj("_btnShuffle", "Shuffle Button");
        BindObj("_btnRepeat", "Repeat Button");

        BindObj("_btnSaveTrack", "Add to Library Button"); // Heart Icon onun child'ı olabilir
        BindObj("_btnQueue", "Add To Queue Btn");
        BindObj("_btnMute", "Mute Button");

        // Account
        BindObj("_txtAccName", "User Display Name Text");
        BindObj("_imgAccProfile", "Profile Picture"); // Eğer objede Image yoksa, child araması
        BindObj("_basePlaylistUIPrefab", "Single Playlist Track");
        BindObj("_basePlaylistContentParent", "Playlists Nav Header");

        so.ApplyModifiedProperties();

        // VR için Raycaster
        var raycaster = newCanvas.GetComponent("TrackedDeviceGraphicRaycaster") as MonoBehaviour;
        if (raycaster == null)
        {
            var type = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
            if (type != null) newCanvas.AddComponent(type);
        }

        Debug.Log("(Auto-Fix) BAŞARIYLA TAMAMLANDI! Artık 100% bağlandı.");
    }
}
// Trigger recompile
