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
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }

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
            float width = rt.rect.width > 0.1f ? rt.rect.width : 200f;
            float height = rt.rect.height > 0.1f ? rt.rect.height : 20f;
            
            if (slider != null && (slider.direction == Slider.Direction.BottomToTop || slider.direction == Slider.Direction.TopToBottom))
            {
                col.size = new Vector3(width * 3f, height, 1f);
            }
            else
            {
                col.size = new Vector3(width, height * 3f, 1f);
            }
            col.center = new Vector3(rt.rect.center.x, rt.rect.center.y, 0f);
        }

        if (interactable != null)
        {
            interactable.hoverEntered.AddListener((args) => isPointerOver = true);
            interactable.hoverExited.AddListener((args) => isPointerOver = false);
        }
    }

    private bool wasTriggerPulled = false;
    private bool isDragging = false;
    private object currentInteractor = null;

    private float lastWidth = -1f;
    private float lastHeight = -1f;

    void Update()
    {
        // Dynamically update collider size if rect transform changes
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null && rt.rect.height > 0.1f && rt.rect.width > 0.1f)
        {
            if (Mathf.Abs(rt.rect.width - lastWidth) > 0.1f || Mathf.Abs(rt.rect.height - lastHeight) > 0.1f)
            {
                lastWidth = rt.rect.width;
                lastHeight = rt.rect.height;
                BoxCollider col = GetComponent<BoxCollider>();
                if (col != null)
                {
                    if (slider != null && (slider.direction == Slider.Direction.BottomToTop || slider.direction == Slider.Direction.TopToBottom))
                        col.size = new Vector3(lastWidth * 3f, lastHeight, 1f);
                    else
                        col.size = new Vector3(lastWidth, lastHeight * 3f, 1f);
                    
                    col.center = new Vector3(rt.rect.center.x, rt.rect.center.y, 0f);
                }
            }
        }

        bool triggerPulled = false;
        foreach (var device in UnityEngine.InputSystem.InputSystem.devices)
        {
            if (device.name.Contains("Controller") || device.name.Contains("Hand"))
            {
                var triggerControl = device.GetChildControl<UnityEngine.InputSystem.Controls.AxisControl>("trigger");
                if (triggerControl != null && triggerControl.ReadValue() > 0.5f)
                {
                    triggerPulled = true;
                    break;
                }
            }
        }

        if (!isDragging)
        {
            if (isPointerOver && triggerPulled && !wasTriggerPulled)
            {
                if (interactable != null && interactable.interactorsHovering.Count > 0)
                {
                    isDragging = true;
                    currentInteractor = interactable.interactorsHovering[0];
                    
                    PointerEventData ped = new PointerEventData(EventSystem.current);
                    ExecuteEvents.Execute(gameObject, ped, ExecuteEvents.pointerDownHandler);
                    
                    TryProcessHit(currentInteractor);
                }
            }
        }

        if (isDragging)
        {
            if (!triggerPulled)
            {
                isDragging = false;
                currentInteractor = null;
                
                PointerEventData ped = new PointerEventData(EventSystem.current);
                ExecuteEvents.Execute(gameObject, ped, ExecuteEvents.pointerUpHandler);
            }
            // Dragging continuous update has been removed as per user request.
        }

        wasTriggerPulled = triggerPulled;
    }

    private void TryProcessHit(object interactor)
    {
        bool hitFound = false;
        var interactorType = interactor.GetType();
        var method = interactorType.GetMethod("TryGetCurrent3DRaycastHit");
        if (method != null)
        {
            object[] parameters = new object[] { null };
            bool success = (bool)method.Invoke(interactor, parameters);
            if (success && parameters[0] is RaycastHit hit)
            {
                UpdateSliderFromHit(hit.point);
                hitFound = true;
            }
        }

        if (!hitFound)
        {
            // Fallback to infinite plane intersection
            Transform interactorTransform = null;
            var transformProp = interactorType.GetProperty("transform");
            if (transformProp != null)
            {
                interactorTransform = (Transform)transformProp.GetValue(interactor);
            }
            else if (interactor is MonoBehaviour mb)
            {
                interactorTransform = mb.transform;
            }

            if (interactorTransform != null)
            {
                Ray ray = new Ray(interactorTransform.position, interactorTransform.forward);
                Plane plane = new Plane(transform.forward, transform.position);
                if (plane.Raycast(ray, out float enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    UpdateSliderFromHit(hitPoint);
                }
            }
        }
    }

    void UpdateSliderFromHit(Vector3 hitPoint)
    {
        RectTransform rt = GetComponent<RectTransform>();
        BoxCollider col = GetComponent<BoxCollider>();
        if (rt != null && slider != null)
        {
            Vector3 localPos = rt.InverseTransformPoint(hitPoint);
            Rect rect = rt.rect;

            float width = rect.width > 0.1f ? rect.width : (col != null ? col.size.x : 200f);
            float height = rect.height > 0.1f ? rect.height : (col != null ? col.size.y : 20f);

            float xMin = rect.width > 0.1f ? rect.xMin : -width / 2f;
            float yMin = rect.height > 0.1f ? rect.yMin : -height / 2f;

            if (slider.direction == Slider.Direction.LeftToRight || slider.direction == Slider.Direction.RightToLeft)
            {
                float normalizedX = (localPos.x - xMin) / width;
                normalizedX = Mathf.Clamp01(normalizedX);
                if (slider.direction == Slider.Direction.RightToLeft) normalizedX = 1f - normalizedX;
                
                slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, normalizedX);
            }
            else
            {
                float normalizedY = (localPos.y - yMin) / height;
                normalizedY = Mathf.Clamp01(normalizedY);
                if (slider.direction == Slider.Direction.TopToBottom) normalizedY = 1f - normalizedY;
                
                slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, normalizedY);
            }
        }
    }
}
