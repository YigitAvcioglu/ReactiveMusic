using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class VRScrollbarLinker : MonoBehaviour
{
    private Scrollbar scrollbar;
    private XRSimpleInteractable interactable;
    private bool isPointerOver = false;

    void Awake()
    {
        scrollbar = GetComponent<Scrollbar>();
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
            float width = rt.rect.width > 0 ? rt.rect.width : 20f;
            float height = rt.rect.height > 0 ? rt.rect.height : 200f;
            
            // Bump the interaction area size slightly so it's easier to hit with the laser pointer
            if (scrollbar != null && (scrollbar.direction == Scrollbar.Direction.BottomToTop || scrollbar.direction == Scrollbar.Direction.TopToBottom))
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
        // Dynamically update collider size if rect transform changes (e.g. after layout rebuilt)
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
                    if (scrollbar != null && (scrollbar.direction == Scrollbar.Direction.BottomToTop || scrollbar.direction == Scrollbar.Direction.TopToBottom))
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
            else if (currentInteractor != null)
            {
                bool hitFound = false;
                var interactorType = currentInteractor.GetType();
                var method = interactorType.GetMethod("TryGetCurrent3DRaycastHit");
                if (method != null)
                {
                    object[] parameters = new object[] { null };
                    bool success = (bool)method.Invoke(currentInteractor, parameters);
                    if (success && parameters[0] is RaycastHit hit)
                    {
                        UpdateScrollbarFromHit(hit.point);
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
                        interactorTransform = (Transform)transformProp.GetValue(currentInteractor);
                    }
                    else if (currentInteractor is MonoBehaviour mb)
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
                            UpdateScrollbarFromHit(hitPoint);
                        }
                    }
                }
            }
        }

        wasTriggerPulled = triggerPulled;
    }

    void UpdateScrollbarFromHit(Vector3 hitPoint)
    {
        RectTransform rt = GetComponent<RectTransform>();
        BoxCollider col = GetComponent<BoxCollider>();
        if (rt != null && scrollbar != null)
        {
            Vector3 localPos = rt.InverseTransformPoint(hitPoint);
            Rect rect = rt.rect;

            float width = rect.width > 0.1f ? rect.width : (col != null ? col.size.x : 20f);
            float height = rect.height > 0.1f ? rect.height : (col != null ? col.size.y : 200f);

            float xMin = rect.width > 0.1f ? rect.xMin : -width / 2f;
            float yMin = rect.height > 0.1f ? rect.yMin : -height / 2f;

            if (scrollbar.direction == Scrollbar.Direction.LeftToRight || scrollbar.direction == Scrollbar.Direction.RightToLeft)
            {
                float normalizedX = (localPos.x - xMin) / width;
                normalizedX = Mathf.Clamp01(normalizedX);
                if (scrollbar.direction == Scrollbar.Direction.RightToLeft) normalizedX = 1f - normalizedX;
                scrollbar.value = normalizedX;
            }
            else
            {
                float normalizedY = (localPos.y - yMin) / height;
                normalizedY = Mathf.Clamp01(normalizedY);
                if (scrollbar.direction == Scrollbar.Direction.TopToBottom) normalizedY = 1f - normalizedY;
                scrollbar.value = normalizedY;
            }
        }
    }
}
