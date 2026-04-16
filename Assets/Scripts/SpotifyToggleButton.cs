using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Spotify UI toggle butonu.
/// Start'ta kendi Button.onClick'ine listener ekler — VRButtonLinker ile çalışır.
/// IPointerClickHandler da desteklenir (TrackedDeviceGraphicRaycaster fallback).
/// </summary>
[RequireComponent(typeof(Button))]
public class SpotifyToggleButton : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Gösterip gizlenecek Spotify Canvas")]
    public Canvas spotifyCanvas;

    private float lastClickTime = -999f;
    private const float DebounceSeconds = 0.5f;

    void Start()
    {
        // VRButtonLinker, Button.onClick.Invoke() çağırıyor.
        // Buradan listener eklersek editörde elle bağlamak gerekmez.
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(Toggle);
            Debug.Log("[SpotifyToggleButton] Button.onClick listener eklendi.");
        }
    }

    // TrackedDeviceGraphicRaycaster'dan gelen tıklamalar için
    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }

    // VRButtonLinker'ın Button.onClick üzerinden çağırdığı metot
    public void Toggle()
    {
        if (Time.time - lastClickTime < DebounceSeconds)
        {
            Debug.Log("[SpotifyToggleButton] Debounce engelledi.");
            return;
        }
        lastClickTime = Time.time;

        if (spotifyCanvas == null)
        {
            Debug.LogWarning("[SpotifyToggleButton] spotifyCanvas atanmamış!");
            return;
        }

        bool newState = !spotifyCanvas.gameObject.activeSelf;
        spotifyCanvas.gameObject.SetActive(newState);
        Debug.Log($"[SpotifyToggleButton] Spotify UI -> {(newState ? "AÇIK" : "KAPALI")}");
    }
}
