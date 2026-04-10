using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Builds and manages the full Spotify UI at runtime.
/// Reuses Spotify4Unity's existing prefabs and controllers.
/// Designed for World-Space Canvas in VR.
/// </summary>
public class SpotifyFullUI : MonoBehaviour
{
    [Header("View Prefabs (from Spotify4Unity)")]
    public GameObject landingViewPrefab;
    public GameObject playlistViewPrefab;
    public GameObject searchViewPrefab;
    public GameObject likedSongsViewPrefab;

    [Header("Item Prefabs (from Spotify4Unity)")]
    public GameObject singleNavPlaylistPrefab;
    public GameObject loadingSpinnerPrefab;

    [Header("Player Sprites")]
    public Sprite playSprite;
    public Sprite pauseSprite;
    public Sprite muteSprite;
    public Sprite unmuteSprite;
    public Sprite shuffleSprite;
    public Sprite repeatSprite;

    [Header("Settings")]
    public float canvasWidth = 1200f;
    public float canvasHeight = 700f;
    public float sidebarWidth = 220f;
    public float playerBarHeight = 80f;

    // Colors
    private static readonly Color BgBlack = new Color(0.07f, 0.07f, 0.07f, 1f);
    private static readonly Color BgDark = new Color(0.12f, 0.12f, 0.12f, 1f);
    private static readonly Color BgLight = new Color(0.18f, 0.18f, 0.18f, 1f);
    private static readonly Color SpotifyGreen = new Color(0.12f, 0.84f, 0.38f, 1f);
    private static readonly Color TextWhite = new Color(0.9f, 0.9f, 0.9f, 1f);
    private static readonly Color TextGray = new Color(0.7f, 0.7f, 0.7f, 1f);

    // Runtime references
    private AppMainContentController _contentController;
    private PlaylistsNavigationController _navController;
    private SpotifyPlayerController _playerController;

    // Buton referansları — Update'te raycast için
    private UnityEngine.UI.Button _ppBtn, _prevBtn, _nextBtn;

    void Start()
    {
        BuildUI();
    }

    void Update()
    {
        // Normal Update işlemleri eklenebilir, input loglarına artık gerek yok
    }

    void BuildUI()
    {
#if UNITY_EDITOR
        if (shuffleSprite == null) shuffleSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Spotify4Unity/Examples/Images/UI/media-shuffle.png");
        if (repeatSprite == null) repeatSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Spotify4Unity/Examples/Images/UI/media-repeat-none.png");
        if (playSprite == null) playSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Spotify4Unity/Examples/Images/UI/media-play.png");
        if (pauseSprite == null) pauseSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Spotify4Unity/Examples/Images/UI/media-pause.png");
#endif
        // Clear children first if rebuilding
        foreach (Transform child in transform) {
            if (child.name == "MainCanvas") DestroyImmediate(child.gameObject);
        }

        // Canvas worldCamera ayarla — GraphicRaycaster koordinat dönüşümü için zorunlu
        var canvas = GetComponent<UnityEngine.Canvas>();
        if (canvas != null)
        {
            if (canvas.worldCamera == null)
                canvas.worldCamera = Camera.main;

            // Sadece Lazerin etkileşmesini sağlamak, Mouse ile fiziksel ekran tıklamasını iptal etmek için GraphicRaycaster silinir.
            var gr = GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (gr != null)
            {
                Destroy(gr);
                Debug.Log("[SpotifyFullUI] GraphicRaycaster silindi (Fare tıklaması devre dışı, sadece Lazer kullanılacak).");
            }
            Debug.Log($"[SpotifyFullUI] Canvas worldCamera: {canvas.worldCamera?.name ?? "NULL"} | renderMode:{canvas.renderMode}");
        }


        // Root panel
        RectTransform root = MakePanel("SpotifyRoot", transform, BgBlack);
        FillParent(root);

        VerticalLayoutGroup mainVert = root.gameObject.AddComponent<VerticalLayoutGroup>();
        mainVert.childForceExpandWidth = true;
        mainVert.childForceExpandHeight = false;
        mainVert.childControlWidth = true;
        mainVert.childControlHeight = true;
        mainVert.spacing = 0;

        // Top Section (sidebar + content)
        RectTransform topSection = MakePanel("TopSection", root, Color.clear);
        LayoutElement topLE = topSection.gameObject.AddComponent<LayoutElement>();
        topLE.flexibleHeight = 1;
        topLE.flexibleWidth = 1;

        HorizontalLayoutGroup topHz = topSection.gameObject.AddComponent<HorizontalLayoutGroup>();
        topHz.childForceExpandWidth = false;
        topHz.childForceExpandHeight = true;
        topHz.childControlWidth = true;
        topHz.childControlHeight = true;
        topHz.spacing = 2;

        // Build sections
        BuildSidebar(topSection);
        BuildMainContent(topSection);
        BuildPlayerBar(root);
    }

