using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class VRSliderLinker : MonoBehaviour
{
    private Slider slider;
    private XRSimpleInteractable interactable;
    private bool isPointerOver = false;

    void Awake()
    {
        slider = GetComponent<Slider>();
        interactable = GetComponent<XRSimpleInteractable>();

        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = false; 
        }
        
        StartCoroutine(UpdateColliderSizeRoutine(col));
    }

    System.Collections.IEnumerator UpdateColliderSizeRoutine(BoxCollider col)
    {
        yield return new WaitForEndOfFrame();
        yield return null; 

        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null && col != null)
        {
            float width = rt.rect.width > 0 ? rt.rect.width : 200f;
            float height = rt.rect.height > 0 ? rt.rect.height : 20f;
            // Height typically needs a bump on sliders so it's easier to hit
            col.size = new Vector3(width, height * 3f, 1f);
            col.center = new Vector3(rt.rect.center.x, rt.rect.center.y, 0f);
        }

        if (interactable != null)
        {
            interactable.hoverEntered.AddListener((args) => isPointerOver = true);
            interactable.hoverExited.AddListener((args) => isPointerOver = false);
        }
    }

    private bool wasTriggerPulled = false;

    void Update()
    {
        if (isPointerOver && interactable.interactorsHovering.Count > 0)
        {
            var hoveringInteractor = interactable.interactorsHovering[0];
            
            bool triggerPulled = false;
            foreach (var device in UnityEngine.InputSystem.InputSystem.devices)
            {
                if (device.name.Contains("Controller") || device.name.Contains("Hand"))
                {
                    var triggerControl = device.GetChildControl<UnityEngine.InputSystem.Controls.AxisControl>("trigger");
                    if (triggerControl != null && triggerControl.ReadValue() > 0.5f)
                    {
                        triggerPulled = true;
                    }
                }
            }

            if (triggerPulled)
            {
                if (!wasTriggerPulled)
                {
                    PointerEventData ped = new PointerEventData(EventSystem.current);
                    ExecuteEvents.Execute(gameObject, ped, ExecuteEvents.pointerDownHandler);
                }
                wasTriggerPulled = true;

                // Reflection to get TryGetCurrent3DRaycastHit just to avoid XRI namespace version issues
                var interactorType = hoveringInteractor.GetType();
                var method = interactorType.GetMethod("TryGetCurrent3DRaycastHit");
                if (method != null)
                {
                    object[] parameters = new object[] { null };
                    bool success = (bool)method.Invoke(hoveringInteractor, parameters);
                    if (success && parameters[0] is RaycastHit hit)
                    {
                        UpdateSliderFromHit(hit.point);
                        return;
                    }
                }
                
                // Fallback: If we couldn't get RaycastHit, just try to get the transform.position of the interactor
                // and approximate the ray projection.
                var attachTransform = hoveringInteractor.transform;
                Ray ray = new Ray(attachTransform.position, attachTransform.forward);
                BoxCollider col = GetComponent<BoxCollider>();
                if (col.Raycast(ray, out RaycastHit fallbackHit, 100f))
                {
                    UpdateSliderFromHit(fallbackHit.point);
                }
            }
            else
            {
                if (wasTriggerPulled)
                {
                    PointerEventData ped = new PointerEventData(EventSystem.current);
                    ExecuteEvents.Execute(gameObject, ped, ExecuteEvents.pointerUpHandler);
                    wasTriggerPulled = false;
                }
            }
        }
        else
        {
            if (wasTriggerPulled)
            {
                PointerEventData ped = new PointerEventData(EventSystem.current);
                ExecuteEvents.Execute(gameObject, ped, ExecuteEvents.pointerUpHandler);
                wasTriggerPulled = false;
            }
        }
    }

    void UpdateSliderFromHit(Vector3 hitPoint)
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null && slider != null)
        {
            Vector3 localPos = rt.InverseTransformPoint(hitPoint);

            Rect rect = rt.rect;
            float normalizedX = (localPos.x - rect.xMin) / rect.width;
            
            normalizedX = Mathf.Clamp01(normalizedX);

            float newValue = Mathf.Lerp(slider.minValue, slider.maxValue, normalizedX);
            slider.value = newValue;
        }
    }
}
