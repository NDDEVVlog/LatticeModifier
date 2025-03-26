using UnityEngine;
using UnityEditor;

public class DuplicateAndOffset : EditorWindow
{
    private float offsetX = 2f; // X-axis offset
    private float offsetZ = 2f; // Z-axis offset
    private int duplicateCount = 1; // Number of duplicates

    [MenuItem("Tools/Duplicate and Offset")]
    private static void ShowWindow()
    {
        GetWindow<DuplicateAndOffset>("Duplicate and Offset");
    }

    private void OnGUI()
    {
        GUILayout.Label("Duplicate Selected Objects", EditorStyles.boldLabel);
        offsetX = EditorGUILayout.FloatField("Offset X Distance", offsetX);
        offsetZ = EditorGUILayout.FloatField("Offset Z Distance", offsetZ);
        duplicateCount = EditorGUILayout.IntField("Number of Duplicates", duplicateCount);

        if (GUILayout.Button("Duplicate"))
        {
            DuplicateSelectedObjects();
        }
    }

    private void DuplicateSelectedObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected to duplicate.");
            return;
        }

        Undo.RecordObjects(selectedObjects, "Duplicate Objects");

        foreach (GameObject obj in selectedObjects)
        {
            for (int i = 1; i <= duplicateCount; i++)
            {
                Vector3 newPosition = obj.transform.position + new Vector3(offsetX * i, 0, offsetZ * i);
                GameObject duplicate = Instantiate(obj, newPosition, obj.transform.rotation);
                duplicate.name = obj.name + " (Copy " + i + ")";
                Undo.RegisterCreatedObjectUndo(duplicate, "Duplicate Object");
            }
        }
    }
}
