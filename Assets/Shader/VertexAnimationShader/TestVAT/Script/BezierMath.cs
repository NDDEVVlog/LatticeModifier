using System.Collections.Generic;
using UnityEngine;

public static class BezierMath
{
    public static List<float> ComputeBernsteinPolynomials(int n, float u)
    {
        List<float> B = new List<float>(new float[n + 1]);
        B[0] = 1.0f;

        for (int j = 1; j <= n; j++)
        {
            float saved = 0.0f;
            for (int k = 0; k < j; k++)
            {
                float temp = B[k];
                B[k] = saved + (1 - u) * temp;
                saved = u * temp;
            }
            B[j] = saved;
        }
        return B;
    }

    public static Vector3 ComputeDeformedPosition(Vector3Param param, Vector3[,,] controlPoints, int L, int M, int N)
    {
        Vector3 newPos = Vector3.zero;

        for (int i = 0; i <= L; i++)
        {
            for (int j = 0; j <= M; j++)
            {
                for (int k = 0; k <= N; k++)
                {
                    float weight = param.bernPolyPack[0][i] * param.bernPolyPack[1][j] * param.bernPolyPack[2][k];
                    newPos += weight * controlPoints[i, j, k];
                }
            }
        }

        return newPos;
    }
}
