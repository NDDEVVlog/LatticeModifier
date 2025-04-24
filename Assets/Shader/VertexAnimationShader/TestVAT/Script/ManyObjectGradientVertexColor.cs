using UnityEngine;
using System.Collections.Generic;

public class ManyObjectGradientVertexColor : MonoBehaviour
{
    public Gradient customGradient;
    [Range(0f, 1f)] public float strength = 0.5f;
    public Vector3 boxSize = Vector3.one;
    public enum GradientAxis { X, Y, Z }
    public GradientAxis gradientAxis = GradientAxis.X;

    private List<MeshFilter> meshFilters = new List<MeshFilter>();
    private Dictionary<MeshFilter, Color[]> originalColors = new Dictionary<MeshFilter, Color[]>();

    void Awake()
    {
        if (customGradient == null)
        {
            customGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2]
            {
                new GradientColorKey(Color.red, 0f),
                new GradientColorKey(Color.blue, 1f)
            };
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            };
            customGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    void Start()
    {
        FindAllMeshFilters();
        StoreOriginalColors();
    }

    void Update()
    {
        ApplyVertexColorsToMeshes();
    }

    void FindAllMeshFilters()
    {
        meshFilters.Clear();
        meshFilters.AddRange(FindObjectsOfType<MeshFilter>());
    }

    void StoreOriginalColors()
    {
        foreach (var meshFilter in meshFilters)
        {
            Mesh mesh = meshFilter.mesh;
            if (mesh == null) continue;

            Color[] colors = mesh.colors.Length > 0 ? (Color[])mesh.colors.Clone() : new Color[mesh.vertexCount];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.white;

            originalColors[meshFilter] = colors;
        }
    }

    void ApplyVertexColorsToMeshes()
    {
        Vector3 boxCenter = transform.position;
        Vector3 halfSize = boxSize * 0.5f;

        foreach (var meshFilter in meshFilters)
        {
            Mesh mesh = meshFilter.mesh;
            if (mesh == null) continue;

            Vector3[] vertices = mesh.vertices;
            Color[] colors = new Color[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldVertex = meshFilter.transform.TransformPoint(vertices[i]);
                
                if (worldVertex.x >= boxCenter.x - halfSize.x && worldVertex.x <= boxCenter.x + halfSize.x &&
                    worldVertex.y >= boxCenter.y - halfSize.y && worldVertex.y <= boxCenter.y + halfSize.y &&
                    worldVertex.z >= boxCenter.z - halfSize.z && worldVertex.z <= boxCenter.z + halfSize.z)
                {
                    float distance, halfExtent;
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
                    colors[i] = originalColors.ContainsKey(meshFilter) ? originalColors[meshFilter][i] : Color.white;
                }
            }
            
            mesh.colors = colors;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }













                                








}





                                        




                                        