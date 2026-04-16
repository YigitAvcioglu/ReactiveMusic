using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupTrainingStage
{
    [MenuItem("Tools/Setup Basic Training Stage with Audio Reactive", false, 1)]
    public static void Setup()
    {
        // Yüklü olan mevcut sahneyi kaydedelim
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorSceneManager.SaveOpenScenes();
        }

        // Training Stage sahnesini açalım
        string scenePath = "Assets/SDF_BasicTrainingStage/Scenes/Stage_BasicTraining.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        Debug.Log("Basic Training Stage açıldı!");

        // 1. Audio Reactive Sistemlerini Bul veya Oluştur
        GameObject audioHub = GameObject.Find("AudioAnalyzerHub");
        if (audioHub == null)
        {
            // AudioReactiveSetup menüsündeki gibi oluşturmak istersen
            AudioReactiveSetup.SetupScene();
        }

        // 2. Işıkları Hesapla (Yukarıdaki hatayı önlemek için)
        Lightmapping.BakeAsync();
        Debug.Log("Işık hesaplamaları (Baking) başlatıldı, sağ alttan takip edebilirsiniz.");
    }
}