    // =========== SIDEBAR ===========
    void BuildSidebar(RectTransform parent)
    {
        RectTransform sidebar = MakePanel("Sidebar", parent, BgDark);
        LayoutElement sLE = sidebar.gameObject.AddComponent<LayoutElement>();
        sLE.preferredWidth = sidebarWidth;
        sLE.minWidth = sidebarWidth;

        VerticalLayoutGroup vl = sidebar.gameObject.AddComponent<VerticalLayoutGroup>();
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;
        vl.childControlWidth = true;
        vl.childControlHeight = false;
        vl.padding = new RectOffset(10, 10, 15, 10);
        vl.spacing = 2;

        // Nav buttons
        RectTransform homeBtn = MakeNavBtn("HomeBtn", sidebar, "Home");
        RectTransform searchBtn = MakeNavBtn("SearchBtn", sidebar, "Search");
        RectTransform likedBtn = MakeNavBtn("LikedBtn", sidebar, "Liked Songs");

        // Separator
        RectTransform sep = MakePanel("Sep", sidebar, new Color(0.3f, 0.3f, 0.3f, 1f));
        LayoutElement sepLE = sep.gameObject.AddComponent<LayoutElement>();
        sepLE.preferredHeight = 1;
        sepLE.minHeight = 1;

        // Playlist scroll area
        GameObject scrollGo = new GameObject("PlaylistScroll");
        scrollGo.transform.SetParent(sidebar, false);
        scrollGo.AddComponent<RectTransform>();
        LayoutElement scrollLE = scrollGo.AddComponent<LayoutElement>();
        scrollLE.flexibleHeight = 1;

        ScrollRect sr = scrollGo.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;

        RectTransform vp = MakePanel("Viewport", scrollGo.transform, Color.clear);
        FillParent(vp);
        Mask mask = vp.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        sr.viewport = vp;

        RectTransform content = MakePanel("PlaylistContent", vp, Color.clear);
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1);

        VerticalLayoutGroup cvl = content.gameObject.AddComponent<VerticalLayoutGroup>();
        cvl.childForceExpandWidth = true;
        cvl.childForceExpandHeight = false;
        cvl.childControlWidth = true;
        cvl.childControlHeight = false;
        cvl.spacing = 1;
        cvl.padding = new RectOffset(0, 0, 5, 5);

        ContentSizeFitter csf = content.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = content;

