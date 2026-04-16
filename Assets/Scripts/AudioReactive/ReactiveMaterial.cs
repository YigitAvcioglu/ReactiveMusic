using UnityEngine;

public class ReactiveMaterial : MonoBehaviour
{
    public AudioSpectrumData data;

    public int materialIndex = 0;
    
    [Header("Emission Reaction")]
    [ColorUsage(false, true)] // Allow HDR colors
    public Color baseEmissionColor = Color.white;
    public float minEmission = 0f;
    public float maxEmission = 5f;
    public float smoothness = 10f;

    private Material mat;
    private float currentEmission;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null && rend.materials.Length > materialIndex)
        {
            mat = rend.materials[materialIndex];
            mat.EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        if (data == null || mat == null) return;

        float targetEmission = Mathf.Lerp(minEmission, maxEmission, data.amplitude);
        currentEmission = Mathf.Lerp(currentEmission, targetEmission, Time.deltaTime * smoothness);

        mat.SetColor("_EmissionColor", baseEmissionColor * currentEmission);
    }
}
