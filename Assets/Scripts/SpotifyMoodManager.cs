using UnityEngine;
using UnityEngine.Networking;
using SpotifyAPI.Web;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class SpotifyMoodManager : MonoBehaviour
{
    [Header("Controllers")]
    public SpotifyVRController spotifyController;
    public EnvironmentColorReactive environmentColor;
    public AudioAnalyzerHub audioHub;
    public ReactiveLissajous lissajousVisual;

    [Header("Analiz Ayarları (Fallback)")]
    [Tooltip("Şarkı değişince ses analizi için bekleme süresi")]
    public float analysisDelay = 1.5f;
    [Tooltip("Ses analizi süresi (saniye)")]
    public float analysisDuration = 7f;

    [Header("Dance Clips by Mood")]
    public AnimationClip[] energeticClips;
    public AnimationClip[] aggressiveClips;
    public AnimationClip[] calmClips;
    public AnimationClip[] melancholicClips;

    // artist_name (lowercase) -> "happy" | "aggressive" | "calm" | "melancholic"
    private Dictionary<string, string> _artistMoodMap = new Dictionary<string, string>();
    private bool _datasetLoaded = false;

    private Coroutine _analysisCoroutine;

    private void Awake()
    {
        if (spotifyController == null)
            spotifyController = GameObject.FindFirstObjectByType<SpotifyVRController>();
        if (environmentColor == null)
            environmentColor = GameObject.FindFirstObjectByType<EnvironmentColorReactive>();
        if (audioHub == null)
            audioHub = GameObject.FindFirstObjectByType<AudioAnalyzerHub>();
        if (lissajousVisual == null)
            lissajousVisual = GameObject.FindFirstObjectByType<ReactiveLissajous>();

        Debug.Log("[MoodManager] Awake - Controller: " + (spotifyController != null) +
                  ", AudioHub: " + (audioHub != null));

        if (spotifyController != null)
            spotifyController.OnTrackIdChanged += HandleTrackChanged;

        StartCoroutine(LoadDataset());
    }

    private void OnDestroy()
    {
        if (spotifyController != null)
            spotifyController.OnTrackIdChanged -= HandleTrackChanged;
    }

    // ── Dataset yükleme (JSON) ────────────────────────────────────────────────

    private IEnumerator LoadDataset()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "artist_mood.json");

#if UNITY_ANDROID && !UNITY_EDITOR
        using (var req = UnityWebRequest.Get(path))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
                ParseJSON(req.downloadHandler.text);
            else
                Debug.LogWarning("[MoodManager] artist_mood.json yuklenemedi: " + req.error);
        }
#else
        Debug.Log("[MoodManager] Dosya yolu: " + path);
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            Debug.Log("[MoodManager] Dosya okundu, karakter sayisi: " + json.Length);
            ParseJSON(json);
        }
        else
        {
            Debug.LogError("[MoodManager] DOSYA BULUNAMADI: " + path);
        }
        yield return null;
