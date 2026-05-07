using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// On-screen virtual keyboard for VR text input.
/// Attaches to InputField elements when they are selected.
/// </summary>
public class VRKeyboard : MonoBehaviour
{
    [Header("Settings")]
    public float keySize = 36f;
    public float keySpacing = 3f;
    public Color keyColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    public Color keyTextColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    public Color bgColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    public Color accentColor = new Color(0.12f, 0.84f, 0.38f, 1f);

    private InputField _activeInputField;
    private GameObject _keyboardPanel;
    private bool _isShift = false;

    // Standard QWERTY layout
    private static readonly string[] ROW1 = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
    private static readonly string[] ROW2 = { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p" };
    private static readonly string[] ROW3 = { "a", "s", "d", "f", "g", "h", "j", "k", "l" };
    private static readonly string[] ROW4 = { "z", "x", "c", "v", "b", "n", "m" };

    void Awake()
    {
        BuildKeyboard();
        HideKeyboard();
    }

    void Update()
    {
        // EventSystem takibini kapattık. Gerçek donanım klavyesi (hardware keyboard) 
        // araya girmesin diye artık InputField'ler standart EventSystem üzerinden seçilmeyecek.
    }

    public void OpenForInputField(InputField inf)
    {
        if (inf != null)
        {
            _activeInputField = inf;
            ShowKeyboard();
            UpdateDisplay();
        }
    }

    public void ShowKeyboard()
    {
        if (_keyboardPanel != null)
        {
            _keyboardPanel.SetActive(true);
        }
    }

    public void HideKeyboard()
    {
        if (_keyboardPanel != null)
        {
            _keyboardPanel.SetActive(false);
            _activeInputField = null;
        }
    }

    void BuildKeyboard()
    {
        // Main Panel
        GameObject panel = new GameObject("VRKeyboardPanel");
        panel.transform.SetParent(transform, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        // Move to the right of the UI instead of bottom
        prt.anchorMin = new Vector2(1.02f, 0.15f);
        prt.anchorMax = new Vector2(1.85f, 0.85f);
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;

        panel.AddComponent<CanvasRenderer>();
        Image bg = panel.AddComponent<Image>();
        bg.color = bgColor;

        VerticalLayoutGroup vl = panel.AddComponent<VerticalLayoutGroup>();
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;
        vl.childControlWidth = true;
        vl.childControlHeight = false;
        vl.spacing = keySpacing;
        vl.padding = new RectOffset(10, 10, 10, 10);
        vl.childAlignment = TextAnchor.MiddleCenter;

        _keyboardPanel = panel;

        // Input display row
        BuildInputDisplay(panel.transform);

        // Key rows
        BuildKeyRow(panel.transform, ROW1);
        BuildKeyRow(panel.transform, ROW2);
        BuildKeyRow(panel.transform, ROW3);
        BuildBottomRow(panel.transform, ROW4);
        BuildSpaceRow(panel.transform);
    }

    void BuildInputDisplay(Transform parent)
    {
        GameObject row = new GameObject("InputDisplay");
        row.transform.SetParent(parent, false);
        RectTransform rt = row.AddComponent<RectTransform>();
        LayoutElement le = row.AddComponent<LayoutElement>();
        le.preferredHeight = keySize;

        row.AddComponent<CanvasRenderer>();
        Image bg = row.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.05f, 1f);

        // Display text
        GameObject txtGo = new GameObject("DisplayText");
        txtGo.transform.SetParent(rt, false);
        RectTransform txtRt = txtGo.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = new Vector2(10, 0);
        txtRt.offsetMax = new Vector2(-10, 0);
        txtGo.AddComponent<CanvasRenderer>();
        Text txt = txtGo.AddComponent<Text>();
        txt.text = "";
        txt.fontSize = 16;
        txt.color = keyTextColor;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleLeft;
    }

    void BuildKeyRow(Transform parent, string[] keys)
    {
        GameObject row = new GameObject("KeyRow");
        row.transform.SetParent(parent, false);
        row.AddComponent<RectTransform>();
        LayoutElement le = row.AddComponent<LayoutElement>();
        le.preferredHeight = keySize;

        HorizontalLayoutGroup hl = row.AddComponent<HorizontalLayoutGroup>();
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = true;
        hl.childControlWidth = false;
        hl.childControlHeight = true;
        hl.spacing = keySpacing;
        hl.childAlignment = TextAnchor.MiddleCenter;

        foreach (string k in keys)
        {
            AddKey(row.transform, k, keySize, keyColor, () => TypeChar(k));
        }
    }

    void BuildBottomRow(Transform parent, string[] keys)
    {
        GameObject row = new GameObject("BottomRow");
        row.transform.SetParent(parent, false);
        row.AddComponent<RectTransform>();
        LayoutElement le = row.AddComponent<LayoutElement>();
        le.preferredHeight = keySize;

        HorizontalLayoutGroup hl = row.AddComponent<HorizontalLayoutGroup>();
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = true;
        hl.childControlWidth = false;
        hl.childControlHeight = true;
        hl.spacing = keySpacing;
        hl.childAlignment = TextAnchor.MiddleCenter;

        // Shift key
        AddKey(row.transform, "Shift", keySize * 1.5f, new Color(0.35f, 0.35f, 0.35f, 1f), OnShift);

        foreach (string k in keys)
        {
            AddKey(row.transform, k, keySize, keyColor, () => TypeChar(k));
        }

        // Backspace key
        AddKey(row.transform, "<-", keySize * 1.5f, new Color(0.35f, 0.35f, 0.35f, 1f), OnBackspace);
    }

    void BuildSpaceRow(Transform parent)
    {
        GameObject row = new GameObject("SpaceRow");
        row.transform.SetParent(parent, false);
        row.AddComponent<RectTransform>();
        LayoutElement le = row.AddComponent<LayoutElement>();
        le.preferredHeight = keySize;

        HorizontalLayoutGroup hl = row.AddComponent<HorizontalLayoutGroup>();
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = true;
        hl.childControlWidth = false;
        hl.childControlHeight = true;
        hl.spacing = keySpacing;
        hl.childAlignment = TextAnchor.MiddleCenter;

        // Close button
        AddKey(row.transform, "Close", keySize * 1.5f, new Color(0.6f, 0.15f, 0.15f, 1f), () => HideKeyboard());

        // Space bar
        AddKey(row.transform, "Space", keySize * 5f, keyColor, () => TypeChar(" "));

        // Search / Enter
        AddKey(row.transform, "Enter", keySize * 2f, accentColor, OnEnter);
    }

    void AddKey(Transform parent, string label, float width, Color color, Action onClick)
    {
        GameObject go = new GameObject("Key_" + label);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, keySize);

        go.AddComponent<CanvasRenderer>();
        Image img = go.AddComponent<Image>();
        img.color = color;

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = color;
        cb.highlightedColor = color * 1.3f;
        cb.pressedColor = accentColor;
        btn.colors = cb;
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick?.Invoke());

