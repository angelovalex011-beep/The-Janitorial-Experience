using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpatialWeightTransfer : MonoBehaviour
{
    [Header("1. Drag your ORIGINAL RIGGED FBX GameObject from the Project panel here")]
    public GameObject originalRiggedFBX;

    [ContextMenu("Spatial Transfer Weights")]
    public void TransferWeightsSpatial()
    {
        #if UNITY_EDITOR
        SkinnedMeshRenderer targetSMR = GetComponent<SkinnedMeshRenderer>();
        if (targetSMR == null || originalRiggedFBX == null)
        {
            Debug.LogError("Assign the SkinnedMeshRenderer and the Original Rigged FBX!");
            return;
        }

        // Find the SkinnedMeshRenderer on the original FBX prefab
        SkinnedMeshRenderer sourceSMR = originalRiggedFBX.GetComponentInChildren<SkinnedMeshRenderer>();
        if (sourceSMR == null)
        {
            Debug.LogError("Could not find a SkinnedMeshRenderer inside the Original Rigged FBX!");
            return;
        }

        Undo.RecordObject(targetSMR.sharedMesh, "Spatial Weight Transfer");
        
        Mesh sourceMesh = sourceSMR.sharedMesh;
        Mesh targetMesh = targetSMR.sharedMesh;

        Vector3[] sourceVertices = sourceMesh.vertices;
        BoneWeight[] sourceWeights = sourceMesh.boneWeights;
        BoneWeight[] targetWeights = new BoneWeight[targetMesh.vertexCount];
        Vector3[] targetVertices = targetMesh.vertices;

        // Loop through every new vertex and find the closest old vertex to copy its weight
        for (int i = 0; i < targetVertices.Length; i++)
        {
            float minDistance = float.MaxValue;
            int closestIndex = 0;

            for (int j = 0; j < sourceVertices.Length; j++)
            {
                float dist = Vector3.Distance(targetVertices[i], sourceVertices[j]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestIndex = j;
                }
            }
            targetWeights[i] = sourceWeights[closestIndex];
        }

        targetMesh.boneWeights = targetWeights;
        targetMesh.bindposes = sourceMesh.bindposes;
        targetSMR.bones = sourceSMR.bones;
        targetSMR.rootBone = sourceSMR.rootBone;

        targetMesh.UploadMeshData(false);
        Debug.Log("Spatial weight transfer complete! The bones should now pull the correct physical areas.");
        #endif
    }
}
