#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MissingScriptFixer
{
    [MenuItem("Tools/Fix Missing Scripts")]
    public static void Fix()
    {
        GameObject go = GameObject.Find("ScriptManager");
        if (go == null)
        {
            Debug.LogError("ScriptManager not found in scene!");
            return;
        }

        // Fix missing scripts
        int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        Debug.Log($"Removed {count} missing scripts from ScriptManager.");

        // Ensure SpotifyMoodManager is present
        var moodManager = go.GetComponent<SpotifyMoodManager>();
        if (moodManager == null)
        {
            moodManager = go.AddComponent<SpotifyMoodManager>();
            Debug.Log("Added missing SpotifyMoodManager to ScriptManager.");
        }

        // Check for duplicates and clean up
        var managers = go.GetComponents<SpotifyMoodManager>();
        if (managers.Length > 1)
        {
            for (int i = 1; i < managers.Length; i++)
            {
                Object.DestroyImmediate(managers[i]);
            }
            Debug.Log($"Removed {managers.Length - 1} duplicate SpotifyMoodManager components.");
        }

        EditorUtility.SetDirty(go);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
        
        Debug.Log("ScriptManager cleanup complete.");
    }
}
#endif
