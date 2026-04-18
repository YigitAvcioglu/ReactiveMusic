using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class RepairSpotifyConnections
{
    [MenuItem("Tools/2- Repair UI Backend Links", false, 9)]
    public static void RepairConnections()
    {
        GameObject canvas = GameObject.Find("Spotify_Full_VR_Canvas");
        if (canvas == null)
        {
            Debug.LogError("Spotify_Full_VR_Canvas bulunamadı! Lütfen önce Tool 1'i çalıştırın.");
            return;
        }

        // Aktif sahnedeki bizim Controller'ı bul
        SpotifyPlayerController[] allControllers = Resources.FindObjectsOfTypeAll<SpotifyPlayerController>();
        SpotifyPlayerController targetController = null;
        foreach (var c in allControllers)
        {
            if (c.gameObject.scene.IsValid()) { targetController = c; break; }
        }

        if (targetController == null)
        {
            Debug.LogError("Sahnede SpotifyPlayerController bulunamadı!");
            return;
        }

        // Hiyerarşi düzeni
        if (!targetController.transform.IsChildOf(canvas.transform))
        {
            Undo.SetTransformParent(targetController.transform, canvas.transform, "Move Controller");
        }

        Debug.Log("Referanslar arka planda inceleniyor, kusursuz yollar çıkarılıyor...");

        // Orijinal sahneyi arkada aç
        string spotifyScenePath = "Assets/Spotify4Unity/Examples/Scenes/Spotify App.unity";
        Scene spotifyScene;
        try
        {
            spotifyScene = EditorSceneManager.OpenScene(spotifyScenePath, OpenSceneMode.Additive);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Spotify App sahnesi açılamadı! " + e.Message);
            return;
        }

        // Orijinal sahnedeki Controller'ı bul
        SpotifyPlayerController originalController = null;
        GameObject[] rootObjects = spotifyScene.GetRootGameObjects();
        foreach (var obj in rootObjects)
        {
            var controller = obj.GetComponentInChildren<SpotifyPlayerController>(true);
            if (controller != null)
            {
                originalController = controller;
                break;
            }
        }

        if (originalController != null)
        {
            SerializedObject soOriginal = new SerializedObject(originalController);
            SerializedObject soTarget = new SerializedObject(targetController);

            string[] propertiesToCopy = new string[] {
                "_trackName", "_artistsNames", "_trackIcon", 
                "_playPauseButton", "_previousButton", "_nextButton",
                "_shuffleButton", "_repeatButton", "_muteButton", "_addToLibraryButton",
                "_currentProgressSlider", "_currentProgressText", "_volumeSlider"
            };

            int successCount = 0;

            foreach (string propName in propertiesToCopy)
            {
                SerializedProperty origProp = soOriginal.FindProperty(propName);
                SerializedProperty targetProp = soTarget.FindProperty(propName);

                if (origProp != null && targetProp != null && origProp.objectReferenceValue != null)
                {
                    Component origComp = origProp.objectReferenceValue as Component;
                    if (origComp != null)
                    {
                        // Orijinal Canvas'a göre yolunu bul
                        string path = GetHierarchyPath(origComp.transform);
                        
                        // Bizim yeni Canvas'ın içinde aynı yolu bul
                        Transform ourMatch = canvas.transform.Find(path);

                        if (ourMatch != null)
                        {
                            Component ourComp = ourMatch.GetComponent(origComp.GetType());
                            if (ourComp != null)
                            {
                                targetProp.objectReferenceValue = ourComp;
                                successCount++;
                                Debug.Log($"[BAŞARILI] {propName} eşleşti -> {path}");
                            }
                            else
                            {
                                Debug.LogWarning($"Bulundu ama Component tipi uyuşmuyor: {propName} -> {path}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Yol sahnemizde bulunamadı: {propName} -> {path}");
                        }
                    }
                }
            }

            soTarget.ApplyModifiedProperties();
            Debug.Log($"🎉 TOPLAM {successCount} BAĞLANTI KUSURSUZCA KOPYALANDI! Lütfen Play tuşuna bas!");
        }
        else
        {
            Debug.LogError("Spotify App sahnesinde orijinal SpotifyPlayerController bulunamadı!");
        }

        EditorSceneManager.CloseScene(spotifyScene, true);
    }

    private static string GetHierarchyPath(Transform child)
    {
        string path = child.name;
        Transform curr = child.parent;
        // Orijinal Canvas'a kadar (veya parent kalmayana kadar) git
        while (curr != null && curr.GetComponent<Canvas>() == null)
        {
            path = curr.name + "/" + path;
            curr = curr.parent;
        }
        return path;
    }
}
