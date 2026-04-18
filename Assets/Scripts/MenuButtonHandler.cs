using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// All-in-one menu button handler for VR laser interaction.
/// - BoxCollider auto-sized to button rect
/// - Hover detection via XRSimpleInteractable.hoverEntered (physics raycast, no UI raycast needed)
/// - Click detection via trigger polling (rising-edge only — no auto-click on laser enter)
/// - Hover darkening in-place (no separate MenuButtonHover needed)
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class MenuButtonHandler : MonoBehaviour
{
    public string targetScene;

    private Button btn;
    private Image img;
    private Color originalColor;
    private bool isHovered = false;
    private bool wasTriggerPulled = false;
    private XRSimpleInteractable interactable;

    void Awake()
    {
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        originalColor = img.color;

        // Ensure BoxCollider exists and is sized correctly after layout
        var col = GetComponent<BoxCollider>();
        if (col == null) col = gameObject.AddComponent<BoxCollider>();
        col.isTrigger = false;
        StartCoroutine(FitCollider(col));

        // Add XRSimpleInteractable if missing
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null) interactable = gameObject.AddComponent<XRSimpleInteractable>();

        // Wire HOVER only — NOT selectEntered (which would auto-click when W is held)
        interactable.hoverEntered.AddListener(_ => OnHoverEnter());
        interactable.hoverExited.AddListener(_ => OnHoverExit());
    }

    void Start()
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(Load);
        Debug.Log($"[MenuButtonHandler] Ready: '{gameObject.name}' -> '{targetScene}'");
    }

    IEnumerator FitCollider(BoxCollider col)
    {
        yield return null;
        yield return null;
        var rt = GetComponent<RectTransform>();
        float w = (rt != null && rt.rect.width > 0) ? rt.rect.width : 400f;
        float h = (rt != null && rt.rect.height > 0) ? rt.rect.height : 150f;
        col.size = new Vector3(w, h, 10f);
        col.center = Vector3.zero;
        Debug.Log($"[MenuButtonHandler] Collider fitted: {col.size}");
    }

    void OnHoverEnter()
    {
        isHovered = true;
        img.color = new Color(originalColor.r * 0.4f, originalColor.g * 0.4f, originalColor.b * 0.4f, originalColor.a);
        Debug.Log($"[MenuButtonHandler] HOVER IN '{gameObject.name}'");
    }

    void OnHoverExit()
    {
        isHovered = false;
        img.color = originalColor;
        Debug.Log($"[MenuButtonHandler] HOVER OUT '{gameObject.name}'");
    }

    void Update()
    {
        // Read trigger axis from any XR controller
        bool triggerPulled = false;
        foreach (var device in InputSystem.devices)
        {
            if (device.name.Contains("Controller") || device.name.Contains("Hand"))
            {
                try
                {
                    var trigger = device.GetChildControl<UnityEngine.InputSystem.Controls.AxisControl>("trigger");
                    if (trigger != null && trigger.ReadValue() > 0.5f)
                        triggerPulled = true;
                }
                catch { }
            }
        }

        // Rising-edge check: only fire once per press, and only while hovering
        if (isHovered && triggerPulled && !wasTriggerPulled)
        {
            Debug.Log($"[MenuButtonHandler] TRIGGER CLICK on '{gameObject.name}'");
            btn.onClick.Invoke();
        }

        wasTriggerPulled = triggerPulled;
    }

    void Load()
    {
        if (!string.IsNullOrEmpty(targetScene))
        {
            Debug.Log($"[MenuButtonHandler] Loading '{targetScene}'");
            SceneManager.LoadScene(targetScene);
        }
        else
            Debug.LogWarning($"[MenuButtonHandler] targetScene is empty on '{gameObject.name}'!");
    }
}
