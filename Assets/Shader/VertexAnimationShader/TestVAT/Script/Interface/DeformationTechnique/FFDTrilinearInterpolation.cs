using UnityEngine;
using System.Collections.Generic;

public class FFDTrilinearInterpolation : ILatticeDeformTechnique
{
    public Vector3 minVertex, maxVertex;
    public Vector3 S, T, U;
    public List<Vector3Param> vertexParams = new List<Vector3Param>();
    public Vector3[] transformedVertices;
    public float alpha = 0.0f;

    public void SetObjectTransform(Transform transform) { }

    public void Parameterize(Vector3 boxPivotPoint, Vector3[] originalVertices, Vector3[,,] latticeDefaultPoint, int gridSizeX, int gridSizeY, int gridSizeZ)
    {   
        vertexParams.Clear();
        minVertex = latticeDefaultPoint[0, 0, 0];
        maxVertex = latticeDefaultPoint[0, 0, 0];

        for (int i = 0; i < gridSizeX; i++)
            for (int j = 0; j < gridSizeY; j++)
                for (int k = 0; k < gridSizeZ; k++)
                {
                    minVertex = Vector3.Min(minVertex, latticeDefaultPoint[i, j, k]);
                    maxVertex = Vector3.Max(maxVertex, latticeDefaultPoint[i, j, k]);
                }

        S = new Vector3(maxVertex.x - minVertex.x, 0f, 0f);
        T = new Vector3(0f, maxVertex.y - minVertex.y, 0f);
        U = new Vector3(0f, 0f, maxVertex.z - minVertex.z);

        ComputeSTU(originalVertices, boxPivotPoint, S, T, U);
    }

    public Vector3[] ApplyDeformation(Vector3 boxPivotPoint, Vector3[] originalVertices, Vector3[,,] controlPoints, int gridSizeX, int gridSizeY, int gridSizeZ, float deformationStrength)
    {
        transformedVertices = new Vector3[originalVertices.Length];

        for (int i = 0; i < vertexParams.Count; i++)
        {
            Vector3Param param = vertexParams[i];

            bool inside = (param.s >= 0 && param.s <= 1) &&
                          (param.t >= 0 && param.t <= 1) &&
                          (param.u >= 0 && param.u <= 1);

            if (inside)
            {
                transformedVertices[i] = ComputeDeformedWithLinearInterpolation(param, controlPoints, gridSizeX - 1, gridSizeY - 1, gridSizeZ - 1);
            }
            else
            {
                transformedVertices[i] = originalVertices[i];
            }
        }

        return transformedVertices;
    }

    private void ComputeSTU(Vector3[] originalVertices, Vector3 X0, Vector3 S, Vector3 T, Vector3 U)
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

            tmp.s = Mathf.Clamp01(Vector3.Dot(cross_TU, X_X0) / Vector3.Dot(cross_TU, S));
            tmp.t = Mathf.Clamp01(Vector3.Dot(cross_SU, X_X0) / Vector3.Dot(cross_SU, T));
            tmp.u = Mathf.Clamp01(Vector3.Dot(cross_TS, X_X0) / Vector3.Dot(cross_TS, U));

            tmp.p = X0 + (tmp.s * S) + (tmp.t * T) + (tmp.u * U);
            tmp.p0 = X0;

            vertexParams.Add(tmp);
        }
    }

    private Vector3 ComputeDeformedWithLinearInterpolation(Vector3Param param, Vector3[,,] controlPoints, int L, int M, int N)
    {
        float s = Mathf.Clamp01(param.s);
        float t = Mathf.Clamp01(param.t);
        float u = Mathf.Clamp01(param.u);

        Vector3 p000 = controlPoints[0, 0, 0];
        Vector3 p100 = controlPoints[L, 0, 0];
        Vector3 p010 = controlPoints[0, M, 0];
        Vector3 p110 = controlPoints[L, M, 0];
        Vector3 p001 = controlPoints[0, 0, N];
        Vector3 p101 = controlPoints[L, 0, N];
        Vector3 p011 = controlPoints[0, M, N];
        Vector3 p111 = controlPoints[L, M, N];

        // Trilinear interpolation
        Vector3 p00 = Vector3.Lerp(p000, p100, s);
        Vector3 p01 = Vector3.Lerp(p001, p101, s);
        Vector3 p10 = Vector3.Lerp(p010, p110, s);
        Vector3 p11 = Vector3.Lerp(p011, p111, s);

        Vector3 p0 = Vector3.Lerp(p00, p10, t);
        Vector3 p1 = Vector3.Lerp(p01, p11, t);

        Vector3 newPos = Vector3.Lerp(p0, p1, u);
        return newPos;
    }
}
