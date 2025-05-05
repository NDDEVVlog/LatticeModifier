using UnityEngine;

[ExecuteAlways]
public class RenderTextureBoxGizmo : MonoBehaviour
{
    public Camera renderCamera;
    public RenderTexture renderTexture;
    public float distanceFromCamera = 5f;
    public Color gizmoColor = Color.cyan;

    static readonly int GlobalRenderTex = Shader.PropertyToID("_GlobalRenderTex");
    static readonly int GlobalBoxCenter = Shader.PropertyToID("_BoxCenter");
    static readonly int GlobalBoxRight = Shader.PropertyToID("_BoxRight");
    static readonly int GlobalBoxUp = Shader.PropertyToID("_BoxUp");

    void Update()
    {
        if (!renderCamera || !renderTexture) return;

        // Aspect and dimensions
        float aspect = (float)renderTexture.width / renderTexture.height;
        float fov = renderCamera.fieldOfView;
        float height = 2f * distanceFromCamera * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float width = height * aspect;

        // Basis vectors
        Vector3 center = renderCamera.transform.position + renderCamera.transform.forward * distanceFromCamera;
        Quaternion rotation = renderCamera.transform.rotation;

        Vector3 halfRight = rotation * Vector3.right * (width / 2f);
        Vector3 halfUp = rotation * Vector3.up * (height / 2f);

        // Set global values
        Shader.SetGlobalTexture(GlobalRenderTex, renderTexture);
        Shader.SetGlobalVector(GlobalBoxCenter, center);
        Shader.SetGlobalVector(GlobalBoxRight, halfRight);
        Shader.SetGlobalVector(GlobalBoxUp, halfUp);
    }

    void OnDrawGizmos()
    {
        if (!renderCamera || !renderTexture) return;

        float aspect = (float)renderTexture.width / renderTexture.height;
        float fov = renderCamera.fieldOfView;
        float height = 2f * distanceFromCamera * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float width = height * aspect;

        Vector3 center = renderCamera.transform.position + renderCamera.transform.forward * distanceFromCamera;
        Quaternion rotation = renderCamera.transform.rotation;

        Vector3 halfRight = rotation * Vector3.right * (width / 2f);
        Vector3 halfUp = rotation * Vector3.up * (height / 2f);

        Vector3 topLeft = center - halfRight + halfUp;
        Vector3 topRight = center + halfRight + halfUp;
        Vector3 bottomLeft = center - halfRight - halfUp;
        Vector3 bottomRight = center + halfRight - halfUp;

        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
