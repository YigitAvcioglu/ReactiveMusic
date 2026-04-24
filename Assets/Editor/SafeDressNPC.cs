using UnityEngine;
using UnityEditor;

public class SafeDressNPC
{
    [InitializeOnLoadMethod]
    public static void DressSafely()
    {
        GameObject serah = GameObject.Find("DanceNPCs/Extra_NPC_16");
        if (serah == null) return;
        
        // Force swap regardless of state

        // Save position/rotation
        Vector3 pos = serah.transform.position;
        Quaternion rot = serah.transform.rotation;
        Transform parent = serah.transform.parent;
        
        // Keep animator controller if possible
        RuntimeAnimatorController animCtrl = null;
        Animator oldAnim = serah.GetComponent<Animator>();
        if (oldAnim != null) animCtrl = oldAnim.runtimeAnimatorController;

        // Load new casual prefab
        GameObject casualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/npc_casual_set_00/Prefabs/npc_csl_00_character_00f_01.prefab");
        if (casualPrefab == null) return;

        GameObject newGirl = PrefabUtility.InstantiatePrefab(casualPrefab) as GameObject;
        newGirl.name = "Extra_NPC_16_Swapped";
        newGirl.transform.SetParent(parent);
        newGirl.transform.position = pos;
        newGirl.transform.rotation = rot;

        // Restore animator controller
        Animator newAnim = newGirl.GetComponent<Animator>();
        if (newAnim != null && animCtrl != null)
        {
            newAnim.runtimeAnimatorController = animCtrl;
            // Optionally, we assign a random cycle offset just like the spawner did
            newAnim.SetFloat("CycleOffset", Random.Range(0f, 1f));
        }

        // Add a marker so we know it's done
        BoxCollider bc = newGirl.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = Vector3.zero;

        // Apply global material upgrader script logically for URP
        Renderer[] renderers = newGirl.GetComponentsInChildren<Renderer>(true);
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NPC_URP_Material.mat");
        if (mat != null)
        {
            foreach (Renderer r in renderers)
            {
                Material[] mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                r.sharedMaterials = mats;
            }
        }

        GameObject.DestroyImmediate(serah);
        newGirl.name = "Extra_NPC_16"; // Rename back to original so it's transparent

        Debug.Log("Successfully and safely dressed NPC 16 by clean swap!");
    }
}
