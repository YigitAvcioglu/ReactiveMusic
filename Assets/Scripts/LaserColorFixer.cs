using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class LaserColorFixer : MonoBehaviour
{
    private XRInteractorLineVisual lineVisual;

    void Start()
    {
        ApplyColors();
    }

    [ContextMenu("Apply Colors")]
    public void ApplyColors()
    {
        if (lineVisual == null) lineVisual = GetComponent<XRInteractorLineVisual>();
        if (lineVisual == null) return;

        // ORIGINAL XR BLUE
        Color xrBlue = new Color(0.125f, 0.588f, 0.953f, 0.6f);

        // Valid -> OPEN BLUE (Button surface)
        Gradient blueGrad = new Gradient();
        blueGrad.SetAlphaKeys(new GradientAlphaKey[] { new GradientAlphaKey(xrBlue.a, 0.0f), new GradientAlphaKey(xrBlue.a, 1.0f) });
        blueGrad.SetColorKeys(new GradientColorKey[] { new GradientColorKey(xrBlue, 0.0f), new GradientColorKey(xrBlue, 1.0f) });

        // Invalid -> RED (Empty space)
        Gradient redGrad = new Gradient();
        redGrad.SetAlphaKeys(new GradientAlphaKey[] { new GradientAlphaKey(xrBlue.a, 0.0f), new GradientAlphaKey(xrBlue.a, 1.0f) });
        redGrad.SetColorKeys(new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.red, 1.0f) });

        lineVisual.validColorGradient = blueGrad;
        lineVisual.invalidColorGradient = redGrad;

        lineVisual.setLineColorGradient = true;
    }
}
