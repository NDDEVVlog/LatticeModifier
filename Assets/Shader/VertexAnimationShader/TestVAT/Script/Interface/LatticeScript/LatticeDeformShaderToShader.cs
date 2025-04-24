using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices; // ✅ Needed for Marshal.SizeOf
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Handles static lattice-based mesh deformation in Unity.
/// Uses a Compute Shader to apply deformations to the mesh based on control points.
/// Implements the ILattice interface for modularity.
/// </summary>

[ExecuteAlways]
public class LatticeDeformShaderToShader : MonoBehaviour, ILattice
{
    private Mesh originalMesh;
    public Lattice customBox3D;

    [SerializeField] private float deformationStrength = 0.5f;

    public int gridSizeX = 2, gridSizeY = 2, gridSizeZ = 2;
    private Vector3[] controlPoints1D;

    [FoldoutGroup("Vector List")]
    public Vector3[] originalVertices, worldVertices, deformedVertices,localDeformedVertices;

    // Compute Shader Reference
    public ComputeShader computeShader;

    private ComputeBuffer worldVerticesBuffer;
    private ComputeBuffer controlPointsBuffer;
    private ComputeBuffer defaultControlPointsBuffer;
    private ComputeBuffer deformedVerticesBuffer;
    private ComputeBuffer vertexParamBuffer; // ✅ Fixed buffer allocation

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)] // ✅ Ensures proper memory alignment
    public struct VertexParams
    {
        public Vector3 ori;
        public Vector3 diff;
        public Vector3 p;
        public Vector3 p0;
        public Vector3 extraParam;
    }
    public VertexParams[] vector3Param; // ✅ Fixed: Made private and properly initialized

    private void Start()
    {
        InitializeLattice(customBox3D);
    }

    void Oalidate()
    {   
        if(customBox3D != null)
            InitializeLattice(customBox3D);
        
    }

    private void Update()
    {
        if (customBox3D != null)
        {
            ApplyDeformation();
        }
    }

    [Button]
    public void InitializeLattice(Lattice customBox3D)
    {
        this.customBox3D = customBox3D;
        originalMesh = GetComponent<MeshFilter>().mesh;
        originalVertices = originalMesh.vertices;

        Vector3[,,] controlPoints3D = customBox3D.GetDefaulControlGridtWorld();
        gridSizeX = customBox3D.GetResolution().x;
        gridSizeY = customBox3D.GetResolution().y;
        gridSizeZ = customBox3D.GetResolution().z;

        controlPoints1D = customBox3D.GetDefaultGrid1DWorld1D();
    }

    public void ApplyDeformation()
    {
        int vertexCount = originalVertices.Length;
        if (vertexCount == 0 || controlPoints1D == null || controlPoints1D.Length == 0)
        {
            Debug.LogWarning("Lattice deformation skipped: No vertices or control points.");
            return;
        }

        // ✅ Allocate worldVertices array
        worldVertices = new Vector3[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            worldVertices[i] = transform.TransformPoint(originalVertices[i]);
        }

        // ✅ Allocate vector3Param array
        vector3Param = new VertexParams[vertexCount];

        // ✅ Release existing buffers before creating new ones
        ReleaseBuffers();

        // Prepare Compute Shader
        int kernelID = computeShader.FindKernel("CSMain");

        // ✅ Allocate Compute Buffers
        worldVerticesBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 3);
        controlPointsBuffer = new ComputeBuffer(controlPoints1D.Length, sizeof(float) * 3);
        deformedVerticesBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 3);
        defaultControlPointsBuffer = new ComputeBuffer(controlPoints1D.Length, sizeof(float) * 3);
        vertexParamBuffer = new ComputeBuffer(vertexCount, Marshal.SizeOf(typeof(VertexParams))); // ✅ Fixed stride calculation

        worldVerticesBuffer.SetData(worldVertices);
        controlPointsBuffer.SetData(customBox3D.GetControlGridWorld1D());
        defaultControlPointsBuffer.SetData(customBox3D.GetDefaultGrid1DWorld1D());
        deformedVerticesBuffer.SetData(new Vector3[vertexCount]);
        vertexParamBuffer.SetData(vector3Param); // ✅ Fixed: Ensuring proper size before SetData

        // Set Compute Shader Buffers
        computeShader.SetBuffer(kernelID, "defaultControlPoints", defaultControlPointsBuffer);
        computeShader.SetBuffer(kernelID, "worldVertices", worldVerticesBuffer);
        computeShader.SetBuffer(kernelID, "controlPoints", controlPointsBuffer);
        computeShader.SetBuffer(kernelID, "deformedVertices", deformedVerticesBuffer);
        computeShader.SetBuffer(kernelID, "vertexParam", vertexParamBuffer);

        computeShader.SetFloat("deformationStrength", deformationStrength);
        computeShader.SetInt("vertexCount", vertexCount);
        computeShader.SetInts("gridSize", new int[] { gridSizeX, gridSizeY, gridSizeZ });
        computeShader.SetVector("latticePivotPoint", customBox3D.transform.position);
        // Dispatch Compute Shader
        int threadGroups = Mathf.CeilToInt(vertexCount / 256.0f);
        computeShader.Dispatch(kernelID, threadGroups, 1, 1);

        // Get Deformed Vertices
        deformedVertices = new Vector3[vertexCount];
        deformedVerticesBuffer.GetData(deformedVertices);
        vertexParamBuffer.GetData(vector3Param);
        

        // Convert to Local Space and Apply to Mesh
         localDeformedVertices = new Vector3[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            localDeformedVertices[i] = transform.InverseTransformPoint(deformedVertices[i]);
        }

        originalMesh.SetUVs(3,localDeformedVertices);
        originalMesh.RecalculateNormals();
        originalMesh.RecalculateBounds();
    }



    // ✅ Release Buffers to Prevent Memory Leaks
    private void ReleaseBuffers()
    {
        worldVerticesBuffer?.Release();
        controlPointsBuffer?.Release();
        deformedVerticesBuffer?.Release();
        vertexParamBuffer?.Release();
        defaultControlPointsBuffer?.Release();

        worldVerticesBuffer = null;
        controlPointsBuffer = null;
        deformedVerticesBuffer = null;
        vertexParamBuffer = null;
        defaultControlPointsBuffer = null;
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }
}
