using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ReactiveLissajous : MonoBehaviour
{
    public AudioSpectrumData data;

    [Header("Shape Settings")]
    public int resolution = 300;
    public float speedX = 3f;
    public float speedY = 2f;
    public float baseRadiusX = 2f;
    public float baseRadiusY = 2f;

    [Header("Reaction")]
    public float audioRadiusMultiplier = 4f;
    public float lineThickness = 0.05f;
    [ColorUsage(false, true)] // HDR color
    public Color baseColor = Color.cyan;
    public Color highColor = Color.magenta;
    
    private LineRenderer line;
    private float time;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = resolution + 1;
        line.useWorldSpace = false;
        
        // Setup LineRenderer basic properties
        line.startWidth = lineThickness;
        line.endWidth = lineThickness;
        line.material = new Material(Shader.Find("Particles/Standard Unlit"));
        line.material.SetColor("_EmissionColor", baseColor);
    }

    void Update()
    {
        if (data == null) return;

        // Progress time faster based on mid-range frequencies
        time += Time.deltaTime * (1f + data.mid * 2f); 
        
        // Expand shape based on overall amplitude
        float currentTargetX = baseRadiusX + (data.amplitude * audioRadiusMultiplier);
        float currentTargetY = baseRadiusY + (data.amplitude * audioRadiusMultiplier);

        var positions = new Vector3[resolution + 1];
        
        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / resolution * Mathf.PI * 2f;
            
            // Lissajous curve formula
            float x = Mathf.Sin(t * speedX + time) * currentTargetX;
            float y = Mathf.Cos(t * speedY) * currentTargetY;
            float z = Mathf.Sin(t * (speedX + speedY)) * (data.bass * 2f); // Z depth driven by bass
            
            positions[i] = new Vector3(x, y, z);
        }
        
        line.SetPositions(positions);
        
        // React line thickness to bass
        float dynamicThickness = Mathf.Lerp(lineThickness, lineThickness * 4f, data.bass);
        line.startWidth = dynamicThickness;
        line.endWidth = dynamicThickness;

        // React color
        Color targetColor = Color.Lerp(baseColor, highColor, data.high);
        line.material.SetColor("_EmissionColor", targetColor * (1f + data.amplitude * 2f));
    }
}
