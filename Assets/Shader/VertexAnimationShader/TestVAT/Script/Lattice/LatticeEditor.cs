using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Lattice))]
public class LatticeEditor : Editor
{
    private Lattice lattice;
    private bool[,,] selectedVertices;
    private SerializedProperty resolutionProperty;
    private bool shiftHeld = false;

    private enum EditMode { Move, Rotate, Scale }
    private EditMode currentMode = EditMode.Move;

    private void OnEnable()
    {
        lattice = (Lattice)target;
        resolutionProperty = serializedObject.FindProperty("resolution");
        SyncSelectedVertices();
    }

    private void SyncSelectedVertices()
    {
        Vector3[,,] controlGrid = lattice.GetControlGrid();
        if (controlGrid != null)
        {
            Vector3Int res = lattice.GetResolution();
            selectedVertices = new bool[res.x, res.y, res.z];
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        if (GUILayout.Button(lattice.IsEditMode() ? "Exit Edit Mode" : "Enter Edit Mode"))
        {
            lattice.ToggleEditMode();
            Tools.hidden = lattice.IsEditMode();  // Hide Unity transform tools in edit mode
            SceneView.RepaintAll();
        }

        EditorGUILayout.PropertyField(resolutionProperty, true);

        if (GUILayout.Button("Apply Resolution"))
        {
            serializedObject.ApplyModifiedProperties();
            Vector3Int newResolution = resolutionProperty.vector3IntValue;
            newResolution.x = Mathf.Clamp(newResolution.x, 1, 10);
            newResolution.y = Mathf.Clamp(newResolution.y, 1, 10);
            newResolution.z = Mathf.Clamp(newResolution.z, 1, 10);
            resolutionProperty.vector3IntValue = newResolution;
            lattice.ApplyResolution(newResolution);
            Vector3Int res = lattice.GetResolution();
            selectedVertices = new bool[res.x, res.y, res.z];
            SyncSelectedVertices();
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("ApplyDefaultGrid")){
            lattice.SetDefaultGrid();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        if (!lattice.IsEditMode()) return;

        Event e = Event.current;
        shiftHeld = e.shift;

        Vector3[,,] controlGrid = lattice.GetControlGrid();
        Vector3Int res = lattice.GetResolution();
        Handles.matrix = lattice.transform.localToWorldMatrix;
        Color originalColor = Handles.color;
        Handles.color = Color.green;


        // Detect keyboard input for switching modes
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.W) currentMode = EditMode.Move;
            if (e.keyCode == KeyCode.E) currentMode = EditMode.Rotate;
            if (e.keyCode == KeyCode.R) currentMode = EditMode.Scale;
        }

        // Draw and select vertices
        for (int z = 0; z < res.z; z++)
        {
            for (int y = 0; y < res.y; y++)
            {
                for (int x = 0; x < res.x; x++)
                {
                    if (Handles.Button(controlGrid[x, y, z], Quaternion.identity, 0.02f, 0.05f, Handles.DotHandleCap))
                    {
                        if (!shiftHeld)
                        {
                            System.Array.Clear(selectedVertices, 0, selectedVertices.Length);
                        }
                        selectedVertices[x, y, z] = !selectedVertices[x, y, z];
                        e.Use();
                    }
                }
            }
        }

        // If vertices are selected, apply transformations
        if (HasSelection())
        {
            Vector3 selectionCenter = GetSelectionCenter(controlGrid);
            EditorGUI.BeginChangeCheck();

            // Store previous position to calculate delta
            Vector3 newPosition = selectionCenter;
            Quaternion newRotation = Quaternion.identity;
            Vector3 newScale = Vector3.one;

            if (currentMode == EditMode.Move)
            {
                newPosition = Handles.PositionHandle(selectionCenter, Quaternion.identity);
            }
            else if (currentMode == EditMode.Rotate)
            {
                newRotation = Handles.RotationHandle(Quaternion.identity, selectionCenter);
            }
            else if (currentMode == EditMode.Scale)
            {
                newScale = Handles.ScaleHandle(Vector3.one, selectionCenter, Quaternion.identity, 1f);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(lattice, "Transform Vertices"); // Registers the undo operation

                // Calculate movement offset
                Vector3 movementOffset = newPosition - selectionCenter;

                for (int z = 0; z < res.z; z++)
                {
                    for (int y = 0; y < res.y; y++)
                    {
                        for (int x = 0; x < res.x; x++)
                        {
                            if (selectedVertices[x, y, z])
                            {
                                Vector3 originalPosition = controlGrid[x, y, z];

                                if (currentMode == EditMode.Move)
                                {
                                    controlGrid[x, y, z] = originalPosition + movementOffset;
                                }
                                else if (currentMode == EditMode.Rotate)
                                {
                                    Vector3 relativePosition = originalPosition - selectionCenter;
                                    controlGrid[x, y, z] = selectionCenter + newRotation * relativePosition;
                                }
                                else if (currentMode == EditMode.Scale)
                                {
                                    Vector3 relativePosition = originalPosition - selectionCenter;
                                    controlGrid[x, y, z] = selectionCenter + Vector3.Scale(relativePosition, newScale);
                                }
                            }
                        }
                    }
                }

                Undo.RecordObject(lattice, "Transform Applied"); // Ensure Unity can track this change
                lattice.SetControlGrid(controlGrid);

                // Mark the object as dirty so Unity knows it needs to be saved
                EditorUtility.SetDirty(lattice);
            }
        }

        // Clear selection if Shift is released
        if (!shiftHeld && e.type == EventType.MouseUp)
        {
            System.Array.Clear(selectedVertices, 0, selectedVertices.Length);
        }

        Handles.color = originalColor;
    }


    private bool HasSelection()
    {
        foreach (bool value in selectedVertices)
        {
            if (value) return true;
        }
        return false;
    }

    private Vector3 GetSelectionCenter(Vector3[,,] controlGrid)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        Vector3Int res = lattice.GetResolution();

        for (int z = 0; z < res.z; z++)
        {
            for (int y = 0; y < res.y; y++)
            {
                for (int x = 0; x < res.x; x++)
                {
                    if (selectedVertices[x, y, z])
                    {
                        sum += controlGrid[x, y, z];
                        count++;
                    }
                }
            }
        }

        return count > 0 ? sum / count : Vector3.zero;
    }
}
