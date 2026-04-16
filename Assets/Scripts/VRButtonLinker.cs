using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// VR buton tıklama köprüsü.
/// IPointerClickHandler: Canvas üzerindeki XR/mouse tıklamalarını yakalar.
/// XRSimpleInteractable: Physics-based XR interaction fallback.
/// </summary>
public class VRButtonLinker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public float colliderScaleMultiplier = 1.0f;
    private Button btn;
    private InputField inputField;
    private XRSimpleInteractable interactable;
    private bool isPointerOver = false;

    void Awake()
    {
        btn = GetComponent<Button>();
        inputField = GetComponent<InputField>();
        interactable = GetComponent<XRSimpleInteractable>();

        // Lazerin (XR Ray Interactor) butonu fiziksel olarak "görebilmesi" için BoxCollider ŞART
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
        // Layout Update'lerinin calismasi icin ek asama
        yield return null; 

        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null && col != null)
        {
            float width = rt.rect.width > 0 ? rt.rect.width : 100f; // Minimal boyut fallback
            float height = rt.rect.height > 0 ? rt.rect.height : 30f;
            col.size = new Vector3(width * colliderScaleMultiplier, height * colliderScaleMultiplier, 1f);
            col.center = new Vector3(rt.rect.center.x, rt.rect.center.y, 0f);
        }

        if (interactable != null)
        {
            interactable.selectEntered.AddListener((args) =>
            {
                Debug.Log($"[VRButtonLinker] XR Select: {gameObject.name}");
                InvokeButton("XRSelect");
            });
            interactable.hoverEntered.AddListener((args) =>
            {
                isPointerOver = true;
                PointerEventData ped = new PointerEventData(EventSystem.current);
                ExecuteEvents.Execute(gameObject, ped, ExecuteEvents.pointerEnterHandler);
            });
            interactable.hoverExited.AddListener((args) =>
            {
                isPointerOver = false;
                PointerEventData ped = new PointerEventData(EventSystem.current);
                ExecuteEvents.Execute(gameObject, ped, ExecuteEvents.pointerExitHandler);
                
                if (btn != null)
                {
                    // Forcibly clear the selection state if it got stuck
                    btn.OnDeselect(new BaseEventData(EventSystem.current));
                }
            });
        }
    }

    private bool wasTriggerPulled = false;

    void Update()
    {
        bool triggerPulled = false;

        // Triggers'ı izleyelim
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad.rightTrigger.ReadValue() > 0.5f || gamepad.leftTrigger.ReadValue() > 0.5f)
            {
                triggerPulled = true;
            }
        }

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

        if (isPointerOver && triggerPulled && !wasTriggerPulled)
        {
            InvokeButton("UpdateManualCheck");
        }

        // Pointer üzerinde olsak da olmasak da tetiğin durumunu kaydet.
        // Aksi takdirde hover başladığı ilk frame tıklama olarak algılanır.
        wasTriggerPulled = triggerPulled;
    }

    // ── IPointerClickHandler ──────────────────────────────────────
    // TrackedDeviceGraphicRaycaster VEYA GraphicRaycaster tarafından
    // tetiklenir (VR lazer ya da mouse fark etmez).
    public void OnPointerClick(PointerEventData eventData)
    {
        InvokeButton("OnPointerClick");
    }

    public void OnPointerEnter(PointerEventData eventData) { isPointerOver = true; }
    public void OnPointerExit(PointerEventData eventData)  { isPointerOver = false; }

    // ─────────────────────────────────────────────────────────────
    private float lastInvokeTime = -1f;

    void InvokeButton(string source)
    {
        Debug.Log($"[VRButtonLinker] InvokeButton called by: {source} -> {gameObject.name}");

        if (Time.time - lastInvokeTime < 0.5f)
        {
            Debug.Log($"[VRButtonLinker] Invoke blocked by debounce -> {gameObject.name}");
            return;
        }

        lastInvokeTime = Time.time;

        if (btn != null && btn.interactable)
        {
            Debug.Log($"[VRButtonLinker] btn.onClick.Invoke -> {gameObject.name}");
            btn.onClick.Invoke();
        }
        else if (inputField != null && inputField.interactable)
        {
            Debug.Log($"[VRButtonLinker] InputField Open VRKeyboard -> {gameObject.name}");
            
            // Eğer fiziksel klavye aktifleşmesin (sadece sanal VR klavye yazı yazsın) istiyorsak
            // EventSystem'den Select YAPMIYORUZ! Böylece gerçek donanım klavyesi buraya veri gönderemiyor.
            VRKeyboard kb = GameObject.FindFirstObjectByType<VRKeyboard>(FindObjectsInactive.Include); 
            if (kb != null) 
            {
                kb.OpenForInputField(inputField);
            }
        }
    }
}
