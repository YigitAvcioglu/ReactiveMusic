using UnityEngine;
using UnityEditor;
using System.Reflection;

public class RepairSpotifyAuth
{
    [MenuItem("Tools/5- Repair Spotify Authentication", false, 13)]
    public static void RepairAuth()
    {
        // Spotify Manager objesini bul
        GameObject manager = GameObject.Find("SpotifyManager");
        if (manager == null)
        {
            Debug.LogError("SpotifyManager sahnede bulunamadı!");
            return;
        }

        // SpotifyService ve PKCE_AuthConfig scriptlerini al
        Component spotifyService = manager.GetComponent("SpotifyService");
        Component pkceAuth = manager.GetComponent("PKCE_AuthConfig");

        if (spotifyService == null || pkceAuth == null)
        {
            Debug.LogError("SpotifyService veya PKCE_AuthConfig eksik!");
            return;
        }

        SerializedObject so = new SerializedObject(spotifyService);
        SerializedProperty authProp = so.FindProperty("_authMethodConfig");

        if (authProp != null)
        {
            authProp.objectReferenceValue = pkceAuth;
            so.ApplyModifiedProperties();
            Debug.Log($"[BAŞARILI] SpotifyService _authMethodConfig başarıyla {pkceAuth.GetType().Name} olarak bağlandı!");
        }
        else
        {
            Debug.LogError("_authMethodConfig propertysi SpotifyService içinde bulunamadı.");
        }

        // Connect on Start emin olalım
        SerializedProperty authOnStart = so.FindProperty("AuthorizeUserOnStart");
        if (authOnStart != null)
        {
            authOnStart.boolValue = true;
            so.ApplyModifiedProperties();
        }

        Debug.Log("🎉 KOPAN SPOTIFY BAĞLANTISI TAMİR EDİLDİ! Lütfen tekrar Play'e bas.");
    }
}
