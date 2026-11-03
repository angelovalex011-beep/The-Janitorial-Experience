using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SkinnedMeshBonesFix : MonoBehaviour
{
    [Header("Drag your active scene Armature Root (e.g., Hips) here")]
    public Transform sceneRootBone;

    [ContextMenu("Force Populate Bones Array")]
    public void ForcePopulateBones()
    {
        #if UNITY_EDITOR
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        if (smr == null)
        {
            Debug.LogError("Attach this script directly to the GameObject with the Skinned Mesh Renderer!");
            return;
        }

        if (sceneRootBone == null)
        {
            Debug.LogError("Please assign the scene root bone (e.g. Hips) first!");
            return;
        }

        Undo.RecordObject(smr, "Populate Skinned Mesh Bones");

        // 1. Gather all bones recursively from the active hierarchy
        System.Collections.Generic.List<Transform> boneList = new System.Collections.Generic.List<Transform>();
        GetBonesRecursive(sceneRootBone, boneList);
        Transform[] gatheredBones = boneList.ToArray();

        // 2. Force assign the root bone and the hidden bones array directly
        smr.rootBone = sceneRootBone;
        smr.bones = gatheredBones;

        // 3. Mark the object dirty so Unity saves the changes
        EditorUtility.SetDirty(smr);

        Debug.Log($"Successfully forced {gatheredBones.Length} bones into the array! The hidden UI should now reveal itself.");
        #endif
    }

          private void GetBonesRecursive(Transform current, System.Collections.Generic.List<Transform> list)
    {
        // ONLY accept bones that explicitly belong to your active Mixamo player skeleton path
        if (current.name.StartsWith("mixamorig"))
        {
            list.Add(current);
        }
        
        foreach (Transform child in current)
        {
            GetBonesRecursive(child, list);
        }
    }


}