        // PlaylistsNavigationController
        _navController = sidebar.gameObject.AddComponent<PlaylistsNavigationController>();
        SetField(_navController, "_homeNavBtn", homeBtn.GetComponentInChildren<Button>());
        SetField(_navController, "_searchNavBtn", searchBtn.GetComponentInChildren<Button>());
        SetField(_navController, "_likedSongsNavBtn", likedBtn.GetComponentInChildren<Button>());
        SetField(_navController, "_playlistPrefab", singleNavPlaylistPrefab);
        SetField(_navController, "_listViewParent", content.transform);
        if (loadingSpinnerPrefab != null)
            SetField(_navController, "_loadingSpinnerPrefab", loadingSpinnerPrefab);
    }

    // =========== MAIN CONTENT ===========
    void BuildMainContent(RectTransform parent)
    {
        RectTransform mc = MakePanel("MainContent", parent, BgBlack);
        LayoutElement mcLE = mc.gameObject.AddComponent<LayoutElement>();
        mcLE.flexibleWidth = 1;
        mcLE.flexibleHeight = 1;

        _contentController = mc.gameObject.AddComponent<AppMainContentController>();
        _contentController.ViewPrefabs = new List<GameObject>
        {
            landingViewPrefab,
            playlistViewPrefab,
            searchViewPrefab,
            likedSongsViewPrefab
        };
        SetField(_contentController, "_viewsParent", mc.transform);

        if (_navController != null)
            SetField(_navController, "_mainContentController", _contentController);
    }

    // =========== PLAYER BAR ===========
    void BuildPlayerBar(RectTransform parent)
    {
        // 1. MAIN BAR (Height: 80)
        RectTransform bar = MakePanel("PlayerBar", parent, BgLight);
        LayoutElement barLE = bar.gameObject.AddComponent<LayoutElement>();
        barLE.preferredHeight = 80;
        barLE.minHeight = 80;

        HorizontalLayoutGroup hz = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
        hz.childForceExpandWidth = false;
        hz.childForceExpandHeight = false;
        hz.childControlWidth = false;
        hz.childControlHeight = false; // Prevents everything from stretching vertically
        hz.padding = new RectOffset(15, 15, 0, 0);
        hz.spacing = 15;
        hz.childAlignment = TextAnchor.MiddleCenter;

        // --- LEFT: Track Info (Width: 250) ---
        RectTransform trackInfo = MakePanel("TrackInfo", bar, Color.clear);
        trackInfo.sizeDelta = new Vector2(250, 80);
        
        HorizontalLayoutGroup tiHz = trackInfo.gameObject.AddComponent<HorizontalLayoutGroup>();
        tiHz.childForceExpandWidth = false;
        tiHz.childControlWidth = false;
        tiHz.childForceExpandHeight = false;
        tiHz.childControlHeight = false;
        tiHz.spacing = 10;
        tiHz.childAlignment = TextAnchor.MiddleLeft;

        RectTransform albumArt = MakePanel("AlbumArt", trackInfo, Color.white);
        albumArt.sizeDelta = new Vector2(56, 56);

        RectTransform trackTexts = MakePanel("TrackTexts", trackInfo, Color.clear);
        trackTexts.sizeDelta = new Vector2(140, 56);
        VerticalLayoutGroup ttVl = trackTexts.gameObject.AddComponent<VerticalLayoutGroup>();
        ttVl.childAlignment = TextAnchor.MiddleLeft;
        ttVl.childControlHeight = false;
        ttVl.childControlWidth = false;
        ttVl.spacing = 2;

        Text trackNameTxt = MakeText("TrackName", trackTexts, "No track playing", 13, TextWhite);
        trackNameTxt.rectTransform.sizeDelta = new Vector2(140, 18);
        Text artistNameTxt = MakeText("ArtistName", trackTexts, "", 11, TextGray);
        artistNameTxt.rectTransform.sizeDelta = new Vector2(140, 16);

        RectTransform addLibBtn = MakeBtn("AddToLibrary", trackInfo, "♡", 18, TextGray, 32);

        // --- CENTER: Controls (Flexible) ---
        RectTransform ctrlPanel = MakePanel("Controls", bar, Color.clear);
        // Fixed width for center panel to ensure centering when sides are 250
        ctrlPanel.sizeDelta = new Vector2(400, 80); 

        VerticalLayoutGroup ctrlVl = ctrlPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        ctrlVl.childForceExpandWidth = false;
        ctrlVl.childForceExpandHeight = false;
        ctrlVl.childControlWidth = true;
        ctrlVl.childControlHeight = false;
        ctrlVl.childAlignment = TextAnchor.MiddleCenter;
        ctrlVl.spacing = 6; // Restored spacing

        // Button row
        RectTransform btnRow = MakePanel("ButtonRow", ctrlPanel, Color.clear);
        btnRow.sizeDelta = new Vector2(400, 40);
        HorizontalLayoutGroup brHz = btnRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        brHz.childForceExpandWidth = false;
        brHz.childForceExpandHeight = false;
        brHz.childControlWidth = false;
        brHz.childControlHeight = false;
        brHz.spacing = 20;
        brHz.childAlignment = TextAnchor.MiddleCenter;

        RectTransform shuffleBtn = MakeBtn("ShuffleBtn", btnRow, "S", 12, TextGray, 28, shuffleSprite);
        RectTransform prevBtn = MakeBtn("PrevBtn", btnRow, "<<", 14, TextWhite, 32);
        RectTransform ppBtn = MakePlayPauseBtn("PlayPauseBtn", btnRow);
        RectTransform nextBtn = MakeBtn("NextBtn", btnRow, ">>", 14, TextWhite, 32);
        RectTransform repeatBtn = MakeBtn("RepeatBtn", btnRow, "R", 12, TextGray, 28, repeatSprite);

        // Progress row
        RectTransform progRow = MakePanel("ProgressRow", ctrlPanel, Color.clear);
        progRow.sizeDelta = new Vector2(400, 20);
        HorizontalLayoutGroup prHz = progRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        prHz.childForceExpandWidth = false;
        prHz.childForceExpandHeight = false;
        prHz.childControlWidth = false;
        prHz.childControlHeight = false;
        prHz.spacing = 10;
        prHz.childAlignment = TextAnchor.MiddleCenter;

        Text curTimeTxt = MakeText("CurrentTime", progRow, "0:00", 11, TextGray);
        curTimeTxt.rectTransform.sizeDelta = new Vector2(45, 20);
        Slider progressSlider = MakeSlider("ProgressSlider", progRow);
        progressSlider.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 20);
        Text totalTimeTxt = MakeText("TotalTime", progRow, "0:00", 11, TextGray);
        totalTimeTxt.rectTransform.sizeDelta = new Vector2(45, 20);
        totalTimeTxt.alignment = TextAnchor.MiddleRight;

        // --- RIGHT: Volume (Width: 250) ---
        RectTransform volPanel = MakePanel("VolumePanel", bar, Color.clear);
        volPanel.sizeDelta = new Vector2(250, 80);

        HorizontalLayoutGroup vpHz = volPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
        vpHz.childForceExpandWidth = false;
        vpHz.childForceExpandHeight = false;
        vpHz.childControlWidth = false;
        vpHz.childControlHeight = false;
        vpHz.spacing = 10;
        vpHz.childAlignment = TextAnchor.MiddleRight;

        RectTransform muteBtn = MakeBtn("MuteBtn", volPanel, "Vol", 12, TextWhite, 32);
        Slider volumeSlider = MakeSlider("VolumeSlider", volPanel);
        // Restored Volume Slider to 150
        volumeSlider.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 20);

        // --- Controller Binding ---
        _playerController = FindObjectOfType<SpotifyPlayerController>();
        if (_playerController == null) _playerController = bar.gameObject.AddComponent<SpotifyPlayerController>();

        SetField(_playerController, "_trackIcon", albumArt.GetComponent<Image>());
        SetField(_playerController, "_trackName", trackNameTxt);
        SetField(_playerController, "_artistsNames", artistNameTxt);
        SetField(_playerController, "_addToLibraryButton", addLibBtn.GetComponent<Button>());
        SetField(_playerController, "_currentProgressText", curTimeTxt);
        SetField(_playerController, "_totalProgressText", totalTimeTxt);
        SetField(_playerController, "_currentProgressSlider", progressSlider);
        SetField(_playerController, "_playPauseButton", ppBtn.GetComponent<Button>());
        SetField(_playerController, "_previousButton", prevBtn.GetComponent<Button>());
        SetField(_playerController, "_nextButton", nextBtn.GetComponent<Button>());
        SetField(_playerController, "_shuffleButton", shuffleBtn.GetComponent<Button>());
        SetField(_playerController, "_repeatButton", repeatBtn.GetComponent<Button>());
        SetField(_playerController, "_shuffleButton", shuffleBtn.GetComponent<Button>());
        SetField(_playerController, "_repeatButton", repeatBtn.GetComponent<Button>());
        SetField(_playerController, "_volumeSlider", volumeSlider);
        SetField(_playerController, "_muteButton", muteBtn.GetComponent<Button>());

        if (playSprite != null) SetField(_playerController, "_playSprite", playSprite);
        if (pauseSprite != null) SetField(_playerController, "_pauseSprite", pauseSprite);
        if (muteSprite != null) SetField(_playerController, "_muteSprite", muteSprite);
        if (unmuteSprite != null) SetField(_playerController, "_unmuteSprite", unmuteSprite);
    }

    // =========== UI HELPER METHODS ===========

    RectTransform MakePanel(string n, Transform p, Color c)
    {
        GameObject go = new GameObject(n);
        go.layer = 5; // UI Layer
        go.transform.SetParent(p, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        go.AddComponent<CanvasRenderer>();
        Image img = go.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = (c.a > 0.01f);
        return rt;
    }

    Text MakeText(string n, Transform p, string txt, int size, Color c)
    {
        GameObject go = new GameObject(n);
        go.layer = 5; // UI Layer
        go.transform.SetParent(p, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<CanvasRenderer>();
        Text t = go.AddComponent<Text>();
        t.text = txt;
        t.fontSize = size;
        t.color = c;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.alignment = TextAnchor.MiddleLeft;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        return t;
    }

    RectTransform MakeNavBtn(string n, Transform p, string label)
    {
        GameObject go = new GameObject(n);
        go.layer = 5; // UI Layer
        go.transform.SetParent(p, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        go.AddComponent<CanvasRenderer>();
        Image img = go.AddComponent<Image>();
        img.color = Color.white;

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(1, 1, 1, 0.02f);
        cb.highlightedColor = new Color(1, 1, 1, 0.1f);
        cb.pressedColor = new Color(1, 1, 1, 0.2f);
        btn.colors = cb;
        btn.targetGraphic = img;

        go.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        go.AddComponent<VRButtonLinker>();

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 36;
        le.minHeight = 36;

        Text t = MakeText(n + "Txt", rt, label, 14, TextGray);
        FillParent(t.GetComponent<RectTransform>());

        return rt;
    }

    RectTransform MakeBtn(string n, Transform p, string label, int fs, Color tc, float sz, Sprite s = null)
    {
        GameObject go = new GameObject(n);
        go.layer = 5; 
        go.transform.SetParent(p, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(sz, sz);

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredWidth = sz;
        le.preferredHeight = sz;

        go.AddComponent<CanvasRenderer>();
        Image img = go.AddComponent<Image>();
        img.color = Color.clear; 
        
        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1, 1, 1, 0.1f);
        cb.pressedColor = new Color(1, 1, 1, 0.2f);
        btn.colors = cb;
        btn.targetGraphic = img;

        go.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        go.AddComponent<VRButtonLinker>();

        // Child 0: Icon (Required by SpotifyPlayerController)
        GameObject ic = new GameObject(n + "Icon");
        ic.layer = 5;
        ic.transform.SetParent(rt, false);
        RectTransform icRt = ic.AddComponent<RectTransform>();
        FillParent(icRt);
        ic.AddComponent<CanvasRenderer>();
        Image icImg = ic.AddComponent<Image>();
        icImg.color = s != null ? Color.white : Color.clear;
        icImg.sprite = s;
        icImg.preserveAspect = true;

        if (s == null)
        {
            Text t = MakeText(n + "Lbl", icRt, label, fs, tc);
            FillParent(t.GetComponent<RectTransform>());
            t.alignment = TextAnchor.MiddleCenter;
        }

        return rt;
    }

    RectTransform MakePlayPauseBtn(string n, Transform p)
    {
        GameObject go = new GameObject(n);
        go.layer = 5; 
        go.transform.SetParent(p, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(40, 40);

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 40;
        le.preferredHeight = 40;

        go.AddComponent<CanvasRenderer>();
        Image bgImg = go.AddComponent<Image>();
        bgImg.color = Color.white;

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = SpotifyGreen;
        cb.highlightedColor = new Color(0.12f, 0.94f, 0.48f, 1f); // Brighter green
        cb.pressedColor = new Color(0.12f, 0.74f, 0.28f, 1f); // Darker green
        btn.colors = cb;
        btn.targetGraphic = bgImg;

        go.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        go.AddComponent<VRButtonLinker>();

        // Child 0: Icon (Required by SpotifyPlayerController)
        GameObject ic = new GameObject("Icon");
        ic.layer = 5;
        ic.transform.SetParent(rt, false);
        RectTransform icRt = ic.AddComponent<RectTransform>();
        FillParent(icRt);
        ic.AddComponent<CanvasRenderer>();
        Image icImg = ic.AddComponent<Image>();
        icImg.color = Color.white;
        icImg.sprite = playSprite;

        if (playSprite == null)
        {
            icImg.color = Color.clear;
            Text t = MakeText(n + "Txt", icRt, ">", 18, Color.black);
            FillParent(t.rectTransform);
            t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        return rt;
    }

    Slider MakeSlider(string n, Transform p)
    {
        GameObject go = new GameObject(n);
        go.layer = 5; // UI Layer
        go.transform.SetParent(p, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 16);

        Slider sl = go.AddComponent<Slider>();
        sl.minValue = 0;
        sl.maxValue = 100;
        sl.value = 0;

        // BG
        GameObject bgGo = new GameObject("BG");
        bgGo.layer = 5; // UI Layer
        bgGo.transform.SetParent(rt, false);
        RectTransform bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.25f);
        bgRt.anchorMax = new Vector2(1, 0.75f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        bgGo.AddComponent<CanvasRenderer>();
        Image bgI = bgGo.AddComponent<Image>();
        bgI.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        // Fill Area
        GameObject fa = new GameObject("FillArea");
        fa.layer = 5; // UI Layer
        fa.transform.SetParent(rt, false);
        RectTransform faRt = fa.AddComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0, 0.25f);
        faRt.anchorMax = new Vector2(1, 0.75f);
        faRt.offsetMin = Vector2.zero;
        faRt.offsetMax = Vector2.zero;

        GameObject fl = new GameObject("Fill");
        fl.layer = 5; // UI Layer
        fl.transform.SetParent(faRt, false);
        RectTransform flRt = fl.AddComponent<RectTransform>();
        flRt.anchorMin = Vector2.zero;
        flRt.anchorMax = new Vector2(0, 1);
        flRt.offsetMin = Vector2.zero;
        flRt.offsetMax = Vector2.zero;
        fl.AddComponent<CanvasRenderer>();
        Image flI = fl.AddComponent<Image>();
        flI.color = SpotifyGreen;
        sl.fillRect = flRt;

        // Handle Area (Slide Area)
        GameObject ha = new GameObject("HandleArea");
        ha.layer = 5;
        ha.transform.SetParent(rt, false);
        RectTransform haRt = ha.AddComponent<RectTransform>();
        haRt.anchorMin = new Vector2(0, 0);
        haRt.anchorMax = new Vector2(1, 1);
        haRt.offsetMin = new Vector2(10, 0);
        haRt.offsetMax = new Vector2(-10, 0);

        GameObject h = new GameObject("Handle");
        h.layer = 5;
        h.transform.SetParent(haRt, false);
        RectTransform hRt = h.AddComponent<RectTransform>();
        hRt.sizeDelta = new Vector2(12, 12);
        h.AddComponent<CanvasRenderer>();
        Image hI = h.AddComponent<Image>();
        hI.color = Color.white;
        sl.handleRect = hRt;
        sl.targetGraphic = hI;

        // Default constraints - SpotifyPlayerController replaces these
        sl.minValue = 0;
        sl.maxValue = 1; 
        sl.value = 0;

        return sl;
    }

    void FillParent(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void SetField(object obj, string name, object val)
    {
        System.Type t = obj.GetType();
        while (t != null)
        {
            var f = t.GetField(name,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            if (f != null) { f.SetValue(obj, val); return; }
            t = t.BaseType;
        }
        Debug.LogWarning("[SpotifyFullUI] Field not found: " + name);
    }
}
