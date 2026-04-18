using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to each menu button. Handles hover darkening and scene loading
/// when triggered via XR laser pointer (TrackedDeviceGraphicRaycaster).
/// Implements IPointerMoveHandler for XR Toolkit compatibility (XR sends PointerMove, not Enter on first frame).
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class MenuButtonLinker : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler,
    IPointerMoveHandler,
    ISubmitHandler
{
    public string targetScene;

    private Color originalColor;
    private Image buttonImage;
    private bool isHovered = false;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        originalColor = buttonImage.color;

        // Wire Button's built-in onClick as well
        GetComponent<Button>().onClick.AddListener(OnButtonClicked);

        Debug.Log($"[MenuButtonLinker] READY on '{gameObject.name}' => targetScene='{targetScene}'" +
                  $" | Canvas: {GetComponentInParent<Canvas>()?.name}" +
                  $" | Raycaster: {GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>() != null}");
    }

    void OnButtonClicked()
    {
        Debug.Log($"[{gameObject.name}] Button.onClick FIRED => loading '{targetScene}'");
        Load();
    }

    void Load()
    {
        if (!string.IsNullOrEmpty(targetScene))
            SceneManager.LoadScene(targetScene);
        else
            Debug.LogWarning($"[{gameObject.name}] targetScene is empty!");
    }

    // Called every frame while XR laser is over this element
    public void OnPointerMove(PointerEventData eventData)
    {
        if (!isHovered)
        {
            isHovered = true;
            Darken();
            Debug.Log($"[{gameObject.name}] OnPointerMove -> hover START");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isHovered)
        {
            isHovered = true;
            Darken();
        }
        Debug.Log($"[{gameObject.name}] OnPointerEnter | hit={eventData?.pointerCurrentRaycast.gameObject?.name}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        Restore();
        Debug.Log($"[{gameObject.name}] OnPointerExit");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[{gameObject.name}] OnPointerClick! button={eventData?.button}");
        Load();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        Debug.Log($"[{gameObject.name}] OnSubmit!");
        Load();
    }

    void Darken()
    {
        buttonImage.color = new Color(
            originalColor.r * 0.35f,
            originalColor.g * 0.35f,
            originalColor.b * 0.35f,
            originalColor.a);
    }

    void Restore()
    {
        buttonImage.color = originalColor;
    }
}