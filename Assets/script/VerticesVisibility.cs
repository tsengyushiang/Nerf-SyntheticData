using UnityEngine;

public class VerticesVisibility : MonoBehaviour
{
    public MeshFilter meshfilter;
    Mesh mesh;
    Vector3[] vertices;
    void Start()
    {
        mesh = meshfilter.mesh;
        vertices = mesh.vertices;
        Debug.Log(vertices.Length);
        Debug.Log(mesh.vertexCount);
    }

    void Update()
    {
    }
}