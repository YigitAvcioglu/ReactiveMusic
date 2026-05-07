using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class LaserToggle : MonoBehaviour
{
    public InputActionReference toggleAction;
    private XRInteractorLineVisual lineVisual;
    private LineRenderer lineRenderer;

    void Start()
    {
        lineVisual = GetComponent<XRInteractorLineVisual>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (lineVisual == null || lineRenderer == null) return;
        
        bool isPressed = false;
        var rayInteractor = GetComponent<XRRayInteractor>();
        if (rayInteractor != null)
        {
            if (rayInteractor.selectInput.ReadIsPerformed()) isPressed = true;
            if (rayInteractor.activateInput.ReadIsPerformed()) isPressed = true;
            if (rayInteractor.uiPressInput.ReadIsPerformed()) isPressed = true;
        }

        lineVisual.enabled = isPressed;
        lineRenderer.enabled = isPressed;
    }
}
