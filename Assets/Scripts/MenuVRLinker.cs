using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MenuVRLinker : MonoBehaviour, IPointerClickHandler
{
    public float colliderScaleMultiplier = 1.0f;
    public bool darkenOnHover = true;
    public string targetScene;
    
    private Button btn;
    private Image img;
    private Color originalColor;
    private XRSimpleInteractable interactable;
    private bool isPointerOver = false;

    void Awake()
    {
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        if (img != null) originalColor = img.color;
        
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null) interactable = gameObject.AddComponent<XRSimpleInteractable>();

        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) col = gameObject.AddComponent<BoxCollider>();
        col.isTrigger = false;
        
        StartCoroutine(UpdateColliderSizeRoutine(col));
    }

    System.Collections.IEnumerator UpdateColliderSizeRoutine(BoxCollider col)
    {
        yield return new WaitForEndOfFrame();
        yield return null;
        if (GetComponent<RectTransform>() != null && col != null)
        {
            RectTransform rt = GetComponent<RectTransform>();
            col.size = new Vector3(rt.rect.width, rt.rect.height, 20f);
            col.center = Vector3.zero;
        }

        if (interactable != null)
        {
            interactable.hoverEntered.AddListener((args) => OnHoverEnterInternal());
            interactable.hoverExited.AddListener((args) => OnHoverExitInternal());
        }
    }

    private void OnHoverEnterInternal()
    {
        isPointerOver = true;
        if (darkenOnHover && img != null) img.color = originalColor * 0.5f;
    }

    private void OnHoverExitInternal()
    {
        isPointerOver = false;
        if (darkenOnHover && img != null) img.color = originalColor;
    }

    private bool wasTriggerPulled = false;

    void Update()
    {
        bool triggerPulled = false;
        if (Gamepad.current != null && (Gamepad.current.rightTrigger.wasPressedThisFrame || Gamepad.current.leftTrigger.wasPressedThisFrame))
            triggerPulled = true;

        foreach (var d in InputSystem.devices)
        {
            if (d.name.Contains("Controller"))
            {
                var trigger = d.GetChildControl<UnityEngine.InputSystem.Controls.AxisControl>("trigger");
                if (trigger != null && trigger.ReadValue() > 0.5f) triggerPulled = true;
            }
        }

        if (isPointerOver && triggerPulled && !wasTriggerPulled)
        {
           DoInvoke();
        }
        wasTriggerPulled = triggerPulled;
    }

    public void OnPointerClick(PointerEventData eventData) => DoInvoke();

    private void DoInvoke()
    {
        if (btn != null && btn.interactable)
        {
            if (!string.IsNullOrEmpty(targetScene))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
            }
            btn.onClick.Invoke();
        }
    }
}
