using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;


public class TexCoordPosition : MonoBehaviour
{
    private Mesh mesh;
    
    
    public Vector3 addition;
    public List<Vector3> texCoords3;

    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer>();

        mesh = meshFilter.mesh;

        if (mesh == null || mesh.vertexCount == 0)
        {
            Debug.LogError("Mesh is missing or has no vertices!");
            return;
        }

        List<Vector3> texCoords3 = new List<Vector3>(mesh.vertexCount);
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            Vector3 vertex = mesh.vertices[i];
            texCoords3.Add(new Vector3(vertex.x, vertex.y, vertex.z)); // Store 3D position in UV3
        }

        mesh.SetUVs(3, texCoords3); // Store in UV3
        mesh.RecalculateNormals();

    
    }

    void Update()
    {
        if (mesh == null || mesh.vertexCount == 0) return;

        texCoords3 = new List<Vector3>();
        mesh.GetUVs(3, texCoords3); // Read UV3

        for (int i = 0; i < texCoords3.Count; i++)
        {
            texCoords3[i] = addition*0.01f;
        }

        mesh.SetUVs(3, texCoords3); // Update UV3
        mesh.RecalculateBounds();
    }
}
