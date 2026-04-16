using UnityEngine;

[CreateAssetMenu(fileName = "AudioSpectrumData", menuName = "Reactive/AudioSpectrumData")]
public class AudioSpectrumData : ScriptableObject
{
    [Range(0f, 1f)] public float bass;
    [Range(0f, 1f)] public float mid;
    [Range(0f, 1f)] public float high;
    [Range(0f, 1f)] public float amplitude;
}
