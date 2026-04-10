using UnityEditor;
using UnityEngine;

public class ClearAuthTool
{
    [MenuItem("Tools/Clear Spotify Auth")]
    public static void ClearAuth()
    {
        PlayerPrefs.DeleteKey("PKCE-credentials");
        PlayerPrefs.Save();
        string path = Application.persistentDataPath + "/PKCE-credentials.json";
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        string path2 = "PKCE-credentials.json";
        if (System.IO.File.Exists(path2)) System.IO.File.Delete(path2);
        
        Debug.Log("Cleared old PKCE credentials from all locations.");
    }
}
