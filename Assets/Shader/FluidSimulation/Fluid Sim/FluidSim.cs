using System;
using System.Collections;
using System.Collections.Generic;
//using ExternalPropertyAttributes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[Serializable]
public struct FluidInteract
{
    public bool Disable;
    public Transform InteractPos;
    [ColorUsage(true, true)] public Color InteractColor;

    public bool ExternalMode;
    [Sirenix.OdinInspector.ShowIf("ExternalMode")] public Vector3 ForceDir;
}
public enum DebugRenderType
{
    Velocity, Density, Pressure, Height,Curl
}

public enum FluidSimType
{
    Laplace, Subtract
}

public class FluidSim : MonoBehaviour
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
    [Range(0f,1f)] public float backgroundAlpha = 0;
    [FoldoutGroup("Setup/Settings")] 
    public float TimeScale = 1;
    [FoldoutGroup("Setup/Settings")] 
    [Range(0,1.0f)]
    public float Pressure = 0.5f;
    [FoldoutGroup("Setup/Settings")] 
    [SerializeField] private float dissipation = 0.1f; // Dissipation rate
    

    [FoldoutGroup("Setup")]
    public ComputeShader shader;
    [FoldoutGroup("Setup")]
    public RenderTexture ResultTex;
    [FoldoutGroup("Setup")]
    public Texture2D obstacleTex;
    [FoldoutGroup("Setup")]
    public Material matResult;
    
    
    [FoldoutGroup("Setup")]
    public List<FluidInteract> MultipleSpheres; //represents mouse

    [FoldoutGroup("Force Settings")]
    public float forceIntensity = 200f, forceRange = 0.01f, BrushDensity = 0.4f;
    [FoldoutGroup("Force Settings")]
    [Range(0f,5f)]public float DensityFade = 0.1f;
    
    private List<Vector2> spheres_prevPos;
    
    [FoldoutGroup("Debug")]
    public RenderTexture velocityTex, densityTex, pressureTex, divergenceTex, HeightTex,CurlTex,OutputTex;

    [FoldoutGroup("Debug/Keys")] public KeyCode SpawnKey, MoveKey, CombineKey;

    [FoldoutGroup("Debug/Test")] 
    public FluidSimType FluidSimType;
    [FoldoutGroup("Debug/Test")]
    public DebugRenderType debugType;
    [FoldoutGroup("Debug/Test")]
    public bool DiffuseToggle = true, AdvectionToggle = true, 
                UserInputToggle = true, DivergenceToggle = true, JacobiToggle = true,
                CurlToggle = true, VorticityToggle = true, StartFlowFromCenterToggle = true,
                PressureToggle = true;

    [FoldoutGroup("Debug/Test")]
    public bool FadeDensityToggle = true, Kernel_PixelizeToggle;
    
    [FoldoutGroup("Debug/Kernel ID")]
    public int dispatchSize = 0 ,kernelCount = 0, kernel_Init = 0;
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
    [FoldoutGroup("Debug/Kernel ID")]
    public int Kernel_Curl = 0;
    [FoldoutGroup("Debug/Kernel ID")]
    public int Kernel_Vorticity = 0;
    [FoldoutGroup("Debug/Kernel ID")]
    public int Kernel_PressureInit = 0;
    
    //Execute
    private void DispatchCompute(int kernel)
    {
        shader.Dispatch (kernel, dispatchSize, dispatchSize, 1);
    }
    private RenderTexture CreateTexture(GraphicsFormat format)
    {
        RenderTexture dataTex = new RenderTexture (TextureRenderSize, TextureRenderSize, 0, format);
        dataTex.filterMode = FilterMode.Bilinear;
        dataTex.wrapMode = TextureWrapMode.Clamp;
        dataTex.enableRandomWrite = true;
        dataTex.Create();

        return dataTex;
    }

    private void OnDisable()
    {
        ResultTex.Release();
    }


    private Vector2 texelSize;
    private Vector2 dyeTexelSize;
    //Unity Methods
    void Start ()
    {
        spheres_prevPos = new List<Vector2>();
        // Add elements to the list before setting their values
        for (int i = 0; i < MultipleSpheres.Count; i++)
        {
            spheres_prevPos.Add(Vector2.zero);
        }
        
        //Create textures
        velocityTex = CreateTexture(GraphicsFormat.R16G16_SFloat); //float2 velocity
        densityTex = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); //float3 color , float density
        pressureTex = CreateTexture(GraphicsFormat.R16_SFloat); //float pressure
        divergenceTex = CreateTexture(GraphicsFormat.R16_SFloat); //float divergence
        HeightTex = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); //float height
        CurlTex = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat);
        OutputTex = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat);
        //Output
        matResult.SetTexture ("_MainTex", densityTex);

        //Set shared variables for compute shader
        shader.SetInt("pixelSize",pixelSize);
        shader.SetFloat("forceIntensity",forceIntensity);
        shader.SetFloat("forceRange",forceRange);
        shader.SetFloat("BrushDensity", BrushDensity);
        shader.SetFloat("Curl", Curl);
        shader.SetFloat("Pressure",Pressure);

        texelSize = new Vector2(1.0f / velocityTex.width, 1.0f / velocityTex.height);
        dyeTexelSize = new Vector2(1.0f / densityTex.width, 1.0f / densityTex.height);

        //Set texture for compute shader
        kernel_Init = shader.FindKernel ("Kernel_Init"); kernelCount++;
        
        kernel_Diffusion = shader.FindKernel ("Kernel_Diffusion"); kernelCount++;
        kernel_Advection = shader.FindKernel ("Kernel_Advection"); kernelCount++;
        kernel_Divergence = shader.FindKernel ("Kernel_Divergence"); kernelCount++;
        kernel_Jacobi = shader.FindKernel ("Kernel_Jacobi"); kernelCount++;
        kernel_SubtractGradient = shader.FindKernel ("Kernel_SubtractGradient"); kernelCount++;
        Kernel_UserInput_Add = shader.FindKernel ("Kernel_UserInput_Add"); kernelCount++;
        Kernel_UserInput_Move = shader.FindKernel ("Kernel_UserInput_Move"); kernelCount++;
        Kernel_FadeDensity = shader.FindKernel ("Kernel_FadeDensity"); kernelCount++;
        Kernel_LaplaceProject = shader.FindKernel ("Kernel_LaplaceProject"); kernelCount++;
        Kernel_StartFlowFromCenter = shader.FindKernel("Kernel_StartFlowFromCenter"); kernelCount++;
        Kernel_Curl         = shader.FindKernel("Kernel_Curl");    kernelCount++;
        Kernel_Vorticity    = shader.FindKernel("Kernel_Vorticity");kernelCount++;
        Kernel_PressureInit   = shader.FindKernel("Kernel_PressureInit");kernelCount++;

        for(int kernel = 0; kernel < kernelCount; kernel++)
        {
            /* 
            This example is not optimized, not all kernels read/write into all textures,
            but I keep it like this for the sake of convenience
            */
            
            shader.SetTexture (kernel, "VelocityTex", velocityTex);
            shader.SetTexture (kernel, "DensityTex", densityTex);
            shader.SetTexture (kernel, "ObstacleTex", obstacleTex);
            
            shader.SetTexture (kernel, "DivergenceTex", divergenceTex);
            shader.SetTexture (kernel, "PressureTex", pressureTex);
            shader.SetTexture (kernel, "HeightTex", HeightTex);
            shader.SetTexture (kernel, "CurlTex", CurlTex);
            shader.SetTexture (kernel, "OutputTex", OutputTex);
        }

        //Init data texture value
        dispatchSize = Mathf.CeilToInt(TextureRenderSize / 16);
        DispatchCompute (kernel_Init);
    }
    void FixedUpdate()
    {   

        //send multiples
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
                spherePos[i] = new Vector4(MultipleSpheres[i].InteractPos.localPosition.x, 
                    MultipleSpheres[i].InteractPos.localPosition.y);

                if (!MultipleSpheres[i].ExternalMode) sphereVel[i] = (spherePos[i] - (Vector4)spheres_prevPos[i]) * 5f;
                else sphereVel[i] = (Vector4)MultipleSpheres[i].ForceDir.normalized * 0.01f;
            
                colors[i] = new Vector4(MultipleSpheres[i].InteractColor.r, 
                    MultipleSpheres[i].InteractColor.g, 
                    MultipleSpheres[i].InteractColor.b, 
                    MultipleSpheres[i].InteractColor.a);
            }
        }
        //shader.SetInts("Disables", Dis); // Set sphere position
        shader.SetVectorArray("spherePositions", spherePos); // Set sphere position
        shader.SetVectorArray("spheresVelocity", sphereVel); // Set sphere vel
        shader.SetVectorArray("dyeColors", colors); // Set sphere position
        shader.SetFloat("Dissipation", dissipation);
        shader.SetVector("TexelSize", texelSize);
        shader.SetVector("DyeTexelSize", dyeTexelSize);
        
        
        shader.SetFloat("_deltaTime", Time.fixedDeltaTime * TimeScale);
        shader.SetFloat("backgroundAlpha", backgroundAlpha);
        shader.SetFloat("Densityfade", DensityFade);
        shader.SetFloat("Curl", Curl);

        TestCompute();
        
        //Save the previous positions for velocity
        for (int i = 0; i < MultipleSpheres.Count; i++)
        {
            spheres_prevPos[i] = spherePos[i];
        }
        
		DebugTexture(debugType);
    }

    void TestCompute()
    {
        //Run compute shader
        
        
        if (UserInputToggle)
        {
            if (Input.GetKey(CombineKey))
            {
                DispatchCompute(Kernel_UserInput_Add);
                DispatchCompute(Kernel_UserInput_Move);
            }
            if(Input.GetKey(SpawnKey)) DispatchCompute (Kernel_UserInput_Add);
            if(Input.GetKey(MoveKey)) DispatchCompute (Kernel_UserInput_Move);
        }
        if(CurlToggle)  DispatchCompute(Kernel_Curl);
        if(VorticityToggle) DispatchCompute(Kernel_Vorticity);

        
        
        if(DivergenceToggle) DispatchCompute (kernel_Divergence);
        
        if (PressureToggle) DispatchCompute(Kernel_PressureInit);
        
        for(int i=0; i<solverIterations; i++)
        {
            if(JacobiToggle) DispatchCompute (kernel_Jacobi);
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
        if(DiffuseToggle) DispatchCompute (kernel_Diffusion);
        if(AdvectionToggle) DispatchCompute (kernel_Advection);
        
        if(FadeDensityToggle) DispatchCompute (Kernel_FadeDensity);
        if(StartFlowFromCenterToggle) DispatchCompute (Kernel_StartFlowFromCenter);
        
    }

    void OnValidate()
    {
        shader.SetInt("size",TextureRenderSize);
        shader.SetInt("pixelSize",pixelSize);
        shader.SetFloat("forceIntensity",forceIntensity);
        shader.SetFloat("forceRange",forceRange);
        shader.SetFloat("BrushDensity", BrushDensity);
        shader.SetFloat("Densityfade", DensityFade);
        shader.SetFloat("Curl", Curl);
        shader.SetFloat("Pressure",Pressure);
        dispatchSize = Mathf.CeilToInt(TextureRenderSize / 16);
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
            case DebugRenderType.Curl:
                pickedRenderTexture = CurlTex;
                break;
        }
        
        Graphics.Blit(pickedRenderTexture, ResultTex);
    }
}
