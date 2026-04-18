using UnityEngine;
using UnityEditor;

public class CleanupSpotifyHierarchy
{
    [MenuItem("Tools/1-Click Clean and Fix Spotify System", false, 7)]
    public static void CleanAndFix()
    {
        // 1. Orijinal Spotify Controller'ı güvenli bir yere taşı (Eğer Canvas'ın içindeyse)
        GameObject oldCanvas = GameObject.Find("SpotifyCanvas");
        if (oldCanvas != null)
        {
            Transform controller = oldCanvas.transform.Find("SpotifyController");
            if (controller != null)
            {
                Undo.SetTransformParent(controller, null, "Unparent Spotify Controller");
                controller.SetSiblingIndex(3);
                Debug.Log("SpotifyController güvenli bir şekilde Canvas'tan çıkarıldı.");
            }

            // Eski canvası tamamen yok et
            Undo.DestroyObjectImmediate(oldCanvas);
            Debug.Log("Eski bozuk SpotifyCanvas başarıyla temizlendi.");
        }

        // 2. Gereksiz VRButtonLinker kalıntılarını (Eski sistemin çakışma yaratan kodu) temizle
        GameObject vrPlayer = GameObject.Find("VR Player");
        if (vrPlayer != null)
        {
            var btnLinker = vrPlayer.GetComponent("VRButtonLinker");
            if (btnLinker != null)
            {
                Undo.DestroyObjectImmediate(btnLinker);
                Debug.Log("Çakışma yapan eski VRButtonLinker silindi.");
            }
        }

        // 3. VR Arayüzüne eksik olabilecek GraphicRaycaster'ı bağla (Standart XR etkileşimi için)
        GameObject newCanvas = GameObject.Find("Spotify_Full_VR_Canvas");
        if (newCanvas != null)
        {
            UnityEngine.UI.GraphicRaycaster oldRay = newCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (oldRay != null) Undo.DestroyObjectImmediate(oldRay);

            System.Type xrRaycasterType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit")
                ?? System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, UnityEngine.XR.Interaction.Toolkit");

            if (xrRaycasterType != null && newCanvas.GetComponent(xrRaycasterType) == null)
            {
                Undo.AddComponent(newCanvas, xrRaycasterType);
            }
        }

        Debug.Log("Sistem temizlendi! Eski eklentiler yeni UI'ı bozuyordu, hepsi kaldırıldı. Şimdi Play'e basıp deneyebilirsin.");
    }
}
