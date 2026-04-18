using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FetchSpotifyUI
{
    [MenuItem("Tools/Fetch and Setup Full Spotify VR UI", false, 5)]
    public static void FetchAndSetup()
    {
        // 1. Aktif sahneyi güvenceye al
        Scene activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.isDirty)
        {
            EditorSceneManager.SaveScene(activeScene);
        }

        // 2. Orijinal Spotify sahnesini bozmadan arka planda (Additive) aç
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

        // 3. Spotify sahnesinin içinden devasa asıl Canvas'ı bul
        GameObject originalCanvas = null;
        GameObject[] rootObjects = spotifyScene.GetRootGameObjects();

        foreach (var obj in rootObjects)
        {
            if (obj.GetComponent<Canvas>() != null)
            {
                originalCanvas = obj;
                break;
            }
        }

        if (originalCanvas == null)
        {
            Debug.LogError("Sahnedeki orijinal Spotify Canvas'ı bulunamadı!");
            EditorSceneManager.CloseScene(spotifyScene, true);
            return;
        }

        // 4. Canvas'ı kopyala ve mevcut sahnemize aktar
        GameObject clonedCanvasInfo = Object.Instantiate(originalCanvas);
        SceneManager.MoveGameObjectToScene(clonedCanvasInfo, activeScene);
        Undo.RegisterCreatedObjectUndo(clonedCanvasInfo, "Fetch Spotify UI");

        // 5. İşimiz bittiği için örnek Spotify sahnesini geri kapat
        EditorSceneManager.CloseScene(spotifyScene, true);

        // --- 6. Bizim Kopyayı Sahnedeki VR'a Göre Uyarla ---
        Canvas targetCanvas = clonedCanvasInfo.GetComponent<Canvas>();
        clonedCanvasInfo.name = "Spotify_Full_VR_Canvas";

        targetCanvas.renderMode = RenderMode.WorldSpace;
        targetCanvas.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
        
        // Klasik bir konuma (odanın merkezi / göz hizası) koyalım
        if (Camera.main != null)
        {
            targetCanvas.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
            Vector3 lookDir = targetCanvas.transform.position - Camera.main.transform.position;
            lookDir.y = 0; // Orijinal dik duruşunu bozmaması için
            if (lookDir != Vector3.zero)
                targetCanvas.transform.rotation = Quaternion.LookRotation(lookDir);
        }
        else
        {
            targetCanvas.transform.position = new Vector3(0f, 1.4f, 2f);
            targetCanvas.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        // --- BUTON TIKLAMALARINI VR UYUMLU YAPMA ---
        GraphicRaycaster oldRaycaster = clonedCanvasInfo.GetComponent<GraphicRaycaster>();
        if (oldRaycaster != null && oldRaycaster.GetType() == typeof(GraphicRaycaster))
        {
            Object.DestroyImmediate(oldRaycaster);
        }

        // Lazerle tıklanma (XR Interaction Toolkit) bileşeni
        System.Type xrRaycasterType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit")
            ?? System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, UnityEngine.XR.Interaction.Toolkit");

        if (xrRaycasterType != null && clonedCanvasInfo.GetComponent(xrRaycasterType) == null)
        {
            clonedCanvasInfo.AddComponent(xrRaycasterType);
        }

        // Ekran seçicisi işleme engel olmasın
        var eventSystem = clonedCanvasInfo.GetComponentInChildren<UnityEngine.EventSystems.EventSystem>();
        if(eventSystem != null) Object.DestroyImmediate(eventSystem.gameObject);

        Debug.Log("İŞLEM TAMAM! Spotify arka plan sahnesinden söküldü, senin sahnene (VR uyumlu ebatta ve tıklanabilir) eklendi.");
    }
}
