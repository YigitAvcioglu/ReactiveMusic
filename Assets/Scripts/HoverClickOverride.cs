using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HoverClickOverride : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button myButton;
    private bool isHovered = false;

    void Awake()
    {
        myButton = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    void Update()
    {
        if (isHovered && myButton != null)
        {
            bool click = false;
            
            // XR Device Simulator bazen mouse sol tıkı yutabiliyor. 
            // O yüzden Space ve Enter ile garantiye alıyoruz.
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            {
                click = true;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                click = true;
            }

            if (click)
            {
                Debug.Log($"✅ HOVER OVERRIDE: {gameObject.name} başarıyla tıklandı!");
                myButton.onClick.Invoke();
            }
        }
    }
}
