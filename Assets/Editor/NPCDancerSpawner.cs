#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;

public class NPCDancerSpawner : MonoBehaviour
{
    static readonly string[] FemaleNpcs = {
        "Assets/npc_casual_set_00/Prefabs/npc_csl_00_character_01f_01.prefab",
        "Assets/npc_casual_set_00/Prefabs/npc_csl_00_character_01f_02.prefab",
        "Assets/npc_casual_set_00/Prefabs/npc_csl_00_character_01f_03.prefab",
        "Assets/npc_casual_set_00/Prefabs/npc_csl_00_character_02f_01.prefab",
        "Assets/npc_casual_set_00/Prefabs/npc_csl_00_character_02f_02.prefab",
        "Assets/npc_casual_set_00/Prefabs/npc_csl_00_character_02f_03.prefab"
    };

    static readonly string[] MaleNpcs = {
        "Assets/npc_casual_set_00/Prefabs/npc_csl_00_character_01m_01.prefab",
        "Assets/npc_casual_set_00/Prefabs/npc_csl_00_character_01m_02.prefab",
        "Assets/npc_casual_set_00/Prefabs/npc_csl_00_character_01m_03.prefab",
        "Assets/npc_casual_set_00/Prefabs/npc_csl_00_character_02m_01.prefab",
        "Assets/npc_casual_set_00/Prefabs/npc_csl_00_character_02m_02.prefab",
        "Assets/npc_casual_set_00/Prefabs/npc_csl_00_character_02m_03.prefab"
    };

    static readonly string[] ExtraNpcs = {
        "Assets/Agarkova_CG/Woman_with_cap/Prefab/Woman_with_cap.prefab",
        "Assets/Bandit/Prefabs/BanditMain.prefab",
        "Assets/civilian_girl/Prefabs/civilian_girl.prefab",
        "Assets/Elven_assassin/Prefabs/elf_realistic.prefab",
        "Assets/npc_casual_set_00/Prefabs/npc_csl_00_character_01f_01.prefab"
    };

    static readonly string[] AllDances = {
        "Assets/Kevin Iglesias/Human Animations/Animations/Female/Social/Dance/Steps/HumanF@Dance01.fbx",
        "Assets/Kevin Iglesias/Human Animations/Animations/Female/Social/Dance/Steps/HumanF@Dance02.fbx",
        "Assets/Kevin Iglesias/Human Animations/Animations/Female/Social/Dance/Steps/HumanF@Dance03.fbx",
        "Assets/Kevin Iglesias/Human Animations/Animations/Female/Social/Dance/Steps/HumanF@Dance04.fbx",
        "Assets/Kevin Iglesias/Human Animations/Animations/Female/Social/Dance/Steps/HumanF@Dance05.fbx",
        "Assets/Kevin Iglesias/Human Animations/Animations/Female/Social/Dance/Steps/HumanF@Dance06.fbx",
        "Assets/Kevin Iglesias/Human Animations/Animations/Male/Social/Dance/Steps/HumanM@Dance01.fbx",
        "Assets/Kevin Iglesias/Human Animations/Animations/Male/Social/Dance/Steps/HumanM@Dance02.fbx",
        "Assets/Kevin Iglesias/Human Animations/Animations/Male/Social/Dance/Steps/HumanM@Dance03.fbx",
        "Assets/Kevin Iglesias/Human Animations/Animations/Male/Social/Dance/Steps/HumanM@Dance04.fbx",
        "Assets/Kevin Iglesias/Human Animations/Animations/Male/Social/Dance/Steps/HumanM@Dance05.fbx",
        "Assets/Kevin Iglesias/Human Animations/Animations/Male/Social/Dance/Steps/HumanM@Dance06.fbx",
        "Assets/MocapDancer anims/Theme - Idle-Chill-Relax/Shy Dancing 1 (132bpm)/Shy Dancing 1 - dance (animation).fbx"
    };

