using UnityEngine;
using System.Collections.Generic;

public class FFDLocalDeform : ILatticeDeformTechnique
{
    public Vector3 minVertex, maxVertex;
    public Vector3 S, T, U;
    public List<Vector3Param> vertexParams = new List<Vector3Param>();
    public Vector3[] transformedVertices;
    public float alpha = 0.8f;

    public void SetObjectTransform(Transform transform) { }

    public void Parameterize(Vector3 boxPivotPoint, Vector3[] originalVertices, Vector3[,,] controlPoints, int gridSizeX, int gridSizeY, int gridSizeZ)
    {
        // Xác định min/max vertex
        minVertex = controlPoints[0, 0, 0];
        maxVertex = controlPoints[gridSizeX - 1, gridSizeY - 1, gridSizeZ - 1];

        // Xác định vector S, T, U của lattice box
        S = new Vector3(maxVertex.x - minVertex.x, 0f, 0f);
        T = new Vector3(0f, maxVertex.y - minVertex.y, 0f);
        U = new Vector3(0f, 0f, maxVertex.z - minVertex.z);

        ComputeSTU(originalVertices, boxPivotPoint, S, T, U, gridSizeX, gridSizeY, gridSizeZ);
    }

    public Vector3[] ApplyDeformation(Vector3 boxPivotPoint, Vector3[] originalVertices, Vector3[,,] controlPoints, int gridSizeX, int gridSizeY, int gridSizeZ, float deformationStrength)
    {
        transformedVertices = new Vector3[originalVertices.Length];

        for (int i = 0; i < vertexParams.Count; i++)
        {
            transformedVertices[i] = ComputeDeformedWithContinuity(vertexParams[i], controlPoints, gridSizeX - 1, gridSizeY - 1, gridSizeZ - 1);
        }

        return transformedVertices;
    }

    private void ComputeSTU(Vector3[] originalVertices, Vector3 X0, Vector3 S, Vector3 T, Vector3 U, int L, int M, int N)
    {
        vertexParams.Clear();

        foreach (Vector3 vertexWorld in originalVertices)
        {
            Vector3 X_X0 = vertexWorld - X0;
            Vector3Param tmp = new Vector3Param();
            tmp.ori = vertexWorld;
            tmp.diff = X_X0;

            Vector3 cross_TU = Vector3.Cross(T, U);
            Vector3 cross_SU = Vector3.Cross(S, U);
            Vector3 cross_TS = Vector3.Cross(T, S);

            tmp.s = Vector3.Dot(cross_TU, X_X0) / Vector3.Dot(cross_TU, S);
            tmp.t = Vector3.Dot(cross_SU, X_X0) / Vector3.Dot(cross_SU, T);
            tmp.u = Vector3.Dot(cross_TS, X_X0) / Vector3.Dot(cross_TS, U);

            // **Kiểm tra xem điểm có nằm trong lattice box không**
            if (tmp.s < 0 || tmp.s > 1 || tmp.t < 0 || tmp.t > 1 || tmp.u < 0 || tmp.u > 1)
            {
                continue; // Bỏ qua điểm nằm ngoài lattice box
            }

            tmp.p = X0 + (tmp.s * S) + (tmp.t * T) + (tmp.u * U);
            tmp.p0 = X0;

            tmp.bernPolyPack = new List<List<float>>();
            tmp.bernPolyPack.Add(BezierMath.ComputeBernsteinPolynomials(L - 1, tmp.s));
            tmp.bernPolyPack.Add(BezierMath.ComputeBernsteinPolynomials(M - 1, tmp.t));
            tmp.bernPolyPack.Add(BezierMath.ComputeBernsteinPolynomials(N - 1, tmp.u));

            vertexParams.Add(tmp);
        }
    }

    private Vector3 ComputeDeformedWithContinuity(Vector3Param param, Vector3[,,] controlPoints, int L, int M, int N)
    {
        Vector3 newPos = Vector3.zero;
        Vector3 tangentS = Vector3.zero;
        Vector3 tangentT = Vector3.zero;
        Vector3 tangentU = Vector3.zero;

        for (int i = 0; i <= L; i++)
        {
            for (int j = 0; j <= M; j++)
            {
                for (int k = 0; k <= N; k++)
                {
                    float weight = param.bernPolyPack[0][i] * param.bernPolyPack[1][j] * param.bernPolyPack[2][k];
                    newPos += weight * controlPoints[i, j, k];

                    // **Giữ biên cố định để đảm bảo local deformation**
                    if (i == 0 || i == L || j == 0 || j == M || k == 0 || k == N)
                        continue;

                    // **C1 Continuity - Compute Tangents**
                    if (i < L) tangentS += weight * (controlPoints[i + 1, j, k] - controlPoints[i, j, k]);
                    if (j < M) tangentT += weight * (controlPoints[i, j + 1, k] - controlPoints[i, j, k]);
                    if (k < N) tangentU += weight * (controlPoints[i, j, k + 1] - controlPoints[i, j, k]);
                }
            }
        }

        // **Local Deformation với C1 & C2 smoothing**
        newPos += alpha * (tangentS + tangentT + tangentU);

        return newPos;
    }
}
