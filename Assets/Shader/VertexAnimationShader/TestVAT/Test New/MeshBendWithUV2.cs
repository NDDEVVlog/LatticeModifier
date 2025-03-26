using UnityEngine;
using System.Collections.Generic;

public class MeshBendWithUV2 : MonoBehaviour
{
    public Transform target; // Object hút mesh
    public float bendStrength = 0.5f; // Cường độ bẻ cong
    private Mesh mesh;
    private List<Vector3> targetPositions = new List<Vector3>();

    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || target == null) return;

        mesh = meshFilter.mesh;
        targetPositions = new List<Vector3>(mesh.vertexCount);

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            targetPositions.Add(target.position); // Lưu target vào UV2
        }

        mesh.SetUVs(2, targetPositions);
        mesh.UploadMeshData(false);
    }

    void Update()
    {
        if (target == null || mesh == null) return;

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            targetPositions[i] = target.position; // Cập nhật vị trí target mới
        }

        mesh.SetUVs(2, targetPositions); // Cập nhật UV2 mỗi frame
    }
}