    [MenuItem("Tools/Spawn Dance NPCs")]
    public static void SpawnDancers()
    {
        var existingRoot = GameObject.Find("DanceNPCs");
        if (existingRoot != null)
            Undo.DestroyObjectImmediate(existingRoot);

        var root = new GameObject("DanceNPCs");
        Undo.RegisterCreatedObjectUndo(root, "Spawn Dance NPCs");

        string controllerFolder = "Assets/GeneratedAnimators";
        Directory.CreateDirectory(controllerFolder);

        List<string> dances = new List<string>(AllDances);
        Shuffle(dances);

        int spawnedCount = 0;
        int danceIdx = 0;

        foreach (var npc in FemaleNpcs)
        {
            SpawnNPC(npc, dances[danceIdx % dances.Count], root.transform, controllerFolder, "Female_NPC_" + spawnedCount, CalculatePosition(spawnedCount), CalculateRotation(spawnedCount));
            spawnedCount++;
            danceIdx++;
        }

        foreach (var npc in MaleNpcs)
        {
            SpawnNPC(npc, dances[danceIdx % dances.Count], root.transform, controllerFolder, "Male_NPC_" + spawnedCount, CalculatePosition(spawnedCount), CalculateRotation(spawnedCount));
            spawnedCount++;
            danceIdx++;
        }

        foreach (var npc in ExtraNpcs)
        {
            SpawnNPC(npc, dances[danceIdx % dances.Count], root.transform, controllerFolder, "Extra_NPC_" + spawnedCount, CalculatePosition(spawnedCount), CalculateRotation(spawnedCount));
            spawnedCount++;
            danceIdx++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log($"[NPCSpawner] Toplam {spawnedCount} karakter sahneye eklendi!");
        Selection.activeGameObject = root;
    }

    static void Shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            string temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

    static Vector3 CalculatePosition(int idx)
    {
        float[] xPositions = { -2.5f, -1.5f, -0.5f, 0.5f, 1.5f, 2.5f };
        int row = idx / 6;  // 0, 1, 2
        int col = idx % 6; 
        
        float zPos = row == 0 ? 0.5f : (row == 1 ? -1.0f : -2.5f);
        float xPos = xPositions[col];
        
        if (row == 1) xPos += 0.5f;
        else if (row == 2) xPos += 0.25f;

        xPos += Random.Range(-0.1f, 0.1f);
        zPos += Random.Range(-0.2f, 0.2f);
        return new Vector3(xPos, 0f, zPos);
    }

    static float CalculateRotation(int idx)
    {
        return Random.Range(-15f, 15f);
    }

    static void SpawnNPC(string prefabPath, string animPath, Transform parent, string ctrlFld, string objName, Vector3 pos, float rot)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) 
        {
            Debug.LogWarning($"Prefab bulunamadi: {prefabPath}");
            return;
        }

        var npc = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        npc.name = objName;
        npc.transform.SetParent(parent);
        npc.transform.localPosition = pos;
        npc.transform.localRotation = Quaternion.Euler(0, rot, 0);
        npc.transform.localScale = Vector3.one;
        Undo.RegisterCreatedObjectUndo(npc, "Spawn " + objName);

        var clip = LoadDanceClip(animPath);
        if (clip != null)
        {
            string ctrlPath = ctrlFld + "/DanceCtrl_" + objName + ".controller";
            var controller = CreateDanceController(ctrlPath, clip);

            var animator = npc.GetComponentInChildren<Animator>();
            if (animator == null) animator = npc.AddComponent<Animator>();
            
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
        }
    }

    static AnimationClip LoadDanceClip(string fbxPath)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        foreach (var a in assets)
        {
            if (a is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                return clip;
        }
        return null;
    }

    static AnimatorController CreateDanceController(string path, AnimationClip clip)
    {
        if (File.Exists(path)) AssetDatabase.DeleteAsset(path);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        var stateMachine = controller.layers[0].stateMachine;

        var state = stateMachine.AddState("Dance");
        state.motion = clip;
        state.speed = Random.Range(0.85f, 1.15f);
        state.cycleOffset = Random.Range(0f, 1f);
        state.cycleOffsetParameter = "";
        
        stateMachine.defaultState = state;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        return controller;
    }
}
#endif
