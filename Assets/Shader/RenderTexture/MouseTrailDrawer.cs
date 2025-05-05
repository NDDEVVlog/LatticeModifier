using UnityEngine;

public class MouseTrailDrawer : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture trailTexture;
    public Camera renderCamera;
    public float fadeDuration = 20f;
    public int trailPixelRadius = 5;
    public Color trailColor = Color.red;

    int kernel;

    void Start()
    {
        if (!trailTexture)
        {
            trailTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGBFloat);
            trailTexture.enableRandomWrite = true;
            trailTexture.Create();
        }

        kernel = computeShader.FindKernel("CSMain");
        Shader.SetGlobalTexture("_TrailTex", trailTexture);
    }

    void Update()
    {
        if (!renderCamera) return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 viewport = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0f);

        computeShader.SetTexture(kernel, "Result", trailTexture);
        computeShader.SetFloat("DeltaTime", Time.deltaTime);
        computeShader.SetFloat("FadeDuration", fadeDuration);
        computeShader.SetInt("TrailRadius", trailPixelRadius);
        computeShader.SetVector("MouseUV", viewport);
        computeShader.SetVector("TrailColor", (Vector4)trailColor);

        int threadGroupsX = Mathf.CeilToInt(trailTexture.width / 8f);
        int threadGroupsY = Mathf.CeilToInt(trailTexture.height / 8f);
        computeShader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);
    }
}