using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// Handles VR interaction for UI buttons.
/// Supports both XR Interactors (via Collider/SimpleInteractable) 
/// and Standard UI Raycasters (via IPointer handlers).
/// </summary>
public class VRButtonLinker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public float colliderScaleMultiplier = 1.0f;
    public bool darkenOnHover = true;
    public string targetScene;

    private Button btn;
    private Image img;
    private Color originalColor;
    private XRSimpleInteractable interactable;
    private bool isPointerOver = false;
    private bool wasTriggerPulled = false;

    void Awake()
    {
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        if (img != null) originalColor = img.color;

        // Only add interaction components if they don't exist
        interactable = GetComponent<XRSimpleInteractable>();
        
        BoxCollider col = GetComponent<BoxCollider>();
        
        // In the Menu scene, we might need these automatically, 
        // but in other scenes we respect existing setup.
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Menu")
        {
            if (interactable == null) interactable = gameObject.AddComponent<XRSimpleInteractable>();
            if (col == null) 
            {
                col = gameObject.AddComponent<BoxCollider>();
                StartCoroutine(UpdateColliderSizeRoutine(col));
            }
        }

        if (interactable != null)
        {
            interactable.hoverEntered.AddListener((args) => OnHoverEnterInternal());
            interactable.hoverExited.AddListener((args) => OnHoverExitInternal());
        }
    }

    System.Collections.IEnumerator UpdateColliderSizeRoutine(BoxCollider col)
    {
        yield return new WaitForEndOfFrame();
        if (GetComponent<RectTransform>() != null && col != null)
        {
            RectTransform rt = GetComponent<RectTransform>();
            col.size = new Vector3(rt.rect.width, rt.rect.height, 10f);
            col.center = Vector3.zero;
        }
    }

    // Standard UI Handlers
    public void OnPointerEnter(PointerEventData eventData) => OnHoverEnterInternal();
    public void OnPointerExit(PointerEventData eventData) => OnHoverExitInternal();
    public void OnPointerClick(PointerEventData eventData) => DoInvoke();

    private void OnHoverEnterInternal()
    {
        isPointerOver = true;
        if (darkenOnHover && img != null) img.color = originalColor * 0.7f;
    }

    private void OnHoverExitInternal()
    {
        isPointerOver = false;
        if (darkenOnHover && img != null) img.color = originalColor;
    }

    void Update()
    {
        // Polling interaction for VR controllers (Manual trigger logic)
        bool triggerPulled = CheckTriggers();

        if (isPointerOver && triggerPulled && !wasTriggerPulled)
        {
            DoInvoke();
        }
        wasTriggerPulled = triggerPulled;
    }

    private bool CheckTriggers()
    {
        if (Gamepad.current != null && (Gamepad.current.rightTrigger.wasPressedThisFrame || Gamepad.current.leftTrigger.wasPressedThisFrame))
            return true;

        foreach (var d in InputSystem.devices)
        {
            if (d.name.Contains("Controller"))
            {
                var trigger = d.GetChildControl<UnityEngine.InputSystem.Controls.AxisControl>("trigger");
                if (trigger != null && trigger.ReadValue() > 0.5f) return true;
            }
        }
        return false;
    }

    private void DoInvoke()
    {
        if (btn != null && btn.interactable)
        {
            // Safety: Only load scene if specifically set
            if (!string.IsNullOrEmpty(targetScene))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
            }
            btn.onClick.Invoke();
        }
    }
}
