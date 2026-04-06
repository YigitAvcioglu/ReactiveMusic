using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class SpotifyUISetup : Editor
{
    [MenuItem("Spotify/Arayüz Oluştur")]
    public static void CreateUI()
    {
        // 1. Zaten Canvas var mı kontrol et, yoksa oluştur
        Canvas canvas = FindFirstObjectByType<Canvas>();
        GameObject canvasGO;
        if (canvas == null)
        {
            canvasGO = new GameObject("SpotifyCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvasGO = canvas.gameObject;
        }

        // 2. Butonları içine koyacağımız yatay bir arkaplan paneli
        GameObject panelGO = new GameObject("SpotifyController");
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Koyu gri arkaplan
        
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.2f, 0.05f);
        panelRect.anchorMax = new Vector2(0.8f, 0.15f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        HorizontalLayoutGroup hLayout = panelGO.AddComponent<HorizontalLayoutGroup>();
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childControlHeight = false;
        hLayout.childControlWidth = false;
        hLayout.spacing = 30;

        // 3. SpotifySimpleUIControl scriptini Panele (veya Canvas'a) ekle
        var controller = panelGO.AddComponent<SpotifySimpleUIControl>();

        // 4. Metod çağrılarıyla butonları oluştur ve referansları ata
        controller.previousButton = CreateBtnWithParams("Btn_Onceki", "Önceki", panelGO.transform);
        controller.playButton = CreateBtnWithParams("Btn_Oynat", "Oynat", panelGO.transform);
        controller.pauseButton = CreateBtnWithParams("Btn_Duraklat", "Duraklat", panelGO.transform);
        controller.nextButton = CreateBtnWithParams("Btn_Sonraki", "Sonraki", panelGO.transform);

        // 5. Oyunda butona basabilmek için EventSystem'in olması şart
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Debug.Log("✅ Spotify Kullanıcı Arayüzü (UI) başarıyla oluşturuldu.");
    }

    // Buton yaratma yardımcı fonksiyonu
    private static Button CreateBtnWithParams(string name, string textStr, Transform parentTransform)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parentTransform, false);
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.11f, 0.84f, 0.38f); // Spotify Yeşil Rengi
        Button btn = btnGO.AddComponent<Button>();
        
        RectTransform btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(100, 50);
        
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        Text text = textGO.AddComponent<Text>();
        text.text = textStr;
        text.color = Color.white; // Siyah yerine beyaz yazı daha güzel durur
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return btn;
    }

    [MenuItem("Spotify/Arayuzu VR Icin Duzenle")]
    public static void ConvertToVRCanvas()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            // 1. World Space'e geçir
            canvas.renderMode = RenderMode.WorldSpace;
            
            // 2. VR için boyutu küçült (1 birim = 1 metre) ve kullanıcının önüne taşı
            RectTransform rect = canvas.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800, 200);
            rect.localScale = new Vector3(0.002f, 0.002f, 0.002f);
            rect.position = new Vector3(0f, 1.2f, 2f); // Yerden 1.2m yükseklik, 2 metre ileri

            // 3. Normal GraphicRaycaster'ı sil
            var oldRaycaster = canvas.GetComponent<GraphicRaycaster>();
            if (oldRaycaster != null)
            {
                DestroyImmediate(oldRaycaster);
            }

            // 4. VR Lazer uyumlu TrackedDeviceGraphicRaycaster ekle!
            // Bu componentin tam adı projedeki pakete göre değişebilir, o yüzden Reflection ile ekliyoruz
            System.Type vrRaycasterType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
            if (vrRaycasterType != null)
            {
                if (canvas.GetComponent(vrRaycasterType) == null)
                    canvas.gameObject.AddComponent(vrRaycasterType);
                Debug.Log("✅ VR Raycaster başarıyla eklendi! Arayüz artık World Space'de.");
            }
            else
            {
                Debug.LogWarning("⚠️ TrackedDeviceGraphicRaycaster bulunamadı. Lütfen Canvas objesine elle 'TrackedDeviceGraphicRaycaster' ekleyin.");
            }
        }
        else
        {
            Debug.LogWarning("Sahne" + "de Canvas bulunamadı!");
        }
    }
}
