using UnityEngine;
using UnityEditor;

public class FixVRHandsAndStage
{
    [MenuItem("Tools/Fix Stage And Remove Hand Cubes", false, 4)]
    public static void FixEverything()
    {
        // 1. Stage Basic Training'i Geri Getir ve Renk Reaktifliğini Ekle
        GameObject trainingStage = GameObject.Find("TrainingStage");
        if (trainingStage == null)
        {
            // Belki kapalı olduğu için Find ile bulunamamıştır, aktif olmayanları da arayalım
            var allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (var t in allTransforms)
            {
                if (t.name == "TrainingStage" || t.name == "PolygonGrid" || t.name == "PolygonGrid_Glow")
                {
                    t.gameObject.SetActive(true);
                    if (t.name == "TrainingStage") trainingStage = t.gameObject;
                }
            }
        }

        if (trainingStage != null)
        {
            // Altındaki tüm kapalı gridleri açıyoruz
            foreach (Transform child in trainingStage.transform)
            {
                child.gameObject.SetActive(true);
            }

            // Sese göre parlayan kodumuzu tekrar ekliyoruz (eğer yoksa)
            if (trainingStage.GetComponent<EnvironmentColorReactive>() == null)
            {
                trainingStage.AddComponent<EnvironmentColorReactive>();
            }
            
            Debug.Log("Basic Training Stage geri getirildi ve sese duyarlı hale getirildi!");
        }

        // 2. VR Ellerindeki Gıcık Küpleri Sil
        // Genelde VR Player objesinin altındaki LeftHand Controller ve RightHand Controller içinde olurlar
        GameObject vrPlayer = GameObject.Find("VR Player");
        if (vrPlayer != null)
        {
            // VR Player içindeki bütün MeshRenderer veya Cube isimli objeleri bulalım
            MeshFilter[] meshes = vrPlayer.GetComponentsInChildren<MeshFilter>(true);
            int deletedCubes = 0;
            
            foreach (var mesh in meshes)
            {
                // Standart Unity Küp filtresi
                if (mesh.sharedMesh != null && mesh.sharedMesh.name == "Cube")
                {
                    // Küpün olduğu objeyi yazarız
                    GameObject cubeObj = mesh.gameObject;
                    
                    // Eğer bu obje ellerin kendisi değil de sadece görsel küpse silelim
                    // (Direkt controller'ı silmemek için adında Cube geçiyorsa veya direkt sadece MeshRenderer ise)
                    if (cubeObj.name.ToLower().Contains("cube") || cubeObj.GetComponent<Camera>() == null)
                    {
                        // Sadece görsel olan render bileşenlerini devre dışı bırakıp/siliyoruz
                        MeshRenderer renderer = cubeObj.GetComponent<MeshRenderer>();
                        if (renderer != null)
                        {
                            Object.DestroyImmediate(renderer);
                            Object.DestroyImmediate(mesh);
                            deletedCubes++;
                        }
                    }
                }
            }
            
            Debug.Log(deletedCubes + " adet el küpü silindi!");
        }
        else
        {
            Debug.LogWarning("VR Player bulunamadı, ellerdeki küplere müdahale edilemedi.");
        }
        
        Debug.Log("Bütün düzeltmeler yapıldı! Sahneyi test edebilirsin.");
    }
}
