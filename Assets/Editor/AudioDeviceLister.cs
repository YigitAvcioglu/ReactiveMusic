#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class AudioDeviceLister
{
    [MenuItem("Tools/List Audio Devices")]
    public static void ListDevices()
    {
        Debug.Log("--- Unity Microphone Devices ---");
        foreach (var device in Microphone.devices)
        {
            Debug.Log($"Microphone: {device}");
        }

        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("No microphone devices found by Unity!");
        }
        
        Debug.Log("--- LASP Initialization Check ---");
        // We can't easily list LASP devices from C# as it's native, 
        // but we can see if it's already initialized.
    }
}
#endif
