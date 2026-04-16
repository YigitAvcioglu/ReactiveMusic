using UnityEngine;

/// <summary>
/// Disko topu: Döner, etrafına renkli spotlar yayar ve sese göre renk/hız değiştirir.
/// </summary>
public class DiscoBallController : MonoBehaviour
{
    [Header("Bağlantılar")]
    public AudioSpectrumData audioData;

    [Header("Döndürme")]
    public float baseRotationSpeed = 30f;   // Saniyede derece
    public float maxRotationSpeed = 180f;

    [Header("Işık Renkleri")]
    public Gradient colorGradient;          // Bas=0 → Tiz=1 arasında renk geçişi
    public float lightIntensityBase = 2f;
    public float lightIntensityMax = 8f;

    [Header("Disko Işınları (spot ışıkları)")]
    public Light[] spotLights;             // Inspector'dan atanacak rotasyonlu spotlar

    // Private
    private Renderer _ballRenderer;
    private MaterialPropertyBlock _mpb;
    private float _currentSpeed;
    private float _hueOffset;

    void Awake()
    {
        // Pivotta değil, çocuk objede (kürede) renderer var
        _ballRenderer = GetComponentInChildren<Renderer>();
        spotLights = GetComponentsInChildren<Light>();
        _mpb = new MaterialPropertyBlock();
        _currentSpeed = baseRotationSpeed;

        // Unity her public Gradient'i "tamamen beyaz" olarak başlatır. O yüzden override ediyoruz.
        colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0f, 0.5f), 0.0f),   // Bas: Neon Pembe
                new GradientColorKey(new Color(0.2f, 0.2f, 1f), 0.33f), // Alt Mid: Mavi
                new GradientColorKey(new Color(0f, 1f, 0f), 0.66f),     // Üst Mid: Yeşil
                new GradientColorKey(new Color(0f, 1f, 1f), 1.0f),      // Tiz: Cyan
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );
    }

    void Update()
    {
        if (audioData == null) return;

        float bass      = audioData.bass;
        float mid       = audioData.mid;
        float high      = audioData.high;
        float amplitude = audioData.amplitude;

        // ── 1. DÖNDÜRME ──────────────────────────────────────────────
        // Amplitüde göre hız (bas vuruşunda daha hızlı)
        float targetSpeed = Mathf.Lerp(baseRotationSpeed, maxRotationSpeed, amplitude);
        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * 4f);
        transform.Rotate(Vector3.up, _currentSpeed * Time.deltaTime, Space.World);

        // ── 2. IŞIK RENGİ ────────────────────────────────────────────
        // Renkler sürekli olarak bir döngüde aksın (zamanla değişsin).
        // Müzik ritmine (bass) göre de renkte ani "sıçramalar" olsun.
        _hueOffset += Time.deltaTime * 0.15f; 
        float colorT = Mathf.Repeat(_hueOffset + (bass * 0.6f), 1f);
        
        Color lightColor = colorGradient.Evaluate(colorT);

        // Işık yoğunluğu amplitüde göre
        float intensity = Mathf.Lerp(lightIntensityBase, lightIntensityMax, amplitude);

        foreach (var light in spotLights)
        {
            if (light == null) continue;
            light.color     = lightColor;
            light.intensity = intensity;
        }

        // ── 3. TOP EMISSIONI ─────────────────────────────────────────
        // Topun kendi yüzeyini de renge göre parlatır
        if (_ballRenderer != null)
        {
            _ballRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor("_EmissionColor", lightColor * intensity * 0.3f);
            _ballRenderer.SetPropertyBlock(_mpb);
        }
    }

    // Editor'de görsel yardım çizgileri
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.15f);
    }
}
