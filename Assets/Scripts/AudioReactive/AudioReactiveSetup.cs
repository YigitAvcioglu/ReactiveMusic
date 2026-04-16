using UnityEngine;
using UnityEditor;
using Lasp;

public class AudioReactiveSetup
{
    [MenuItem("Tools/Setup Audio Reactive Scene")]
    public static void SetupScene()
    {
        string dataPath = "Assets/Scripts/AudioReactive/AudioSpectrumData.asset";
        AudioSpectrumData data = AssetDatabase.LoadAssetAtPath<AudioSpectrumData>(dataPath);

        GameObject hubObj = new GameObject("AudioAnalyzerHub");
        AudioAnalyzerHub hubScript = hubObj.AddComponent<AudioAnalyzerHub>();
        hubScript.targetData = data;

        AudioLevelTracker CreateTracker(string n, FilterType f, float d)
        {
            GameObject go = new GameObject(n);
            go.transform.SetParent(hubObj.transform);
            AudioLevelTracker t = go.AddComponent<AudioLevelTracker>();
            t.filterType = f; t.dynamicRange = d; return t;
        }

        hubScript.bassTracker = CreateTracker("Tracker_Bass", FilterType.LowPass, 12f);
        hubScript.midTracker = CreateTracker("Tracker_Mid", FilterType.BandPass, 16f);
        hubScript.highTracker = CreateTracker("Tracker_High", FilterType.HighPass, 20f);
        hubScript.amplitudeTracker = CreateTracker("Tracker_Amplitude", FilterType.Bypass, 12f);

        GameObject visualObj = new GameObject("ReactiveLissajousVisuals");
        visualObj.transform.position = new Vector3(0, 2, 5);
        var lissajous = visualObj.AddComponent<ReactiveLissajous>();
        lissajous.data = data;
        lissajous.baseColor = new Color(0, 2f, 2f);
        lissajous.highColor = new Color(2f, 0, 2f);

        GameObject lightObj = new GameObject("ReactiveLight");
        lightObj.transform.position = new Vector3(0, 5, 0);
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Point; l.range = 20f;
        var rlight = lightObj.AddComponent<ReactiveLight>();
        rlight.data = data;

        GameObject pivotObj = new GameObject("ReactiveRotator");
        pivotObj.transform.position = new Vector3(3, 2, 5);
        var rotator = pivotObj.AddComponent<ReactiveRotation>();
        rotator.data = data;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(pivotObj.transform);
        cube.transform.localPosition = new Vector3(2, 0, 0);
        cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        var rmat = cube.AddComponent<ReactiveMaterial>();
        rmat.data = data;
        rmat.baseEmissionColor = Color.yellow;
        rmat.maxEmission = 3f;

        EditorGUIUtility.PingObject(hubObj);
        Debug.Log("Audio reactive scene setup complete!");
    }
}
