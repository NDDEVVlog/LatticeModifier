// using UnityEngine;

// public class TrilinearFFD : ILatticeDeformTechnique
// {   
    
//     public void Parameterize(Vector3 boxPivotPoint,Vector3[] originalVerex, Vector3[,,] controlPoints, int gridSizeX, int gridSizeY, int gridSizeZ)
//     {
//         throw new System.NotImplementedException();
//     }
//     public Vector3 ApplyDeformation(Vector3 boxPivotPoint,Vector3 vertex, Vector3[,,] controlPoints, int gridSizeX, int gridSizeY, int gridSizeZ, float deformationStrength)
//     {
//         return ApplyTrilinearFFD(vertex, controlPoints, gridSizeX, gridSizeY, gridSizeZ);
//     }

//     private Vector3[] ApplyTrilinearFFD(Vector3 vertex, Vector3[,,] controlPoints, int gridSizeX, int gridSizeY, int gridSizeZ)
//     {
//         Vector3 normalizedPos = GetNormalizedLatticePosition(vertex, controlPoints, gridSizeX, gridSizeY, gridSizeZ);

//         int x0 = Mathf.Clamp(Mathf.FloorToInt(normalizedPos.x * (gridSizeX - 1)), 0, gridSizeX - 1);
//         int y0 = Mathf.Clamp(Mathf.FloorToInt(normalizedPos.y * (gridSizeY - 1)), 0, gridSizeY - 1);
//         int z0 = Mathf.Clamp(Mathf.FloorToInt(normalizedPos.z * (gridSizeZ - 1)), 0, gridSizeZ - 1);

//         int x1 = Mathf.Min(x0 + 1, gridSizeX - 1);
//         int y1 = Mathf.Min(y0 + 1, gridSizeY - 1);
//         int z1 = Mathf.Min(z0 + 1, gridSizeZ - 1);

//         float tx = (normalizedPos.x * (gridSizeX - 1)) - x0;
//         float ty = (normalizedPos.y * (gridSizeY - 1)) - y0;
//         float tz = (normalizedPos.z * (gridSizeZ - 1)) - z0;

//         if (controlPoints == null)
//         {
//             Debug.LogError("❌ Control Points are NULL!");
//             return vertex;
//         }

//         Vector3 c000 = controlPoints[x0, y0, z0];
//         Vector3 c100 = controlPoints[x1, y0, z0];
//         Vector3 c010 = controlPoints[x0, y1, z0];
//         Vector3 c110 = controlPoints[x1, y1, z0];

//         Vector3 c001 = controlPoints[x0, y0, z1];
//         Vector3 c101 = controlPoints[x1, y0, z1];
//         Vector3 c011 = controlPoints[x0, y1, z1];
//         Vector3 c111 = controlPoints[x1, y1, z1];

//         Vector3 lerpX1 = Vector3.Lerp(c000, c100, tx);
//         Vector3 lerpX2 = Vector3.Lerp(c010, c110, tx);
//         Vector3 lerpX3 = Vector3.Lerp(c001, c101, tx);
//         Vector3 lerpX4 = Vector3.Lerp(c011, c111, tx);

//         Vector3 lerpY1 = Vector3.Lerp(lerpX1, lerpX2, ty);
//         Vector3 lerpY2 = Vector3.Lerp(lerpX3, lerpX4, ty);

//         Vector3 result = Vector3.Lerp(lerpY1, lerpY2, tz);
//         return result;
//     }

//     private Vector3 GetNormalizedLatticePosition(Vector3 vertex, Vector3[,,] controlPoints, int gridSizeX, int gridSizeY, int gridSizeZ)
//     {
//         Vector3 min = controlPoints[0, 0, 0];
//         Vector3 max = controlPoints[gridSizeX - 1, gridSizeY - 1, gridSizeZ - 1];

//         float s = Mathf.Clamp01((vertex.x - min.x) / (max.x - min.x));
//         float t = Mathf.Clamp01((vertex.y - min.y) / (max.y - min.y));
//         float u = Mathf.Clamp01((vertex.z - min.z) / (max.z - min.z));

//         return new Vector3(s, t, u);
//     }
// }
