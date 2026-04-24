using UnityEngine;

/// <summary>
/// Tüm çevrenin (duvarlar, zemin vb.) ana ve parlama (emission) renklerini müziğe göre otomatik değiştirir.
/// Grid ve özel shader renklendirmelerini de destekler.
/// </summary>
public class EnvironmentColorReactive : MonoBehaviour
{
    [Header("Ses Bağlantısı")]
    public AudioSpectrumData audioData;

    [Header("Hedef Objeler (Boş bırakılırsa tüm alt objeler aranır)")]
    public Renderer[] environmentRenderers;

    [Header("Renk Cümbüşü (Gradient)")]
    public Gradient colorGradient;
    public float colorChangeSpeed = 0.15f;

    private MaterialPropertyBlock _mpb;
    private float _hueOffset;

    void Start()
    {
        _mpb = new MaterialPropertyBlock();

        if (environmentRenderers == null || environmentRenderers.Length == 0)
        {
            environmentRenderers = GetComponentsInChildren<Renderer>();
        }
        
        if (audioData == null)
        {
            audioData = GameObject.FindFirstObjectByType<AudioSpectrumData>();
        }

        bool isWhiteGradient = colorGradient != null && colorGradient.colorKeys != null && colorGradient.colorKeys.Length == 2 && colorGradient.colorKeys[0].color == Color.white && colorGradient.colorKeys[1].color == Color.white;
        if (colorGradient == null || colorGradient.alphaKeys == null || colorGradient.alphaKeys.Length == 0 || isWhiteGradient)
        {
            colorGradient = new Gradient();
            colorGradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(1f, 0f, 0.5f), 0.0f),   // Pembe
                    new GradientColorKey(new Color(0.2f, 0.2f, 1f), 0.33f), // Mavi
                    new GradientColorKey(new Color(0f, 1f, 0f), 0.66f),     // Yeşil
                    new GradientColorKey(new Color(0f, 1f, 1f), 1.0f),      // Cyan
                },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            );
        }
    }

    void Update()
    {
        if (audioData == null) return;

        float bass = audioData.bass;
        float amplitude = audioData.amplitude;

        _hueOffset += Time.deltaTime * colorChangeSpeed;
        float colorT = Mathf.Repeat(_hueOffset + (bass * 0.15f), 1f);
        
        Color targetColor = colorGradient.Evaluate(colorT);
        
        // Müzik şiddetine göre parlaklık (Hem çizgiler hem duvarlar için)
        // Işıkların tamamen sönmesini engellemek için alt limiti (minimum emission) artırdık (0.5f -> 1.5f)
        float emissionStrength = Mathf.Lerp(1.5f, 4.0f, amplitude + (bass * 0.5f));
        Color finalGlowColor = targetColor * emissionStrength;

        foreach (var r in environmentRenderers)
        {
            if (r == null) continue;
            
            r.GetPropertyBlock(_mpb);
            
            // 1. Grid/Çizgi Shader'ları için muhtemel özellik isimleri
            _mpb.SetColor("_GridColor", finalGlowColor);
            _mpb.SetColor("_LineColor", finalGlowColor);
            _mpb.SetColor("_TintColor", finalGlowColor);
            
            // 2. Standart ve URP Materyalleri için
            _mpb.SetColor("_BaseColor", targetColor * 0.5f);
            _mpb.SetColor("_Color", targetColor * 0.5f);
            
            // 3. Parlama değerleri
            _mpb.SetColor("_EmissionColor", finalGlowColor);
            
            r.SetPropertyBlock(_mpb);
        }
    }
}
