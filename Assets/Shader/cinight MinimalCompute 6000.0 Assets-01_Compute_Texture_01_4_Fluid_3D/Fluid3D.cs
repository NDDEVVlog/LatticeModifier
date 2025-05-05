/* Reference
My code was originally based on: https://github.com/Scrawk/GPU-GEMS-2D-Fluid-Simulation
Nice tutorial understanding basic fluid concept: https://www.youtube.com/watch?v=iKAVRgIrUOU
Very nice tutorial for artists to understand the maths: https://shahriyarshahrabi.medium.com/gentle-introduction-to-fluid-simulation-for-programmers-and-technical-artists-7c0045c40bac
*/

using System.ComponentModel;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class Fluid3D : MonoBehaviour 
{
	public ComputeShader shader;
	public Material matResult;
	public int size = 256;
	public Transform sphere; //represents mouse
	public int solverIterations = 50;
	[Range(0,1.0f)]
	public float pressure = 0.8f;

	[Header("Force Settings")]
	public float forceIntensity = 200f;
	public float forceRange = 0.01f;
	private Vector3 sphere_prevPos = Vector3.zero;
	//public Color dyeColor = Color.white;

	[Header("Curl and FadeDensity")]
	public float Curl = 1.0f;
	[Range(0f, 5f)]
	public float DensityFade = 0.1f;


	public Color fountainColor;

	[Space(10)]

	public RenderTexture velocityTex;
	public RenderTexture densityTex;
	public RenderTexture pressureTex;
	public RenderTexture divergenceTex;
	public RenderTexture curlTex;


	[SerializeField] private int dispatchSize = 0;
	[SerializeField] private int kernel_Init = 0;
	[SerializeField] private int kernel_Diffusion = 0;
	[SerializeField] private int kernel_UserInput = 0;
	[SerializeField] private int kernel_Jacobi = 0;
	[SerializeField] private int kernel_Advection = 0;
	[SerializeField] private int kernel_Divergence = 0;
	[SerializeField] private int kernel_SubtractGradient = 0;
	[SerializeField] private int kernel_StartFlowFromCenter = 0;
	[SerializeField] private int kernal_FadeDensity = 0;
	[SerializeField] private int kernel_Curl = 0;
	[SerializeField] private int kernel_Vorticity = 0;
	[SerializeField] private int kernel_PressureInit = 0;


	
	
    public bool isFadeDensity = true;

    private RenderTexture CreateTexture(GraphicsFormat format)
	{
		RenderTexture dataTex = new RenderTexture (size, size, format, 0);
		dataTex.volumeDepth = size;
		dataTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
		dataTex.filterMode = FilterMode.Bilinear;
		dataTex.wrapMode = TextureWrapMode.Clamp;
		dataTex.enableRandomWrite = true;
		dataTex.Create ();

		return dataTex;
	}

	private void DispatchCompute(int kernel)
	{
		shader.Dispatch (kernel, dispatchSize, dispatchSize, dispatchSize);
	}

	void Start () 
	{
		//Create textures
		velocityTex = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); //float3 velocity , float unused
		densityTex = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); //float3 color , float density
		pressureTex = CreateTexture(GraphicsFormat.R16_SFloat); //float pressure
		divergenceTex = CreateTexture(GraphicsFormat.R16_SFloat); //float divergence
		curlTex = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat);

		//Output
		matResult.SetTexture ("_MainTex", densityTex);

		//Set shared variables for compute shader
		shader.SetInt("size",size);
		shader.SetFloat("forceIntensity",forceIntensity);
		shader.SetFloat("forceRange",forceRange);
		shader.SetFloat("Densityfade", DensityFade);
        shader.SetFloat("Curl", Curl);
		shader.SetFloat("Pressure",pressure);

		//Set texture for compute shader
		/* 
		This example is not optimized, some textures are readonly, 
		but I keep it like this for the sake of convenience
		*/
		kernel_Init = shader.FindKernel ("Kernel_Init");
		shader.SetTexture (kernel_Init, "VelocityTex", velocityTex);
		shader.SetTexture (kernel_Init, "DensityTex", densityTex);
		shader.SetTexture (kernel_Init, "PressureTex", pressureTex);
		shader.SetTexture (kernel_Init, "DivergenceTex", divergenceTex);
		shader.SetTexture (kernel_Init, "CurlTex", curlTex);

		kernel_Diffusion = shader.FindKernel ("Kernel_Diffusion");
		shader.SetTexture (kernel_Diffusion, "DensityTex", densityTex);

		kernel_Advection = shader.FindKernel ("Kernel_Advection");
		shader.SetTexture (kernel_Advection, "VelocityTex", velocityTex);
		shader.SetTexture (kernel_Advection, "DensityTex", densityTex);

		kernel_UserInput = shader.FindKernel ("Kernel_UserInput");
		shader.SetTexture (kernel_UserInput, "VelocityTex", velocityTex);
		shader.SetTexture (kernel_UserInput, "DensityTex", densityTex);

		kernel_Divergence = shader.FindKernel ("Kernel_Divergence");
		shader.SetTexture (kernel_Divergence, "VelocityTex", velocityTex);
		shader.SetTexture (kernel_Divergence, "DivergenceTex", divergenceTex);

		kernel_Jacobi = shader.FindKernel ("Kernel_Jacobi");
		shader.SetTexture (kernel_Jacobi, "DivergenceTex", divergenceTex);
		shader.SetTexture (kernel_Jacobi, "PressureTex", pressureTex);

		kernel_SubtractGradient = shader.FindKernel ("Kernel_SubtractGradient");
		shader.SetTexture (kernel_SubtractGradient, "PressureTex", pressureTex);
		shader.SetTexture (kernel_SubtractGradient, "VelocityTex", velocityTex);

		kernel_StartFlowFromCenter = shader.FindKernel("Kernel_StartFlowFromCenter");
		shader.SetTexture (kernel_StartFlowFromCenter, "VelocityTex", velocityTex);
		shader.SetTexture (kernel_StartFlowFromCenter, "DensityTex", densityTex);

		
		kernal_FadeDensity = shader.FindKernel("Kernel_FadeDensity"); // Fix typo: "kernal" to "kernel"
		shader.SetTexture(kernal_FadeDensity, "PressureTex", pressureTex);
		shader.SetTexture(kernal_FadeDensity, "VelocityTex", velocityTex);
		shader.SetTexture(kernal_FadeDensity, "DensityTex", densityTex);

		kernel_PressureInit = shader.FindKernel("Kernel_PressureInit"); // Fix typo: "kernal" to "kernel"
		shader.SetTexture(kernel_PressureInit, "PressureTex", pressureTex);

		kernel_Vorticity = shader.FindKernel("Kernel_Vorticity");
		shader.SetTexture (kernel_Vorticity, "VelocityTex", velocityTex);
		shader.SetTexture (kernel_Vorticity, "CurlTex", curlTex);

		kernel_Curl = shader.FindKernel("Kernel_Curl");
		shader.SetTexture (kernel_Curl, "VelocityTex", velocityTex);
		shader.SetTexture (kernel_Curl, "CurlTex", curlTex);
		//Init data texture value
		dispatchSize = Mathf.CeilToInt(size / 8);
		DispatchCompute (kernel_Init);
	}

	
	void FixedUpdate()
	{
		//Send sphere (mouse) position
		Vector3 npos = new Vector3( sphere.position.x / transform.lossyScale.x, sphere.position.y / transform.lossyScale.y, sphere.position.z / transform.lossyScale.z );
		shader.SetVector("spherePos",npos);
	
		//Send sphere (mouse) velocity
		Vector3 velocity = npos - sphere_prevPos;
		shader.SetVector("sphereVelocity",velocity);
		shader.SetVector("fountainColor", fountainColor);
		shader.SetFloat("_deltaTime", Time.fixedDeltaTime);
		    // Calculate time-based color
			float time = Time.time * 0.5f;
			Color newColor = new Color(
				0.5f + 0.5f * Mathf.Sin(time),           // Red oscillates
				0.5f + 0.5f * Mathf.Sin(time + 1.0f),    // Green oscillates (offset phase)
				0.5f + 0.5f * Mathf.Sin(time + 2.0f)     // Blue oscillates (offset phase)
			);

			// Pass the new color to the shader
			shader.SetVector("dyeColor", newColor);

		//Run compute shader
		DispatchCompute (kernel_StartFlowFromCenter);
		
		//
		DispatchCompute (kernel_Curl);
		//DispatchCompute (kernel_Vorticity);
		DispatchCompute (kernel_Divergence);
		
		DispatchCompute (kernel_UserInput);
		DispatchCompute (kernel_PressureInit);
		
		for(int i=0; i<solverIterations; i++)
		{
			DispatchCompute (kernel_Jacobi);
		}
		DispatchCompute (kernel_SubtractGradient);
		DispatchCompute (kernel_Diffusion);
		DispatchCompute (kernel_Advection);
		
		
		if(isFadeDensity) DispatchCompute(kernal_FadeDensity);

		
		//Save the previous position for velocity
		sphere_prevPos = npos;
	}

    void OnValidate()
    {
        shader.SetInt("size",size);
		shader.SetFloat("forceIntensity",forceIntensity);
		shader.SetFloat("forceRange",forceRange);
		shader.SetFloat("Densityfade", DensityFade);
        shader.SetFloat("Curl", Curl);

    }
}
