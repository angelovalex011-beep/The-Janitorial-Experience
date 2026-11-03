using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MirrorScript: MonoBehaviour
{
    [Header("References")]
    public Camera reflectionCamera;     // the child Camera
    public Renderer mirrorRenderer;     // the child Quad's MeshRenderer

    [Header("Settings")]
    [Range(0.25f, 1f)] public float resolutionScale = 1f;
    public float clipOffset = 0.01f;
    public LayerMask reflectLayers = ~0;

    RenderTexture rt;
    int lastW, lastH;
    static bool rendering;

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        if (reflectionCamera) reflectionCamera.enabled = false; // rendered manually
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
        if (rt != null) { rt.Release(); DestroyImmediate(rt); rt = null; }
    }

    void OnBeginCamera(ScriptableRenderContext ctx, Camera cam)
    {
        if (rendering) return;                                  // avoid recursion
        if (cam == reflectionCamera) return;
        if (cam.cameraType != CameraType.Game && cam.cameraType != CameraType.SceneView) return;
        if (reflectionCamera == null || mirrorRenderer == null) return;

        Transform m = mirrorRenderer.transform;
        Vector3 pos = m.position;

        // Use whichever quad normal faces the viewing camera
        Vector3 normal = -m.forward;
        if (Vector3.Dot(normal, cam.transform.position - pos) < 0f) normal = -normal;
        if (Vector3.Dot(normal, cam.transform.position - pos) < 0f) return;

        EnsureRenderTexture(cam);

        Vector4 plane = new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(normal, pos));
        Matrix4x4 reflection = CalculateReflectionMatrix(plane);

        reflectionCamera.CopyFrom(cam);
        reflectionCamera.cullingMask = reflectLayers;
        reflectionCamera.targetTexture = rt;
        reflectionCamera.transform.position = reflection.MultiplyPoint(cam.transform.position);
        reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

        Vector4 clip = CameraSpacePlane(reflectionCamera, pos, normal);
        reflectionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clip);

        rendering = true;
        mirrorRenderer.enabled = false;     // don't reflect the mirror itself
        GL.invertCulling = true;

#if UNITY_2022_2_OR_NEWER
        var request = new UniversalRenderPipeline.SingleCameraRequest { destination = rt };
        if (RenderPipeline.SupportsRenderRequest(reflectionCamera, request))
            RenderPipeline.SubmitRenderRequest(reflectionCamera, request);
#else
        UniversalRenderPipeline.RenderSingleCamera(ctx, reflectionCamera);
#endif

        GL.invertCulling = false;
        mirrorRenderer.enabled = true;
        rendering = false;
    }

    void EnsureRenderTexture(Camera cam)
    {
        int w = Mathf.Max(16, (int)(cam.pixelWidth * resolutionScale));
        int h = Mathf.Max(16, (int)(cam.pixelHeight * resolutionScale));
        if (rt != null && w == lastW && h == lastH) return;

        if (rt != null) { rt.Release(); DestroyImmediate(rt); }
        rt = new RenderTexture(w, h, 24, RenderTextureFormat.DefaultHDR);
        rt.name = "MirrorRT";
        lastW = w; lastH = h;
        mirrorRenderer.sharedMaterial.SetTexture("_ReflectionTex", rt);
    }

    Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal)
    {
        Matrix4x4 v = cam.worldToCameraMatrix;
        Vector3 cPos = v.MultiplyPoint(pos + normal * clipOffset);
        Vector3 cNormal = v.MultiplyVector(normal).normalized;
        return new Vector4(cNormal.x, cNormal.y, cNormal.z, -Vector3.Dot(cPos, cNormal));
    }

    static Matrix4x4 CalculateReflectionMatrix(Vector4 p)
    {
        Matrix4x4 m = Matrix4x4.identity;
        m.m00 = 1f - 2f * p.x * p.x; m.m01 = -2f * p.x * p.y;      m.m02 = -2f * p.x * p.z;      m.m03 = -2f * p.w * p.x;
        m.m10 = -2f * p.y * p.x;      m.m11 = 1f - 2f * p.y * p.y; m.m12 = -2f * p.y * p.z;      m.m13 = -2f * p.w * p.y;
        m.m20 = -2f * p.z * p.x;      m.m21 = -2f * p.z * p.y;      m.m22 = 1f - 2f * p.z * p.z; m.m23 = -2f * p.w * p.z;
        return m;
    }
}