#endif

        _datasetLoaded = true;
        Debug.Log("[MoodManager] Yukleme tamamlandi. Map'teki kayit: " + _artistMoodMap.Count);

        // Ilk 5 kaydi logla (format dogrulama)
        int shown = 0;
        foreach (var kv in _artistMoodMap)
        {
            Debug.Log("[MoodManager] Ornek: \"" + kv.Key + "\" => " + kv.Value);
            if (++shown >= 5) break;
        }
    }

    private void ParseJSON(string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        var matches = Regex.Matches(json, "\"([^\"]+)\":\\s*\"([^\"]+)\"");
        
        int added = 0;
        string pendingArtist = null;

        foreach (Match match in matches)
        {
            string rawKey = match.Groups[1].Value;
            string rawValue = match.Groups[2].Value;
            string lowerKey = rawKey.ToLower().Trim();

            if (lowerKey == "entries") continue;

            if (lowerKey == "artists" || lowerKey == "artist")
            {
                pendingArtist = rawValue;
            }
            else if (lowerKey == "genre")
            {
                if (!string.IsNullOrEmpty(pendingArtist))
                {
                    string sanitizedKey = Sanitize(pendingArtist);
                    if (!_artistMoodMap.ContainsKey(sanitizedKey))
                    {
                        _artistMoodMap[sanitizedKey] = GenreToMood(rawValue);
                        added++;
                    }
                }
                pendingArtist = null;
            }
            else
            {
                // Legacy format
                string sanitizedKey = Sanitize(rawKey);
                if (!_artistMoodMap.ContainsKey(sanitizedKey))
                {
                    _artistMoodMap[sanitizedKey] = GenreToMood(rawValue);
                    added++;
                }
            }
        }

        if (added > 0)
            Debug.Log("[MoodManager] JSON yuklendi. Sanatci: " + added);
        else
            Debug.LogError("[MoodManager] JSON icinde gecerli veri bulunamadi!");
    }

    private string Sanitize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.ToLower().Trim();
        // Turkce ve bozuk karakterleri temizle
        s = s.Replace("ş", "s").Replace("ı", "i").Replace("ç", "c").Replace("ü", "u").Replace("ö", "o").Replace("ğ", "g");
        s = s.Replace("åÿ", "s").Replace("ä±", "i").Replace("ã§", "c").Replace("ã¼", "u").Replace("ã¶", "o").Replace("ã°", "g");
        return s;
    }

    // ── Spotify track_genre → Mood mapping ───────────────────────────────────

    private string GenreToMood(string genre)
    {
        string g = genre.ToLower().Trim();
        if (g.Contains("happy")) return "happy";
        if (g.Contains("aggressive")) return "aggressive";
        if (g.Contains("calm")) return "calm";
        if (g.Contains("melancholic")) return "melancholic";
        return "happy"; // Default
    }

    // ── Track değişimi ────────────────────────────────────────────────────────

    private void HandleTrackChanged(string trackId)
    {
        if (string.IsNullOrEmpty(trackId)) return;
        Debug.Log("[MoodManager] Track degisti: " + trackId);
        StartCoroutine(DetermineAndApplyMood(trackId));
    }

    private IEnumerator DetermineAndApplyMood(string trackId)
    {
        yield return new WaitUntil(() => _datasetLoaded);

        var client = spotifyController.GetClient();
        if (client == null)
        {
            Debug.LogWarning("[MoodManager] Spotify client null.");
            yield break;
        }

        var trackTask = client.Tracks.Get(trackId);
        var featuresTask = client.Tracks.GetAudioFeatures(trackId);

        yield return new WaitUntil(() => trackTask.IsCompleted && featuresTask.IsCompleted);

        if (!trackTask.IsCompletedSuccessfully || trackTask.Result == null)
        {
            Debug.LogWarning("[MoodManager] Track bilgisi alinamadi.");
            yield break;
        }

        var fullTrack = trackTask.Result;
        var audioFeatures = featuresTask.IsCompletedSuccessfully ? featuresTask.Result : null;

        string artistName = fullTrack.Artists != null && fullTrack.Artists.Count > 0
            ? fullTrack.Artists[0].Name
            : "";

        Debug.Log("[MoodManager] Sanatci: " + artistName);

        string lookupKey = Sanitize(artistName);
        if (_artistMoodMap.TryGetValue(lookupKey, out string datasetMood))
        {
            Debug.Log("[MoodManager] Dataset eslesmesi: " + artistName + " (" + lookupKey + ") => " + datasetMood);
            ApplyMoodByName(datasetMood);
            yield break;
        }

        // Dataset'te yoksa Spotify'in kendi Audio Features verilerini kullan
        if (audioFeatures != null && (audioFeatures.Energy > 0 || audioFeatures.Valence > 0))
        {
            float e = audioFeatures.Energy;
            float v = audioFeatures.Valence;
            Debug.Log($"[MoodManager] Dataset'te yok, Spotify Analizi kullaniliyor: Energy={e}, Valence={v}");
            
            UpdateMood(e, v);
            
            string spotifyMood = "happy";
            if (e > 0.6f) spotifyMood = (v >= 0.5f) ? "happy" : "aggressive";
            else spotifyMood = (v >= 0.5f) ? "calm" : "melancholic";
            
            UpdateNPCDances(spotifyMood);
            yield break;
        }

        Debug.Log("[MoodManager] Dataset ve Spotify verisi bulunamadi. Son care LASP analizine geciliyor...");

        if (_analysisCoroutine != null)
            StopCoroutine(_analysisCoroutine);
        _analysisCoroutine = StartCoroutine(AnalyzeAudioMood());
    }

    // ── Dataset mood ismine gore parametre uygula ─────────────────────────────

    private void ApplyMoodByName(string moodName)
    {
        switch (moodName.ToLower())
        {
            case "happy":
                UpdateMood(0.8f, 0.8f);
                break;
            case "aggressive":
                UpdateMood(0.8f, 0.2f);
                break;
            case "calm":
                UpdateMood(0.25f, 0.75f);
                break;
            case "melancholic":
                UpdateMood(0.2f, 0.2f);
                break;
            default:
                UpdateMood(0.5f, 0.5f);
                break;
        }
        UpdateNPCDances(moodName);
    }
    // ── LASP ses analizi (fallback) ───────────────────────────────────────────

    private IEnumerator AnalyzeAudioMood()
    {
        yield return new WaitForSeconds(analysisDelay);

        if (audioHub == null) yield break;

        float totalAmp = 0f, totalBass = 0f, totalMid = 0f, totalHigh = 0f;
        int samples = 0;
        float elapsed = 0f;

        while (elapsed < analysisDuration)
        {
            elapsed += Time.deltaTime;
            totalAmp  += GetLevel(audioHub.amplitudeTracker);
            totalBass += GetLevel(audioHub.bassTracker);
            totalMid  += GetLevel(audioHub.midTracker);
            totalHigh += GetLevel(audioHub.highTracker);
            samples++;
            yield return null;
        }

        if (samples == 0) yield break;

        float avgAmp  = totalAmp  / samples;
        float avgBass = totalBass / samples;
        float avgMid  = totalMid  / samples;
        float avgHigh = totalHigh / samples;

        float energy   = Mathf.Clamp01(avgAmp * 3.5f);
        float spectral = 0.5f;
        float totalSpectral = avgBass + avgMid + avgHigh;
        if (totalSpectral > 0.001f)
            spectral = Mathf.Clamp01((avgHigh * 1.5f + avgMid * 0.8f) / (totalSpectral + 0.001f));

        float valence = spectral;

        Debug.Log("[MoodManager] Ses analizi => Energy: " + energy.ToString("F2") +
                  ", Valence: " + valence.ToString("F2"));

        UpdateMood(energy, valence);

        string fallbackMood = "happy"; // 0 gelirse happy de, melancholic deme.
        if (energy < 0.05f) 
        {
            fallbackMood = "happy";
        }
        else if (energy > 0.5f)
        {
            fallbackMood = (valence >= 0.5f) ? "happy" : "aggressive";
        }
        else
        {
            fallbackMood = (valence >= 0.5f) ? "calm" : "melancholic";
        }

        Debug.Log("[MoodManager] LASP Fallback Mood: " + fallbackMood);
        UpdateNPCDances(fallbackMood);
        _analysisCoroutine = null;
    }

    private void UpdateNPCDances(string mood)
    {
        AnimationClip[] selectedClips = null;
        switch (mood.ToLower())
        {
            case "happy":       selectedClips = energeticClips;   break;
            case "aggressive":  selectedClips = aggressiveClips;  break;
            case "calm":        selectedClips = calmClips;        break;
            case "melancholic": selectedClips = melancholicClips; break;
        }

        if (selectedClips == null || selectedClips.Length == 0) return;

        GameObject root = GameObject.Find("DanceNPCs");
        if (root == null) return;

        Animator[] animators = root.GetComponentsInChildren<Animator>();
        for (int i = 0; i < animators.Length; i++)
        {
            var anim = animators[i];
            var clip = selectedClips[i % selectedClips.Length];

            if (anim.runtimeAnimatorController != null)
            {
                AnimatorOverrideController aoc = anim.runtimeAnimatorController as AnimatorOverrideController;
                if (aoc == null)
                    aoc = new AnimatorOverrideController(anim.runtimeAnimatorController);

                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                aoc.GetOverrides(overrides);

                for (int j = 0; j < overrides.Count; j++)
                    overrides[j] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[j].Key, clip);

                aoc.ApplyOverrides(overrides);
                anim.runtimeAnimatorController = aoc;
            }
        }
    }

    private float GetLevel(Lasp.AudioLevelTracker tracker)
    {
        if (tracker == null) return 0f;
        return Mathf.Clamp01(tracker.normalizedLevel);
    }

    // ── Sahne güncelleme ──────────────────────────────────────────────────────

    private void UpdateMood(float energy, float valence)
    {
        float speed = 0.15f, gain = 1.0f, fallSpeed = 2.0f, rxSmoothSpeed = 10f;
        float baseHsvVal = 1f, baseHsvSat = 1f;
        float colorTemperature = 0f;

        if (energy >= 0.5f && valence >= 0.5f)
        {
            baseHsvVal = 1.0f; baseHsvSat = 0.9f;
            speed = 0.65f; colorTemperature = 0.8f;
            gain = Mathf.Lerp(1.2f, 2.0f, energy);
            fallSpeed = Mathf.Lerp(3.0f, 5.0f, energy);
            rxSmoothSpeed = Mathf.Lerp(9f, 12f, energy);
            Debug.Log("[MoodManager] MOD: Coskulu/Neseli rxSmooth:" + rxSmoothSpeed.ToString("F1"));
        }
        else if (energy >= 0.5f && valence < 0.5f)
        {
            baseHsvVal = 0.6f; baseHsvSat = 1.0f;
            speed = 0.35f; colorTemperature = 1.0f;
            gain = Mathf.Lerp(0.8f, 1.3f, energy);
            fallSpeed = Mathf.Lerp(2.0f, 3.5f, energy);
            rxSmoothSpeed = Mathf.Lerp(6f, 9f, energy);
            Debug.Log("[MoodManager] MOD: Agresif/Karanlik rxSmooth:" + rxSmoothSpeed.ToString("F1"));
        }
        else if (energy < 0.5f && valence >= 0.5f)
        {
            baseHsvVal = 0.8f; baseHsvSat = 0.6f;
            speed = 0.15f; colorTemperature = -0.7f;
            gain = Mathf.Lerp(0.7f, 1.0f, energy);
            fallSpeed = Mathf.Lerp(1.0f, 2.0f, energy);
            rxSmoothSpeed = Mathf.Lerp(4f, 6f, energy);
            Debug.Log("[MoodManager] MOD: Huzurlu/Sakin rxSmooth:" + rxSmoothSpeed.ToString("F1"));
        }
        else
        {
            baseHsvVal = 0.4f; baseHsvSat = 0.8f;
            speed = 0.05f; colorTemperature = -1.0f;
            gain = Mathf.Lerp(0.4f, 0.7f, energy);
            fallSpeed = Mathf.Lerp(0.5f, 1.2f, energy);
            rxSmoothSpeed = Mathf.Lerp(2f, 4f, energy);
            Debug.Log("[MoodManager] MOD: Melankolik/Huzunlu rxSmooth:" + rxSmoothSpeed.ToString("F1"));
        }

        if (environmentColor != null)
        {
            environmentColor.colorChangeSpeed = speed;
            environmentColor.colorGradient = GenerateProceduralGradient(colorTemperature, baseHsvVal, baseHsvSat);
        }

        if (lissajousVisual != null)
            lissajousVisual.smoothSpeed = rxSmoothSpeed;

        SetLaspParams(gain, fallSpeed);
    }

    private Gradient GenerateProceduralGradient(float temperature, float v, float s)
    {
        Gradient g = new Gradient();
        GradientColorKey[] colors = new GradientColorKey[4];
        GradientAlphaKey[] alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1f, 0f);
        alphas[1] = new GradientAlphaKey(1f, 1f);

        if (Mathf.Approximately(temperature, 0.8f))
        {
            // Happy mod: Her renkten (yeşil, mavi, kırmızı vb.) olsun
            float startHue = Random.value;
            for (int i = 0; i < 4; i++)
            {
                // Renkleri tüm spektruma yayarak gökkuşağı tarzı bir geçiş oluştur
                float h = Mathf.Repeat(startHue + (i * 0.25f), 1f);
                colors[i] = new GradientColorKey(Color.HSVToRGB(h, s, v), i / 3f);
            }
        }
        else if (Mathf.Approximately(temperature, 1.0f))
        {
            // Aggressive mod: %50 Kırmızı, %30 Turuncu, %20 Mor
            float[] allowedHues = { 0.0f, 0.0f, 0.01f, 0.02f, 0.03f, 0.04f, 0.05f, 0.06f, 0.76f, 0.80f };
            for (int i = 0; i < 4; i++)
            {
                float h = allowedHues[Random.Range(0, allowedHues.Length)];
                colors[i] = new GradientColorKey(Color.HSVToRGB(h, s, v), i / 3f);
            }
        }
        else if (Mathf.Approximately(temperature, -0.7f))
        {
            // Calm mod: Sadece Açık Mavi / Turkuaz (0.50 - 0.56) tonları. Mor asla yok.
            float[] allowedHues = { 0.50f, 0.52f, 0.54f, 0.56f };
            for (int i = 0; i < 4; i++)
            {
                float h = allowedHues[Random.Range(0, allowedHues.Length)];
                // Beyaz-Mavi karışımı hissiyatını artırmak için saturation'ı rastgele biraz daha kısıyoruz
                float currentS = s * Random.Range(0.6f, 1.0f); 
                colors[i] = new GradientColorKey(Color.HSVToRGB(h, currentS, v), i / 3f);
            }
        }
        else
        {
            float baseHue;
            if (temperature <= -0.5f)
            {
                // Melancholic (-1.0)
                float[] hues = { 0.50f, 0.55f, 0.60f, 0.65f, 0.70f, 0.75f };
                baseHue = hues[Random.Range(0, hues.Length)];
            }
            else
            {
                // Fallback / Unknown
                float[] hues = { 0.25f, 0.30f, 0.35f, 0.40f, 0.45f };
                baseHue = hues[Random.Range(0, hues.Length)];
            }

            for (int i = 0; i < 4; i++)
            {
                float h = Mathf.Repeat(baseHue + (Random.value - 0.5f) * 0.2f, 1f);
                colors[i] = new GradientColorKey(Color.HSVToRGB(h, s, v), i / 3f);
            }
        }

        g.SetKeys(colors, alphas);
        return g;
    }

    private void SetLaspParams(float gain, float fallSpeed)
    {
        if (audioHub == null) return;
        void Apply(Lasp.AudioLevelTracker t, float g, float f)
        {
            if (t != null) { t.gain = g; t.fallDownSpeed = f; }
        }
        Apply(audioHub.bassTracker,      gain * 1.5f, fallSpeed);
        Apply(audioHub.midTracker,       gain,        fallSpeed);
        Apply(audioHub.highTracker,      gain,        fallSpeed);
        Apply(audioHub.amplitudeTracker, gain,        fallSpeed);
    }
}
