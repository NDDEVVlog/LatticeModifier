using UnityEngine;

public interface ILatticeDeformTechnique
{   
    void Parameterize(Vector3 boxPivotPoint,Vector3[] originalVerex,Vector3[,,] latticeDefaultPoint, int gridSizeX, int gridSizeY, int gridSizeZ);
    Vector3[] ApplyDeformation(Vector3 boxPivotPoint ,Vector3[] originalVertices, Vector3[,,] controlPoints, int gridSizeX, int gridSizeY, int gridSizeZ, float deformationStrength);
}
