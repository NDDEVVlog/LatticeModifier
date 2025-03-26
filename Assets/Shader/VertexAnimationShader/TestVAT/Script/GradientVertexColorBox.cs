/*********************
Made By NDDEVGAME
Date : 3/9/2025



*********************/

using UnityEngine;

public class GradientVertexColorBox : MonoBehaviour
{
    public Gradient customGradient; // Define gradient in Inspector (e.g., Red to Blue)
    [Range(0f, 1f)] public float strength = 0.5f; // 0 = full gradient, 1 = solid start color

    public Vector3 boxSize = Vector3.one; // Size of the effect box

    public enum GradientAxis { X, Y, Z }
    public GradientAxis gradientAxis = GradientAxis.X;

    [SerializeField] private MeshFilter targetMeshFilter;
    private Color[] originalColors;

    void Awake()
    {
        // Initialize default gradient if not set
        if (customGradient == null)
        {
            customGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(Color.red, 0f); // Center
            colorKeys[1] = new GradientColorKey(Color.blue, 1f); // Edge
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);
            customGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    void Start()
    {
        if (targetMeshFilter != null)
        {
            StoreOriginalColors();
        }
        else
        {
            Debug.LogWarning("No target MeshFilter assigned in GradientVertexColorBox!");
        }
    }

    void Update()
    {
        if (targetMeshFilter != null)
        {
            ApplyVertexColorsToMesh();
        }
    }

    void StoreOriginalColors()
    {
        Mesh mesh = targetMeshFilter.mesh;
        if (mesh == null) return;

        originalColors = new Color[mesh.vertexCount];
        if (mesh.colors.Length > 0)
            originalColors = (Color[])mesh.colors.Clone();
        else
            for (int i = 0; i < originalColors.Length; i++)
                originalColors[i] = Color.white;
    }

    void ApplyVertexColorsToMesh()
    {
        Vector3 boxCenter = transform.position;
        Vector3 halfSize = boxSize * 0.5f;

        Mesh mesh = targetMeshFilter.mesh;
        if (mesh == null) return;

        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldVertex = targetMeshFilter.transform.TransformPoint(vertices[i]);

            if (worldVertex.x >= boxCenter.x - halfSize.x && worldVertex.x <= boxCenter.x + halfSize.x &&
                worldVertex.y >= boxCenter.y - halfSize.y && worldVertex.y <= boxCenter.y + halfSize.y &&
                worldVertex.z >= boxCenter.z - halfSize.z && worldVertex.z <= boxCenter.z + halfSize.z)
            {
                float distance;
                float halfExtent;
                switch (gradientAxis)
                {
                    case GradientAxis.X:
                        distance = Mathf.Abs(worldVertex.x - boxCenter.x);
                        halfExtent = halfSize.x;
                        break;
                    case GradientAxis.Y:
                        distance = Mathf.Abs(worldVertex.y - boxCenter.y);
                        halfExtent = halfSize.y;
                        break;
                    case GradientAxis.Z:
                        distance = Mathf.Abs(worldVertex.z - boxCenter.z);
                        halfExtent = halfSize.z;
                        break;
                    default:
                        distance = 0f;
                        halfExtent = 1f;
                        break;
                }

                float t = distance / halfExtent;
                float adjustedT = Mathf.Lerp(t, 0, strength);
                colors[i] = customGradient.Evaluate(adjustedT);
            }
            else
            {
                colors[i] = originalColors[i];
            }
        }

        mesh.colors = colors;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}