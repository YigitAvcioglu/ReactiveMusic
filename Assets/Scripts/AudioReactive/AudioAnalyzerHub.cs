using UnityEngine;
using Lasp;

public class AudioAnalyzerHub : MonoBehaviour
{
    public AudioSpectrumData targetData;
    
    [Header("LASP Trackers")]
    public AudioLevelTracker bassTracker;
    public AudioLevelTracker midTracker;
    public AudioLevelTracker highTracker;
    public AudioLevelTracker amplitudeTracker;

    void Update()
    {
        if (targetData == null) return;

        if (bassTracker != null) targetData.bass = bassTracker.normalizedLevel;
        if (midTracker != null) targetData.mid = midTracker.normalizedLevel;
        if (highTracker != null) targetData.high = highTracker.normalizedLevel;
        if (amplitudeTracker != null) targetData.amplitude = amplitudeTracker.normalizedLevel;
    }
}
