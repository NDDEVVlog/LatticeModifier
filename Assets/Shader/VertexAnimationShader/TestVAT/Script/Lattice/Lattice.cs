using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class Lattice : MonoBehaviour
{
    [SerializeField] private Vector3[,,] controlGrid;
    [SerializeField] private Vector3[,,] defaultVertices;
    [SerializeField] private int[] indices;
    [SerializeField] private bool editMode = false;
    [SerializeField] private float size = 1f;
    [SerializeField] private Vector3Int resolution = new Vector3Int(2, 2, 2); // Default resolution
    [SerializeField] private bool drawDefaultVertices = false; // Toggle for drawing default vertices

    public List<Vector3> savedControlGrid = new List<Vector3>(); // Serialized list
    public List<Vector3> savedDefaultGrid = new List<Vector3>();
    private void OnEnable()
    {
        Debug.Log("ControlGrid is null: " + (controlGrid == null) +
                ", ControlGrid Length is 0: " + (controlGrid != null && controlGrid.Length == 0) +
                ", Indices is null: " + (indices == null) +
                ", Indices Length is 0: " + (indices != null && indices.Length == 0));

        if (controlGrid == null || controlGrid.Length == 0 || indices == null || indices.Length == 0)
        {
            RestoreControlGrid();
            RestoreDefaultGrid();

        }
    }

    public void ToggleEditMode() => editMode = !editMode;
    public bool IsEditMode() => editMode;

    #region Resolution
    public void ApplyResolution(Vector3Int newResolution)
    {
        resolution = newResolution;
        CreateControlGrid();
        SaveControlGrid();
        SetDefaultGrid();
        MarkDirty();
    }

    public Vector3Int GetResolution() => resolution;

    #endregion

    private void CreateControlGrid()
    {
        int l = Mathf.Max(resolution.x - 1, 1);
        int m = Mathf.Max(resolution.y - 1, 1);
        int n = Mathf.Max(resolution.z - 1, 1);

        controlGrid = new Vector3[resolution.x, resolution.y, resolution.z];

        // Define the base position as the object's position
        Vector3 X0 = transform.position;

        // Define the size of the box in each direction
        Vector3 S = transform.right * size;    // X-axis direction
        Vector3 T = transform.up * size;       // Y-axis direction
        Vector3 U = transform.forward * size;  // Z-axis direction

        for (int i = 0; i < resolution.x; i++)
        {
            for (int j = 0; j < resolution.y; j++)
            {
                for (int k = 0; k < resolution.z; k++)
                {
                    controlGrid[i, j, k] = (i / (float)l) * S + (j / (float)m) * T + (k / (float)n) * U;
                }
            }
        }
    }




    public Vector3[] GetControlGridWorld1D()
    {
        // Convert 3D array to 1D array
        Vector3[] controlPoints1D = new Vector3[resolution.x * resolution.y * resolution.z];
        Vector3 X0 = transform.position;
        int index = 0;
        for (int x = 0; x < resolution.x; x++)
            for (int y = 0; y < resolution.y; y++)
                for (int z = 0; z < resolution.z; z++)
                    controlPoints1D[index++] = X0 + controlGrid[x, y, z];

        return controlPoints1D;
    }

    public Vector3[] GetDefaultGrid1DWorld1D()
    {
        // Convert 3D array to 1D array
        Vector3[] controlPoints1D = new Vector3[resolution.x * resolution.y * resolution.z];
        int index = 0;
        for (int x = 0; x < resolution.x; x++)
            for (int y = 0; y < resolution.y; y++)
                for (int z = 0; z < resolution.z; z++)
                    controlPoints1D[index++] = defaultVertices[x, y, z];

        return controlPoints1D;
    }


    public Vector3[,,] GetControlGrid() => controlGrid;
    public Vector3[,,] GetControlGridWorld()
    {
        Vector3[,,] worldGrid = new Vector3[resolution.x, resolution.y, resolution.z];
        Vector3 X0 = transform.position;

        for (int i = 0; i < resolution.x; i++)
        {
            for (int j = 0; j < resolution.y; j++)
            {
                for (int k = 0; k < resolution.z; k++)
                {
                    worldGrid[i, j, k] = X0 + controlGrid[i, j, k];
                }
            }
        }
        return worldGrid;
    }

    public Vector3[,,] GetDefaulControlGridtWorld() => defaultVertices;

    public void SetDefaultGrid()
    {
        defaultVertices = new Vector3[resolution.x, resolution.y, resolution.z];
        savedDefaultGrid.Clear();

        for (int i = 0; i < controlGrid.GetLength(0); i++)
        {
            for (int j = 0; j < controlGrid.GetLength(1); j++)
            {
                for (int k = 0; k < controlGrid.GetLength(2); k++)
                {
                    defaultVertices[i, j, k] = controlGrid[i, j, k];
                    savedDefaultGrid.Add(controlGrid[i, j, k]); // Save to list
                }
            }
        }
        Debug.Log("Default Grid Saved: " + savedDefaultGrid.Count + " elements.");
    }

    public void RestoreDefaultGrid()
    {
        if (savedDefaultGrid == null || savedDefaultGrid.Count == 0)
        {
            Debug.LogWarning("No saved default grid found.");
            return;
        }

        defaultVertices = new Vector3[resolution.x, resolution.y, resolution.z];
        int index = 0;
        for (int x = 0; x < resolution.x; x++)
        {
            for (int y = 0; y < resolution.y; y++)
            {
                for (int z = 0; z < resolution.z; z++)
                {
                    if (index < savedDefaultGrid.Count)
                    {
                        defaultVertices[x, y, z] = savedDefaultGrid[index];
                    }
                    index++;
                }
            }
        }
        Debug.Log("Default Grid Restored.");
    }


    public void SetControlGrid(Vector3[,,] newGrid)
    {
        controlGrid = newGrid;
        SaveControlGrid();
        MarkDirty();
    }
    public void SaveControlGrid()
    {
        savedControlGrid.Clear(); // Reset previous data
        if (controlGrid == null) return;

        // Flatten the 3D array into a list
        for (int x = 0; x < resolution.x; x++)
        {
            for (int y = 0; y < resolution.y; y++)
            {
                for (int z = 0; z < resolution.z; z++)
                {
                    savedControlGrid.Add(controlGrid[x, y, z]);
                }
            }
        }
        Debug.Log("Control Grid Saved: " + savedControlGrid.Count + " elements.");
    }

    public void RestoreControlGrid()
    {
        if (savedControlGrid == null || savedControlGrid.Count == 0)
        {
            Debug.LogWarning("No saved control grid found.");
            return;
        }

        // Reconstruct the 3D array
        controlGrid = new Vector3[resolution.x, resolution.y, resolution.z];
        int index = 0;
        for (int x = 0; x < resolution.x; x++)
        {
            for (int y = 0; y < resolution.y; y++)
            {
                for (int z = 0; z < resolution.z; z++)
                {
                    if (index < savedControlGrid.Count)
                    {
                        controlGrid[x, y, z] = savedControlGrid[index];
                    }
                    index++;
                }
            }
        }
        Debug.Log("Control Grid Restored.");
    }



    private void MarkDirty()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    private void OnDrawGizmos()
    {
        if (controlGrid == null) return;

        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix; // Apply object transform for local controlGrid

        int xRes = controlGrid.GetLength(0);
        int yRes = controlGrid.GetLength(1);
        int zRes = controlGrid.GetLength(2);

        for (int z = 0; z < zRes; z++)
        {
            for (int y = 0; y < yRes; y++)
            {
                for (int x = 0; x < xRes; x++)
                {
                    Vector3 current = controlGrid[x, y, z];

                    Gizmos.DrawSphere(current, 0.05f);
                    Gizmos.color = Color.blue;

                    if (x + 1 < xRes) Gizmos.DrawLine(current, controlGrid[x + 1, y, z]);
                    if (y + 1 < yRes) Gizmos.DrawLine(current, controlGrid[x, y + 1, z]);
                    if (z + 1 < zRes) Gizmos.DrawLine(current, controlGrid[x, y, z + 1]);
                }
            }
        }

        if (editMode)
        {
            Gizmos.color = Color.red;
            foreach (var vertex in controlGrid)
            {
                Gizmos.DrawSphere(vertex, 0.05f);
            }
        }

        // Draw Default Vertices in World Space
        if (drawDefaultVertices && defaultVertices != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var vertex in defaultVertices)
            {
                // Convert to world space
                Gizmos.DrawSphere(vertex, 0.1f);
            }
        }
    }

}
