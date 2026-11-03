using UnityEngine;
using System.Collections.Generic;

public class SkinnedMeshBridge : MonoBehaviour
{
    [Header("Drag your Armature Root (e.g., Hips) here")]
    public Transform rootBone;

    [ContextMenu("Bind Mesh to Armature")]
    public void BindMesh()
    {
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        if (smr == null)
        {
            Debug.LogError("Put this script on the GameObject with the Skinned Mesh Renderer!");
            return;
        }

        if (rootBone == null)
        {
            Debug.LogError("Please assign the Root Bone first!");
            return;
        }

        Mesh mesh = smr.sharedMesh;
        if (mesh == null)
        {
            Debug.LogError("No mesh assigned to the Skinned Mesh Renderer!");
            return;
        }

        // 1. Gather all bones in the hierarchy down from rootBone
        List<Transform> boneList = new List<Transform>();
        GetBonesRecursive(rootBone, boneList);
        Transform[] bones = boneList.ToArray();

        // 2. Generate the missing BindPoses matrix array
        Matrix4x4[] bindPoses = new Matrix4x4[bones.Length];
        for (int i = 0; i < bones.Length; i++)
        {
            // Calculate where each bone is relative to the mesh's local space
            bindPoses[i] = bones[i].worldToLocalMatrix * transform.localToWorldMatrix;
        }

        // 3. Apply the bindposes back to the asset mesh
        mesh.bindposes = bindPoses;

        // 4. Assign the root and hidden bones array to the renderer
        smr.rootBone = rootBone;
        smr.bones = bones;

        Debug.Log($"Successfully bound {bones.Length} bones! The bindpose error should clear.");
    }

    private void GetBonesRecursive(Transform current, List<Transform> list)
    {
        list.Add(current);
        foreach (Transform child in current)
        {
            GetBonesRecursive(child, list);
        }
    }
}
