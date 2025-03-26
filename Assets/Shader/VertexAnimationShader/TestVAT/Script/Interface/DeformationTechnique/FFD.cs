using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements the simplest Free-Form Deformation (FFD) for manipulating vertex positions in 3D space.
/// </summary>
public class FFD : ILatticeDeformTechnique
{
    public float weight = 1f;
    public Vector3 minVertex, maxVertex;
    public Vector3 S, T, U;
    public List<Vector3Param> vertexParams = new List<Vector3Param>();
    public Vector3[] transformedVertices;
    public List<Vector3> debugControlPoint;
    public List<string> debugInfo = new List<string>();


    // Pass the object's transform to align coordinates
    public void SetObjectTransform(Transform transform)
    {

    }

    /// <summary>
    /// Parameterizes the object's vertices relative to a grid of control points.
    /// </summary>
    /// <param name="boxPivotPoint">Pivot point of the deformation grid.</param>
    /// <param name="originalVertices">Array of original vertex positions.</param>
    /// <param name="controlPoints">3D array of control points forming the deformation grid.</param>
    /// <param name="gridSizeX">Number of control points along the X-axis.</param>
    /// <param name="gridSizeY">Number of control points along the Y-axis.</param>
    /// <param name="gridSizeZ">Number of control points along the Z-axis.</param>
    public void Parameterize(Vector3 boxPivotPoint, Vector3[] originalVertices, Vector3[,,] controlPoints, int gridSizeX, int gridSizeY, int gridSizeZ)
    {
        minVertex = controlPoints[0, 0, 0];
        maxVertex = controlPoints[0, 0, 0];
        for (int i = 0; i < gridSizeX; i++)
            for (int j = 0; j < gridSizeY; j++)
                for (int k = 0; k < gridSizeZ; k++)
                {
                    minVertex = Vector3.Min(minVertex, controlPoints[i, j, k]);
                    maxVertex = Vector3.Max(maxVertex, controlPoints[i, j, k]);
                }

        S = new Vector3(maxVertex.x - minVertex.x, 0f, 0f);
        T = new Vector3(0f, maxVertex.y - minVertex.y, 0f);
        U = new Vector3(0f, 0f, maxVertex.z - minVertex.z);

        ComputeSTU(originalVertices, boxPivotPoint, S, T, U, gridSizeX, gridSizeY, gridSizeZ);
    }

     /// <summary>
    /// Applies deformation to the object's vertices based on control point displacement.
    /// </summary>
    /// <param name="boxPivotPoint">Pivot point of the deformation grid.</param>
    /// <param name="originalVertices">Original vertex positions.</param>
    /// <param name="controlPoints">3D array of control points affecting deformation.</param>
    /// <param name="gridSizeX">Grid size along the X-axis.</param>
    /// <param name="gridSizeY">Grid size along the Y-axis.</param>
    /// <param name="gridSizeZ">Grid size along the Z-axis.</param>
    /// <param name="deformationStrength">Strength of the deformation.</param>
    /// <returns>Array of transformed vertex positions.</returns>
    public Vector3[] ApplyDeformation(Vector3 boxPivotPoint, Vector3[] originalVertices, Vector3[,,] controlPoints, int gridSizeX, int gridSizeY, int gridSizeZ, float deformationStrength)
    {
        transformedVertices = new Vector3[originalVertices.Length];
        int idx = 0;
        foreach (Vector3Param vp in vertexParams)
        {
            Vector3 deformedPosition = ComputeDeformedPosition(vp, controlPoints, gridSizeX - 1, gridSizeY - 1, gridSizeZ - 1);

            transformedVertices[idx++] = deformedPosition;
        }
        return transformedVertices;
    }
    
    /// <summary>
    /// Everything Belows is the simplest  Deformation Equation
    /// </summary>
    /// <param name="originalVertices"></param>
    /// <param name="X0"></param>
    /// <param name="S"></param>
    /// <param name="T"></param>
    /// <param name="U"></param>
    /// <param name="L"></param>
    /// <param name="M"></param>
    /// <param name="N"></param>

    private void ComputeSTU(Vector3[] originalVertices, Vector3 X0, Vector3 S, Vector3 T, Vector3 U, int L, int M, int N)
    {
        vertexParams.Clear();
        for (int v = 0; v < originalVertices.Length; v++)
        {
            Vector3 vertexWorld = originalVertices[v]; // Assumed world space
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

            tmp.p = X0 + (tmp.s * S) + (tmp.t * T) + (tmp.u * U);
            tmp.p0 = X0;

            tmp.bernPolyPack = new List<List<float>>();
            tmp.bernPolyPack.Add(new List<float>());
            tmp.bernPolyPack.Add(new List<float>());
            tmp.bernPolyPack.Add(new List<float>());

            for (int i = 0; i <= L - 1; i++)
                tmp.bernPolyPack[0].Add(Bernstein(L - 1, i, tmp.s));
            for (int j = 0; j <= M - 1; j++)
                tmp.bernPolyPack[1].Add(Bernstein(M - 1, j, tmp.t));
            for (int k = 0; k <= N - 1; k++)
                tmp.bernPolyPack[2].Add(Bernstein(N - 1, k, tmp.u));

            vertexParams.Add(tmp);
        }
    }

    private Vector3 ComputeDeformedPosition(Vector3Param r, Vector3[,,] controlPoints, int l, int m, int n)
    {
        Vector3 tS = Vector3.zero;
        for (int i = 0; i <= l; i++)
        {
            float B_i = r.bernPolyPack[0][i];
            for (int j = 0; j <= m; j++)
            {
                float B_j = r.bernPolyPack[1][j];
                for (int k = 0; k <= n; k++)
                {
                    float B_k = r.bernPolyPack[2][k];
                    tS += B_i * B_j * B_k * controlPoints[i, j, k];
                }
            }
        }
        return tS;
    }

    private float Bernstein(int n, int i, float t)
    {
        if (t < 0f) t = 0f;
        if (t > 1f) t = 1f;
        return Factorial(n) / (Factorial(i) * Factorial(n - i)) * Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i);
    }

    private float Factorial(int num)
    {
        if (num <= 0) return 1f;
        float result = 1f;
        for (int i = 1; i <= num; i++)
            result *= i;
        return result;
    }

    float BinomialCoefficient(int n, int k)
    {
        if (k == 0 || k == n) return 1;
        k = Mathf.Min(k, n - k);
        int result = 1;
        for (int i = 1; i <= k; i++)
            result = (result * (n - i + 1)) / i;
        return result;
    }
}

[System.Serializable]
public class Vector3Param
{
    public List<List<float>> bernPolyPack;
    public Vector3 p = Vector3.zero;
    public Vector3 ori = Vector3.zero;
    public Vector3 diff = Vector3.zero;
    public Vector3 p0 = Vector3.zero;
    public float s, t, u;

    public Vector3Param()
    {
        s = t = u = 0f;
    }

    public Vector3Param(Vector3Param v)
    {
        s = v.s;
        t = v.t;
        u = v.u;
        p = v.p;
        p0 = v.p0;
    }
}