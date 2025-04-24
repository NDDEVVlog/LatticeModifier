using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class FluidTest : MonoBehaviour
{
    public int size = 1024;
    public int dispatchSize = 64;
    public ComputeShader shader;
    private int kernelIndex;
    public RenderTexture fluidDensityTexture, fluidDensityRWTexture, fluidVelocityRWTexture;
    
    private void DispatchCompute(int kernel)
    {
        shader.Dispatch (kernel, dispatchSize, dispatchSize, 1);
    }
    private RenderTexture CreateTexture(GraphicsFormat format)
    {
        RenderTexture dataTex = new RenderTexture (size, size, 0, format);
        dataTex.filterMode = FilterMode.Bilinear;
        dataTex.wrapMode = TextureWrapMode.Clamp;
        dataTex.enableRandomWrite = true;
        dataTex.Create ();

        return dataTex;
    }
    
    void Start()
    {
        kernelIndex = shader.FindKernel("main"); // CSMain is the name of the compute shader kernel
        
        //Create textures
        //fluidDensityTexture = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); //float2 velocity
        fluidDensityRWTexture = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); //float3 color , float density
        fluidVelocityRWTexture = CreateTexture(GraphicsFormat.R16G16_SFloat); //float pressure
    }
    void FixedUpdate()
    {
        shader.SetTexture(kernelIndex, "FluidDensity", fluidDensityTexture); // Set input texture
        shader.SetTexture(kernelIndex, "FluidDensityRW", fluidDensityRWTexture); // Set output texture
        shader.SetTexture(kernelIndex, "FluidVelocityRW", fluidVelocityRWTexture); // Set output texture
        DispatchCompute(kernelIndex); // Dispatch the compute shader
    }
}
