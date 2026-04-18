using UnityEngine;
using UnityEditor;

public class ActivateSpotifyElements
{
    [MenuItem("Tools/3- Forcibly Activate Spotify UI", false, 11)]
    public static void ActivateUI()
    {
        // Controller'ın kapalı olma ihtimaline karşı zorla açalım
        SpotifyPlayerController[] allControllers = Resources.FindObjectsOfTypeAll<SpotifyPlayerController>();
        foreach(var c in allControllers)
        {
            if (c.gameObject.scene.IsValid())
            {
                c.gameObject.SetActive(true);
                // Scriptin tiki (enabled) kaldırılmışsa tikle
                c.enabled = true;
                EditorUtility.SetDirty(c.gameObject);
                Debug.Log($"[Fix] {c.gameObject.name} aktif hale getirildi.");
            }
        }

        GameObject canvas = GameObject.Find("Spotify_Full_VR_Canvas");
        if (canvas != null)
        {
            canvas.SetActive(true);
            
            // Lazerin algılamama ihtimaline karşı canvasa BoxCollider ekle (sadece butonlara tıklandığından emin olmak için basic fallback)
            // Ama XR Graphic Raycaster ana sistemdir.
            var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster != null) raycaster.enabled = true;

            Canvas c = canvas.GetComponent<Canvas>();
            if (c != null)
            {
                c.enabled = true;
            }
        }

        Debug.Log("🚀 SPOTIFY SISTEMI ZORLA AKTIF EDILDI! Lütfen oyunu başlat (Play'e bas) ve test et.");
    }
}