        go.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        VRButtonLinker linker = go.AddComponent<VRButtonLinker>();
        linker.colliderScaleMultiplier = 1.4f;

        // Label
        GameObject txtGo = new GameObject("Label");
        txtGo.transform.SetParent(rt, false);
        RectTransform txtRt = txtGo.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;
        txtGo.AddComponent<CanvasRenderer>();
        Text txt = txtGo.AddComponent<Text>();
        txt.text = label;
        txt.fontSize = 14;
        txt.color = keyTextColor;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
    }

    // === Input Actions ===

    void TypeChar(string c)
    {
        if (_activeInputField == null) return;

        string ch = _isShift ? c.ToUpper() : c;
        _activeInputField.text += ch;

        if (_isShift) _isShift = false;

        UpdateDisplay();
    }

    void OnBackspace()
    {
        if (_activeInputField == null) return;
        string t = _activeInputField.text;
        if (t.Length > 0)
            _activeInputField.text = t.Substring(0, t.Length - 1);
        UpdateDisplay();
    }

    void OnShift()
    {
        _isShift = !_isShift;
    }

    void OnEnter()
    {
        if (_activeInputField == null) return;
        // Trigger onEndEdit which many search fields listen to
        _activeInputField.onEndEdit.Invoke(_activeInputField.text);

        // Also try to find and click any nearby search button
        Button searchBtn = _activeInputField.GetComponentInParent<SearchViewController>()
            ?.GetComponentInChildren<Button>();
        if (searchBtn != null) searchBtn.onClick.Invoke();

        HideKeyboard();
    }

    void UpdateDisplay()
    {
        // Update the keyboard's display text
        Text displayTxt = _keyboardPanel?.transform.Find("InputDisplay/DisplayText")?.GetComponent<Text>();
        if (displayTxt != null && _activeInputField != null)
            displayTxt.text = _activeInputField.text;
    }
}
