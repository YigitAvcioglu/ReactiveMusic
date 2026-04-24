using UnityEngine;

[RequireComponent(typeof(Light))]
public class ReactiveLight : MonoBehaviour
{
    public AudioSpectrumData data;
    
    [Header("Color Reaction (Bass=Red, High=Blue)")]
    public bool reactColor = true;
    public Color bassColor = Color.red;
    public Color highColor = Color.blue;
    [Range(0.1f, 20f)] public float colorLerpSpeed = 5f;

    [Header("Intensity Reaction")]
    public bool reactIntensity = true;
    public float minIntensity = 0.5f;
    public float maxIntensity = 3.0f;
    public float intensityMultiplier = 1.0f;

    private Light myLight;
    private Color targetColor;

    void Start()
    {
        myLight = GetComponent<Light>();
        targetColor = myLight.color;
    }

    void Update()
    {
        if (data == null) return;

        if (reactColor)
        {
            // Calculate a color mix based on frequency
            Color desiredColor = Color.Lerp(minIntensity > 0 ? myLight.color : Color.black, highColor, data.high);
            desiredColor = Color.Lerp(desiredColor, bassColor, data.bass); 
            
            targetColor = Color.Lerp(targetColor, desiredColor, Time.deltaTime * colorLerpSpeed);
            myLight.color = targetColor;
        }

        if (reactIntensity)
        {
            float safeMinIntensity = Mathf.Max(minIntensity, 0.8f);
            float targetIntensity = Mathf.Lerp(safeMinIntensity, maxIntensity, data.amplitude * intensityMultiplier);
            // Fast smoothing for intensity to avoid bad strobe in VR
            myLight.intensity = Mathf.Lerp(myLight.intensity, targetIntensity, Time.deltaTime * 15f); 
        }
    }
}
