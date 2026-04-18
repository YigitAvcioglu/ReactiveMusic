using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Hover darkening for VR menu buttons.
/// Works WITH VRButtonLinker: darkens on hover, restores on exit.
/// Scene loading is wired separately via Button.onClick in MenuSceneSetup.
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Color originalColor;
    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        originalColor = buttonImage.color;
        Debug.Log($"[MenuButtonHover] Ready on '{gameObject.name}' color={originalColor}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.color = new Color(originalColor.r * 0.35f, originalColor.g * 0.35f, originalColor.b * 0.35f, originalColor.a);
        Debug.Log($"[MenuButtonHover] ENTER '{gameObject.name}'");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.color = originalColor;
        Debug.Log($"[MenuButtonHover] EXIT '{gameObject.name}'");
    }
}
