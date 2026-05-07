using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(InputField))]
public class VRInputFieldLinker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private InputField _inputField;
    private XRSimpleInteractable _interactable;
    private bool _isPointerOver = false;
    private bool _wasTriggerPulled = false;

    void Awake()
    {
        _inputField = GetComponent<InputField>();

        // Only add interaction components if they don't exist
        _interactable = GetComponent<XRSimpleInteractable>();
        if (_interactable == null)
        {
            _interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }

        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            StartCoroutine(UpdateColliderSizeRoutine(col));
        }

        if (_interactable != null)
        {
            _interactable.hoverEntered.AddListener((args) => OnHoverEnterInternal());
            _interactable.hoverExited.AddListener((args) => OnHoverExitInternal());
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

    public void OnPointerEnter(PointerEventData eventData) => OnHoverEnterInternal();
    public void OnPointerExit(PointerEventData eventData) => OnHoverExitInternal();
    public void OnPointerClick(PointerEventData eventData) => DoInvoke();

    private void OnHoverEnterInternal()
    {
        _isPointerOver = true;
    }

    private void OnHoverExitInternal()
    {
        _isPointerOver = false;
    }

    void Update()
    {
        bool triggerPulled = CheckTriggers();

        if (_isPointerOver && triggerPulled && !_wasTriggerPulled)
        {
            DoInvoke();
        }
        _wasTriggerPulled = triggerPulled;
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
        if (_inputField != null && _inputField.interactable)
        {
            VRKeyboard keyboard = FindObjectOfType<VRKeyboard>(true); // include inactive
            if (keyboard == null)
            {
                // Instantiate VRKeyboard
                Canvas parentCanvas = GetComponentInParent<Canvas>();
                GameObject kbGo = new GameObject("VRKeyboard_Instance");
                if (parentCanvas != null)
                {
                    kbGo.transform.SetParent(parentCanvas.transform, false);
                }
                keyboard = kbGo.AddComponent<VRKeyboard>();
            }

            keyboard.OpenForInputField(_inputField);
        }
    }
}
