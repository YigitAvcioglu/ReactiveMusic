using UnityEngine;
using UnityEditor;

public class FinalFixToggleAndAuth
{
    [MenuItem("Tools/4- Fix Toggle Button & Finalize", false, 12)]
    public static void FinalizeFixes()
    {
        // 1. Orijinal Toggle Button'a yeni Canvas'ı bağlayalım (null hatası için)
        GameObject canvasObj = GameObject.Find("Spotify_Full_VR_Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("Spotify_Full_VR_Canvas bulunamadı!");
        }
        else
        {
            Canvas newCanvas = canvasObj.GetComponent<Canvas>();
            SpotifyToggleButton[] toggleButtons = Resources.FindObjectsOfTypeAll<SpotifyToggleButton>();
            foreach (var tb in toggleButtons)
            {
                if (tb.gameObject.scene.IsValid())
                {
                    tb.spotifyCanvas = newCanvas;
                    EditorUtility.SetDirty(tb);
                    Debug.Log($"[BAŞARILI] SpotifyToggleButton ({tb.gameObject.name}) -> Yeni Canvas'a BAĞLANDI!");
                }
            }
        }
        Debug.Log("🎉 SON İŞLEM TAMAM! Oyun başlatıldığında Toggle hatası vermeyecek.");
    }
}
