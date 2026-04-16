using UnityEngine;
using UnityEditor;
using System.Linq;

public class RoomBuilder
{
    [MenuItem("Tools/Build Closed Room", false, 2)]
    public static void BuildRoom()
    {
        MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        MeshRenderer existingFloor = null;
        MeshRenderer existingWall = null;

        foreach (var r in renderers)
        {
            if (r.transform.parent != null && r.transform.parent.name == "BasicTraining_ClosedRoom") continue;

            if (Mathf.Abs(r.transform.up.y) > 0.9f && existingFloor == null)
            {
                existingFloor = r;
            }
            else if (Mathf.Abs(r.transform.up.y) < 0.1f && existingWall == null)
            {
                existingWall = r;
            }
        }

        if (existingFloor == null && existingWall == null)
        {
            Debug.LogError("Sahnedeki mevcut duvar veya zemin bulunamadı!");
            return;
        }

        GameObject wallPrefab = existingWall != null ? existingWall.gameObject : existingFloor.gameObject;
        GameObject floorPrefab = existingFloor != null ? existingFloor.gameObject : existingWall.gameObject;

        GameObject roomParent = new GameObject("BasicTraining_ClosedRoom");
        Undo.RegisterCreatedObjectUndo(roomParent, "Build Closed Room"); // Unity'nin Geri Alma (Ctrl+Z) hafızasına kaydet

        float roomWidth = 20f;  
        float roomHeight = 10f; 
        
        if (existingFloor != null)
        {
            roomWidth = existingFloor.transform.lossyScale.x * 10f; 
        }

        CreateWall(wallPrefab, new Vector3(0, roomHeight / 2, roomWidth / 2), Quaternion.Euler(90, 0, 0), roomParent.transform, "Wall_North");
        CreateWall(wallPrefab, new Vector3(0, roomHeight / 2, -roomWidth / 2), Quaternion.Euler(90, 0, 0), roomParent.transform, "Wall_South");
        CreateWall(wallPrefab, new Vector3(roomWidth / 2, roomHeight / 2, 0), Quaternion.Euler(90, 90, 0), roomParent.transform, "Wall_East");
        CreateWall(wallPrefab, new Vector3(-roomWidth / 2, roomHeight / 2, 0), Quaternion.Euler(90, 90, 0), roomParent.transform, "Wall_West");
        CreateWall(floorPrefab, new Vector3(0, roomHeight, 0), Quaternion.Euler(180, 0, 0), roomParent.transform, "Ceiling");
        CreateWall(floorPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), roomParent.transform, "Floor_Center");

        if (existingWall != null && existingWall.transform.parent != roomParent.transform)
        {
            Undo.RecordObject(existingWall.gameObject, "Hide Original Wall");
            existingWall.gameObject.SetActive(false);
        }
        if (existingFloor != null && existingFloor.transform.parent != roomParent.transform)
        {
            Undo.RecordObject(existingFloor.gameObject, "Hide Original Floor");
            existingFloor.gameObject.SetActive(false);
        }

        Debug.Log("Oda başarıyla kapatıldı! Tüm duvarlar ve tavan oluşturuldu.");
    }

    private static void CreateWall(GameObject referenceObj, Vector3 pos, Quaternion rot, Transform parent, string name)
    {
        GameObject newWall = Object.Instantiate(referenceObj, pos, rot, parent);
        newWall.name = name;
    }

    // --- YAPILAN İŞLEMLERİ GERİ ALMA BUTONU ---
    [MenuItem("Tools/Undo Closed Room", false, 3)]
    public static void UndoRoom()
    {
        // 1. Oluşturduğumuz "BasicTraining_ClosedRoom" objesini bul ve sil
        GameObject roomParent = GameObject.Find("BasicTraining_ClosedRoom");
        if (roomParent != null)
        {
            Object.DestroyImmediate(roomParent);
        }

        // 2. Sahnedeki görünmez (kapalı) hale getirdiğimiz tüm MeshRenderer objelerini bul ve tekrar aç
        MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var r in renderers)
        {
            // Eğer görünmez durumdaysa tekrar aktif hale getir (sadece orijinal gizlenmiş duvarları açar)
            if (!r.gameObject.activeInHierarchy)
            {
                r.gameObject.SetActive(true);
            }
        }
        
        Debug.Log("Oluşturulan oda silindi ve gizlenen eski duvarlar geri getirildi!");
    }
}
