using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRButtonLinker : MonoBehaviour
{
    private Button btn;
    private XRSimpleInteractable interactable;
    
    private InputAction rightTrigger;
    private InputAction leftTrigger;
    private bool isHovered = false;

    void Awake()
    {
        btn = GetComponent<Button>();
        interactable = GetComponent<XRSimpleInteractable>();

        // VR Kontrolcülerinin "Tetik" tuşlarını doğrudan dinle
        rightTrigger = new InputAction(type: InputActionType.Button, binding: "<XRController>{RightHand}/{PrimaryTrigger}");
        rightTrigger.AddBinding("<XRController>{RightHand}/triggerPressed");
        
        leftTrigger = new InputAction(type: InputActionType.Button, binding: "<XRController>{LeftHand}/{PrimaryTrigger}");
        leftTrigger.AddBinding("<XRController>{LeftHand}/triggerPressed");
        
        rightTrigger.Enable();
        leftTrigger.Enable();

        if (interactable != null)
        {
            // Eski usül select için
            interactable.selectEntered.AddListener((args) => {
                Debug.Log($"🎯 (Select) Lazer ile Tiklandi: {gameObject.name}");
                if (btn != null) btn.onClick.Invoke();
            });

            // Hover (Lazer butonun üzerine geldiğinde)
            interactable.hoverEntered.AddListener((args) => {
                isHovered = true;
            });

            // Hover (Lazer butonun üzerinden ayrıldığında)
            interactable.hoverExited.AddListener((args) => {
                isHovered = false;
            });
        }
    }

    void Update()
    {
        // Eğer lazer butonun üzerindeyse ve kullanıcı tetiği çekerse (veya Simulator'de Sol Tık yaparsa)
        if (isHovered && btn != null)
        {
            bool mouseClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool rTrigger = rightTrigger.triggered;
            bool lTrigger = leftTrigger.triggered;
            bool spaceKey = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
            bool enterKey = Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame;

            if (mouseClick || rTrigger || lTrigger || spaceKey || enterKey)
            {
                Debug.Log($"💥 (Override) Zorunlu Tetikleme Başarılı! Vurulan Buton: {gameObject.name}");
                btn.onClick.Invoke();
            }
        }
    }

    void OnDestroy()
    {
        rightTrigger?.Dispose();
        leftTrigger?.Dispose();
    }
}
