using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Handles static lattice-based mesh deformation in Unity.
/// Uses a lattice grid to apply deformations to the mesh based on control points.
/// Implements the ILattice interface for modularity.
/// Slow at fack
/// </summary>
public class StaticLatticeDeform : MonoBehaviour, ILattice
{
    // Stores the original mesh before deformation
    private Mesh originalMesh;

    // Reference to the Lattice structure used for deformation
    public Lattice customBox3D;

    // Strength of the deformation effect
    [SerializeField] private float deformationStrength = 0.5f;

    // Grid resolution for the lattice
    public int gridSizeX = 2, gridSizeY = 2, gridSizeZ = 2;

    // 3D array storing control points in world space
    private Vector3[,,] controlPoints;

    // The technique used to apply deformation, allowing for flexibility in deformation algorithms
    [SerializeReference]
    public ILatticeDeformTechnique deformTechnique;

    // Arrays for storing vertex data
    [FoldoutGroup("Vector List")]
    public Vector3[] originalVertices, worldVertices, deformedVertices,localDeformedVertices;

    /// <summary>
    /// Initializes the lattice deformation on start.
    /// </summary>
    private void Start()
    {
        InitializeLattice(customBox3D);
    }

    /// <summary>
    /// Applies deformation every frame if a lattice is assigned.
    /// </summary>
    private void Update()
    {
        if (customBox3D != null)
        {
            ApplyDeformation();
        }
    }

    /// <summary>
    /// Initializes the lattice grid by storing control points and setting grid resolution.
    /// </summary>
    /// <param name="customBox3D">The lattice grid used for deformation.</param>
    public void InitializeLattice(Lattice customBox3D)
    {
        this.customBox3D = customBox3D;

        // Get the mesh component and store original vertex positions
        originalMesh = GetComponent<MeshFilter>().mesh;
        originalVertices = originalMesh.vertices;

        // Retrieve the default control grid in world space
        controlPoints = customBox3D.GetDefaulControlGridtWorld();

        // Set grid resolution based on the lattice
        gridSizeX = customBox3D.GetResolution().x;
        gridSizeY = customBox3D.GetResolution().y;
        gridSizeZ = customBox3D.GetResolution().z;
    }

    /// <summary>
    /// Applies the deformation based on control points and lattice settings.
    /// </summary>
    public void ApplyDeformation()
    {   
        // Convert the mesh vertices to world space to ensure deformation is not affected by object movement
        worldVertices = new Vector3[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            worldVertices[i] = transform.TransformPoint(originalVertices[i]);
        }

        // Parameterize the deformation based on the lattice's control points
        deformTechnique.Parameterize(
            customBox3D.transform.position, 
            worldVertices, 
            customBox3D.GetDefaulControlGridtWorld(), 
            gridSizeX, gridSizeY, gridSizeZ
        );

        // Retrieve updated control points in world space
        controlPoints = customBox3D.GetControlGridWorld();

        // Apply the deformation algorithm using control points and deformation strength
        deformedVertices = deformTechnique.ApplyDeformation(
            customBox3D.transform.position, 
            worldVertices, 
            controlPoints, 
            gridSizeX, gridSizeY, gridSizeZ, 
            deformationStrength
        );

        // Convert deformed vertices back to local space before applying to the mesh
         localDeformedVertices = new Vector3[deformedVertices.Length];
        for (int i = 0; i < deformedVertices.Length; i++)
        {
            localDeformedVertices[i] = transform.InverseTransformPoint(deformedVertices[i]);
        }

        // Update the mesh with the new deformed vertex positions
        originalMesh.vertices = localDeformedVertices;
        originalMesh.RecalculateNormals();
        originalMesh.RecalculateBounds();
    }
}
