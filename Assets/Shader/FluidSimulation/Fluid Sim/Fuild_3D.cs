using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class Fuild_3D : MonoBehaviour
{
    [FoldoutGroup("Setup/Settings")]
    public int TextureRenderSize = 1024;
    [FoldoutGroup("Setup/Settings")]
    public int solverIterations = 50;
    [FoldoutGroup("Setup/Settings")]
    public float Curl = 1;
    [FoldoutGroup("Setup/Settings")]
    public int pixelSize = 64;
    [FoldoutGroup("Setup/Settings")]
    [Range(0f, 1f)] public float backgroundAlpha = 0;
    [FoldoutGroup("Setup/Settings")]
    public float TimeScale = 1;


    [FoldoutGroup("Setup")]
    public ComputeShader shader;
    [FoldoutGroup("Setup")]
    public RenderTexture ResultTex;
    [FoldoutGroup("Setup")]
    public Texture3D obstacleTex;
    [FoldoutGroup("Setup")]
    public Material matResult;

    [FoldoutGroup("Setup")]
    public List<FluidInteract> MultipleSpheres; //represents mouse




    [FoldoutGroup("Force Settings")]
    public float forceIntensity = 200f, forceRange = 0.01f, BrushDensity = 0.4f;
    [FoldoutGroup("Force Settings")]
    [Range(0f, 5f)] public float DensityFade = 0.1f;
    private List<Vector2> spheres_prevPos;

    [FoldoutGroup("Debug")]
    public RenderTexture velocityTex, densityTex, pressureTex, divergenceTex, HeightTex;

    [FoldoutGroup("Debug/Keys")] public KeyCode SpawnKey, MoveKey, CombineKey;

    [FoldoutGroup("Debug/Test")]
    public FluidSimType FluidSimType;
    [FoldoutGroup("Debug/Test")]
    public DebugRenderType debugType;
    [FoldoutGroup("Debug/Test")]
    public bool DiffuseToggle = true, AdvectionToggle = true,
        UserInputToggle = true, DivergenceToggle = true, JacobiToggle = true;
    [FoldoutGroup("Debug/Test")]
    public bool FadeDensityToggle = true, Kernel_PixelizeToggle;

    [FoldoutGroup("Debug/Kernel ID")]
    public int dispatchSize = 0, kernelCount = 0, kernel_Init = 0;
    [FoldoutGroup("Debug/Kernel ID")]
    public int kernel_Diffusion = 0, kernel_UserInput = 0, kernel_Jacobi = 0;
    [FoldoutGroup("Debug/Kernel ID")]
    public int kernel_Advection = 0, kernel_Divergence = 0, kernel_SubtractGradient = 0;
    [FoldoutGroup("Debug/Kernel ID")]
    public int Kernel_UserInput_Add = 0, Kernel_UserInput_Move = 0, Kernel_FadeDensity = 0;
    [FoldoutGroup("Debug/Kernel ID")]
    public int Kernel_LaplaceProject = 0;
    [FoldoutGroup("Debug/Kernel ID")]
    public int Kernel_StartFlowFromCenter = 0;


    private RenderTexture CreateTexture(GraphicsFormat format)
    {
        RenderTexture dataTex = new RenderTexture(TextureRenderSize, TextureRenderSize, 0, format)
        {
            volumeDepth = TextureRenderSize,
            dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            enableRandomWrite = true,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        dataTex.enableRandomWrite = true;
        dataTex.Create();

        return dataTex;
    }

    private void DispatchCompute(int kernel)
    {
        shader.Dispatch(kernel, dispatchSize, dispatchSize, dispatchSize);
    }

    void Start()
    {
        spheres_prevPos = new List<Vector2>();
        // Add elements to the list before setting their values
        for (int i = 0; i < MultipleSpheres.Count; i++)
        {
            spheres_prevPos.Add(Vector2.zero);
        }

        //Create textures
        velocityTex = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); //float2 velocity
        densityTex = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); //float3 color , float density
        pressureTex = CreateTexture(GraphicsFormat.R16_SFloat); //float pressure
        divergenceTex = CreateTexture(GraphicsFormat.R16_SFloat); //float divergence
        HeightTex = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); //float height

        //Output
        matResult.SetTexture("_MainTex", densityTex);

        //Set shared variables for compute shader
        shader.SetInt("pixelSize", pixelSize);
        shader.SetFloat("forceIntensity", forceIntensity);
        shader.SetFloat("forceRange", forceRange);
        shader.SetFloat("BrushDensity", BrushDensity);

        //Set texture for compute shader
        List<int> kernelIndices = new List<int>();

        kernel_Init = shader.FindKernel("Kernel_Init"); kernelIndices.Add(kernel_Init);
        kernel_Diffusion = shader.FindKernel("Kernel_Diffusion"); kernelIndices.Add(kernel_Diffusion);
        kernel_Advection = shader.FindKernel("Kernel_Advection"); kernelIndices.Add(kernel_Advection);
        kernel_Divergence = shader.FindKernel("Kernel_Divergence"); kernelIndices.Add(kernel_Divergence);
        kernel_Jacobi = shader.FindKernel("Kernel_Jacobi"); kernelIndices.Add(kernel_Jacobi);
        kernel_SubtractGradient = shader.FindKernel("Kernel_SubtractGradient"); kernelIndices.Add(kernel_SubtractGradient);
        //Kernel_UserInput_Add = shader.FindKernel("Kernel_UserInput_Add"); kernelIndices.Add(Kernel_UserInput_Add);
        //Kernel_UserInput_Move = shader.FindKernel("Kernel_UserInput_Move"); kernelIndices.Add(Kernel_UserInput_Move);
        //Kernel_FadeDensity = shader.FindKernel("Kernel_FadeDensity"); kernelIndices.Add(Kernel_FadeDensity);
        //Kernel_LaplaceProject = shader.FindKernel("Kernel_LaplaceProject"); kernelIndices.Add(Kernel_LaplaceProject);
        Kernel_StartFlowFromCenter = shader.FindKernel("Kernel_StartFlowFromCenter"); kernelIndices.Add(Kernel_StartFlowFromCenter);
        // Now set textures correctly:
        foreach (var kernel in kernelIndices)
        {
            shader.SetTexture(kernel, "VelocityTex", velocityTex);
            shader.SetTexture(kernel, "DensityTex", densityTex);
            shader.SetTexture(kernel, "ObstacleTex", obstacleTex);
            shader.SetTexture(kernel, "DivergenceTex", divergenceTex);
            shader.SetTexture(kernel, "PressureTex", pressureTex);
            shader.SetTexture(kernel, "HeightTex", HeightTex);
        }


        //Init data texture value
        dispatchSize = Mathf.CeilToInt(TextureRenderSize /8);
        DispatchCompute(kernel_Init);
    }

    void FixedUpdate()
    {   
        


                // Send multiples
        shader.SetFloat("PosAmount", MultipleSpheres.Count);

        Vector4[] spherePos = new Vector4[MultipleSpheres.Count];
        Vector4[] sphereVel = new Vector4[MultipleSpheres.Count];
        Vector4[] colors = new Vector4[MultipleSpheres.Count];
        int[] Dis = new int[MultipleSpheres.Count];

        for (int i = 0; i < MultipleSpheres.Count; i++)
        {
            Dis[i] = MultipleSpheres[i].Disable ? 1 : 0;

            if (Dis[i] == 0)
            {
                // Get position relative to world scale
                Transform t = MultipleSpheres[i].InteractPos;
                Vector3 scaledPos = new Vector3(
                    t.position.x / transform.lossyScale.x,
                    t.position.y / transform.lossyScale.y,
                    t.position.z / transform.lossyScale.z
                );

                spherePos[i] = new Vector4(scaledPos.x, scaledPos.y, scaledPos.z, 1f);

                // Calculate velocity
                if (!MultipleSpheres[i].ExternalMode)
                    sphereVel[i] = (spherePos[i] - (Vector4)spheres_prevPos[i]) * 5f;
                else
                    sphereVel[i] = (Vector4)MultipleSpheres[i].ForceDir.normalized * 0.01f;

                // Set color
                Color c = MultipleSpheres[i].InteractColor;
                colors[i] = new Vector4(c.r, c.g, c.b, c.a);
            }
        }

        // Set arrays to shader
        shader.SetVectorArray("spherePositions", spherePos);
        shader.SetVectorArray("spheresVelocity", sphereVel);
        shader.SetVectorArray("dyeColors", colors);

        // Other settings
        shader.SetFloat("_deltaTime", Time.fixedDeltaTime * TimeScale);
        shader.SetFloat("backgroundAlpha", backgroundAlpha);
        //shader.SetFloat("Densityfade", DensityFade);
        //shader.SetFloat("Curl", Curl);

		DispatchCompute (kernel_Diffusion);
		DispatchCompute (kernel_Advection);
		DispatchCompute (kernel_UserInput);
		DispatchCompute (kernel_Divergence);
		for(int i=0; i<solverIterations; i++)
		{
			DispatchCompute (kernel_Jacobi);
		}
		DispatchCompute (kernel_SubtractGradient);
		DispatchCompute (Kernel_StartFlowFromCenter);
        // Compute logic
        //TestCompute();

        // Store previous positions
        for (int i = 0; i < MultipleSpheres.Count; i++)
        {
            spheres_prevPos[i] = spherePos[i];
        }

        DebugTexture(debugType);
    }

    void TestCompute()
    {
        //Run compute shader

        // if (UserInputToggle)
        // {
        //     if (Input.GetKey(CombineKey))
        //     {
        //         DispatchCompute(Kernel_UserInput_Add);
        //         DispatchCompute(Kernel_UserInput_Move);
        //     }
        //     if (Input.GetKey(SpawnKey)) DispatchCompute(Kernel_UserInput_Add);
        //     if (Input.GetKey(MoveKey)) DispatchCompute(Kernel_UserInput_Move);
        // }

        if (DiffuseToggle) DispatchCompute(kernel_Diffusion);
        if (AdvectionToggle) DispatchCompute(kernel_Advection);
        if (DivergenceToggle) DispatchCompute(kernel_Divergence);

        for (int i = 0; i < solverIterations; i++)
        {
            if (JacobiToggle) DispatchCompute(kernel_Jacobi);
        }

        switch (FluidSimType)
        {
            case FluidSimType.Laplace:
                DispatchCompute(Kernel_LaplaceProject);
                break;
            case FluidSimType.Subtract:
                DispatchCompute(kernel_SubtractGradient);
                break;
        }

        if (FadeDensityToggle) DispatchCompute(Kernel_FadeDensity);
        DispatchCompute (Kernel_StartFlowFromCenter);
    }

    void OnValidate()
    {
        shader.SetInt("size", TextureRenderSize);
        shader.SetInt("pixelSize", pixelSize);
        shader.SetFloat("forceIntensity", forceIntensity);
        shader.SetFloat("forceRange", forceRange);
        shader.SetFloat("BrushDensity", BrushDensity);
        shader.SetFloat("Densityfade", DensityFade);

        dispatchSize = Mathf.CeilToInt(TextureRenderSize / 8);
    }

    private RenderTexture pickedRenderTexture;
    public void DebugTexture(DebugRenderType Type)
    {
        switch (Type)
        {
            case DebugRenderType.Density:
                pickedRenderTexture = densityTex;
                break;
            case DebugRenderType.Velocity:
                pickedRenderTexture = velocityTex;
                break;
            case DebugRenderType.Pressure:
                pickedRenderTexture = pressureTex;
                break;
            case DebugRenderType.Height:
                pickedRenderTexture = HeightTex;
                break;
        }

        Graphics.Blit(pickedRenderTexture, ResultTex);
    }
}
