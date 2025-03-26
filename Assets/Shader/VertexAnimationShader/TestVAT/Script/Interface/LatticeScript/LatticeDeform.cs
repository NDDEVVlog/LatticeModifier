using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
/// <summary>
/// CPU lattice deformation
/// Only work on the mesh inside lattice
/// Fuckup with those outside the lattice box
/// Faster
/// </summary>
public class LatticeDeform : MonoBehaviour, ILattice
{

    private Mesh originalMesh;

    public Lattice customBox3D;

    [SerializeField] private float deformationStrength = 0.5f;
    public int gridSizeX = 2, gridSizeY = 2, gridSizeZ = 2;
    private Vector3[,,] controlPoints;





    [SerializeReference]
    public ILatticeDeformTechnique deformTechnique;


    [FoldoutGroup("Vector List")]
    public Vector3[] originalVertices, worldVertices,local;
    [FoldoutGroup("Debug")]
    
    public bool debugOriginalVertices = false;
    [FoldoutGroup("Debug")]
    public bool debugWorldVertices = false;
    [FoldoutGroup("Debug")]
    public bool debugLocal = false;
    [FoldoutGroup("Debug")]
    public Color originalVerticesColor = Color.red;
    [FoldoutGroup("Debug")]
    public Color worldVerticesColor = Color.green;
    [FoldoutGroup("Debug")]
    public Color localColor = Color.blue;

    private void Start()
    {

        InitializeLattice(customBox3D);
    }

    private void Update()
    {
        if (customBox3D != null)
        {
            ApplyDeformation();
        }
    }

    public void InitializeLattice(Lattice customBox3D)
    {
        this.customBox3D = customBox3D;
        originalMesh = GetComponent<MeshFilter>().mesh;
        originalVertices = originalMesh.vertices;
        //deformedVertices = new Vector3[originalVertices.Length];

        controlPoints = customBox3D.GetControlGridWorld();

        gridSizeX = customBox3D.GetResolution().x;
        gridSizeY = customBox3D.GetResolution().y;
        gridSizeZ = customBox3D.GetResolution().z;


        worldVertices     = new Vector3[originalVertices.Length];
 
        for (int i = 0; i < originalVertices.Length; i++)
        {
            worldVertices[i] = transform.TransformPoint(originalVertices[i]);
        }
        deformTechnique.Parameterize(customBox3D.transform.position,worldVertices,controlPoints,gridSizeX,gridSizeY,gridSizeZ);
        
    }

    public void ApplyDeformation()
    {
        controlPoints = customBox3D.GetControlGridWorld();  
        

         local = new Vector3[originalVertices.Length];
         var deformedVertices  = deformTechnique.ApplyDeformation(customBox3D.transform.position,worldVertices, controlPoints, gridSizeX, gridSizeY, gridSizeZ, deformationStrength);
        for (int i = 0; i < originalVertices.Length; i++)
        {
            local[i] =  transform.InverseTransformPoint(deformedVertices [i]);
        }

        originalMesh.vertices =local ;
        originalMesh.RecalculateNormals();
        originalMesh.RecalculateBounds();
    }

#region DebugZone
     private void OnDrawGizmos()
    {
        if (debugOriginalVertices)
            DrawGizmoPoints(originalVertices, originalVerticesColor);

        if (debugWorldVertices)
            DrawGizmoPoints(worldVertices, worldVerticesColor);

        if (debugLocal)
            DrawGizmoPoints(local, localColor);
    }

    private void DrawGizmoPoints(Vector3[] points, Color color)
    {
        if (points == null) return;

        Gizmos.color = color;
        foreach (Vector3 point in points)
        {
            Gizmos.DrawSphere(transform.position + point, 0.05f); // Adjust size as needed
        }
    }
    #endregion

}
