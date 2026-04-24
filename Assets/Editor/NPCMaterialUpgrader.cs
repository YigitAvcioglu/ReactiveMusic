#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class NPCMaterialUpgrader
{
    [MenuItem("Tools/Fix NPC Materials")]
    [MenuItem("Tools/Fix NPC Materials")]
    public static void UpgradeMaterials()
    {
        string[] searchFolders = { 
            "Assets/npc_casual_set_00",
            "Assets/Agarkova_CG",
            "Assets/Bandit",
            "Assets/civilian_girl",
            "Assets/Elven_assassin",
            "Assets/Serah Fei Bikini"
        };
        string[] guids = AssetDatabase.FindAssets("t:Material", searchFolders);
        
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogError("[MaterialUpgrader] URP Lit shader bulunamadı!");
            return;
        }

        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (mat != null && mat.shader.name != urpLit.name)
            {
                Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
                
                mat.shader = urpLit;
                
                if (mainTex != null) 
                    mat.SetTexture("_BaseMap", mainTex);
                mat.SetColor("_BaseColor", color);
                
                EditorUtility.SetDirty(mat);
                count++;
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"[MaterialUpgrader] {count} adet materyal URP'ye yükseltildi.");
    }
}
#endif
