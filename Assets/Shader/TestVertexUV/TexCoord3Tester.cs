using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TexCoord3Tester : MonoBehaviour
{
    [System.Serializable]
    public struct VertexTextCoord
    {
        public int VertexIndex;
        public Vector3 Pos;
    }

    private MeshFilter meshFilter;
    
    // Exposed list for real-time adjustment in Inspector
    public List<VertexTextCoord> vertexTextCoords = new List<VertexTextCoord>();

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null) return;

        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        
        // Initialize list with default world positions
        vertexTextCoords.Clear();
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            vertexTextCoords.Add(new VertexTextCoord { VertexIndex = i, Pos = worldPos });
        }

        ApplyTexCoord3();
        
        // Assign shader
        //GetComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/MatchTexcoortoVertex"));
    }

    void Update()
    {
        ApplyTexCoord3();
    }

    void ApplyTexCoord3()
    {
        if (meshFilter == null || meshFilter.sharedMesh == null) return;

        Mesh mesh = meshFilter.mesh;
        List<Vector3> worldPositions = new List<Vector3>();

        // Use a for-loop to modify the struct inside the list
        for (int i = 0; i < vertexTextCoords.Count; i++)
        {
            VertexTextCoord vtc = vertexTextCoords[i];  // Get struct (copy)
            //vtc.Pos = new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f)); // Modify value
            vertexTextCoords[i] = vtc;  // Assign back to the list
            worldPositions.Add(vtc.Pos);
        }

        mesh.SetUVs(3, worldPositions);
        mesh.UploadMeshData(false);
    }

}
