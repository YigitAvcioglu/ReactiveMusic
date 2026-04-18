using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Button))]
public class MenuBtnFix : MonoBehaviour
{
    public string targetScene;
    private Button btn;
    private Image img;
    private Color originalColor;
    private bool isHovered = false;
    private bool wasPressed = false;

    void Start()
    {
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        originalColor = img.color;

        var interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null) interactable = gameObject.AddComponent<XRSimpleInteractable>();
        interactable.hoverEntered.AddListener(_ => {
            isHovered = true;
            img.color = originalColor * 0.5f;
        });
        interactable.hoverExited.AddListener(_ => {
            isHovered = false;
            img.color = originalColor;
        });

        btn.onClick.AddListener(() => SceneManager.LoadScene(targetScene));
    }

    void Update()
    {
        bool pressed = false;
        foreach (var d in InputSystem.devices) {
            if (d.name.Contains("Controller")) {
                var t = d.GetChildControl<UnityEngine.InputSystem.Controls.AxisControl>("trigger");
                if (t != null && t.ReadValue() > 0.5f) pressed = true;
            }
        }
        if (isHovered && pressed && !wasPressed) btn.onClick.Invoke();
        wasPressed = pressed;
    }
}
