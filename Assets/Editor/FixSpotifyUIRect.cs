using UnityEngine;
using UnityEditor;

public class FixSpotifyUIRect
{
    [MenuItem("Tools/Fix Spotify UI Layout", false, 6)]
    public static void FixRect()
    {
        GameObject canvasObj = GameObject.Find("Spotify_Full_VR_Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("Spotify_Full_VR_Canvas bulunamadı!");
            return;
        }

        RectTransform rect = canvasObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            Undo.RecordObject(rect, "Fix UI Size");
            
            // World Space Canvas'larda UI'ın düzgün render olması için 
            // ekran çözünürlüğü gibi sabit piksel değerlerine ihtiyacımız var (örn. 1920 x 1080 veya 1280 x 720).
            rect.sizeDelta = new Vector2(1280, 720); // 16:9 geniş ekran ölçüsü
            
            // Pivot noktasını tam ortaya alıyoruz
            rect.pivot = new Vector2(0.5f, 0.5f);
            
            // Konumunu biraz daha insani ve göz hizası yapalım
            // Yukarı kaldır, biraz geriye al
            rect.transform.position = new Vector3(0f, 1.5f, 1.0f);
            
            // Scale'i 1280x720 çok büyük olmasın diye ayarlayalım
            // 1280 * 0.001 = 1.28 metre genişlik yapar. Bu ideal bir VR televizyon boyutudur.
            rect.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);

            Debug.Log("Arayüzün ekran kayması ve bozukluğu giderildi! Dev 16:9 televizyon ekranı gibi sabitlendi.");
        }
    }
}
