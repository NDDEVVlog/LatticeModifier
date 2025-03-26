// using UnityEngine;

// public class FFD : ILatticeDeformTechnique
// {   
//     public float weight = 1f;
//     public void Parameterize(Vector3 boxPivotPoint,Vector3[] originalVerex, Vector3[,,] controlPoints, int gridSizeX, int gridSizeY, int gridSizeZ)
//     {
//         throw new System.NotImplementedException();
//     }
//     public Vector3[] ApplyDeformation(Vector3 boxPivotPoint, Vector3 vertex, Vector3[,,] controlPoints, int gridSizeX, int gridSizeY, int gridSizeZ, float deformationStrength)
//     {
//         // Compute the local coordinate system
//         Vector3 X0 = boxPivotPoint;
//         Vector3 S = controlPoints[gridSizeX - 1, 0, 0] - X0; // X-direction
//         Vector3 T = controlPoints[0, gridSizeY - 1, 0] - X0; // Y-direction
//         Vector3 U = controlPoints[0, 0, gridSizeZ - 1] - X0; // Z-direction

//         // Compute (s, t, u) coordinates
//         Vector3 stu = ComputeSTU(vertex, X0, S, T, U);

//         // Compute the deformed position using Bernstein polynomial
//         Vector3 deformedPosition = ComputeDeformedPosition(controlPoints, stu, gridSizeX - 1, gridSizeY - 1, gridSizeZ - 1);

//         // Apply deformation strength
//         return Vector3.Lerp(vertex, deformedPosition, deformationStrength);
//     }

//     private Vector3 ComputeSTU(Vector3 vertex, Vector3 X0, Vector3 S, Vector3 T, Vector3 U)
//     {
//         Vector3 X_X0 = vertex - X0;

//         Vector3 cross_TU = Vector3.Cross(T, U);
//         Vector3 cross_SU = Vector3.Cross(S, U);
//         Vector3 cross_TS = Vector3.Cross(T, S);

//         float s = Vector3.Dot(cross_TU, X_X0) / Vector3.Dot(cross_TU, S);
//         float t = Vector3.Dot(cross_SU, X_X0) / Vector3.Dot(cross_SU, T);
//         float u = Vector3.Dot(cross_TS, X_X0) / Vector3.Dot(cross_TS, U);

//         return new Vector3(s, t, u);
//     }

//     private Vector3 ComputeDeformedPosition(Vector3[,,] controlPoints, Vector3 stu, int l, int m, int n)
//     {
//         Vector3 result = Vector3.zero;

//         // Evaluate the Bernstein polynomial
//         for (int i = 0; i <= l; i++)
//         {
//             float B_i = Bernstein(l, i, stu.x);
//             for (int j = 0; j <= m; j++)
//             {
//                 float B_j = Bernstein(m, j, stu.y);
//                 for (int k = 0; k <= n; k++)
//                 {
//                     float B_k = Bernstein(n, k, stu.z);
//                     result += B_i * B_j * B_k * controlPoints[i, j, k];
//                 }
//             }
//         }
//         return result;
//     }

//     private float Bernstein(int n, int i, float t)
//     {
//         return BinomialCoefficient(n, i) * Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i);
//     }

//     private int BinomialCoefficient(int n, int k)
//     {
//         if (k == 0 || k == n) return 1;
        
//         int result = 1;
//         k = Mathf.Min(k, n - k); // Use symmetry property: C(n, k) = C(n, n-k)

//         for (int i = 1; i <= k; i++)
//         {
//             result = (result * (n - i + 1)) / i;
//         }

//         return result;
//     }


// }
