using UnityEngine;
using System.Collections.Generic;
public struct VisiblePointRecord
{
    public Vector3 xyz;

    // corresponding 2d info
    public List<Vector2> uv; // project uv
    public List<bool> visible; //can be seen in camera_<index>
}
public class VerticesVisibility : MonoBehaviour
{
    public MeshFilter meshfilter;
    Mesh mesh;
    Vector3[] vertices;
    void Start()
    {
        mesh = meshfilter.mesh;
        (meshfilter.gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider).sharedMesh = mesh;
        vertices = mesh.vertices;
        //Debug.Log(vertices.Length);
    }

    public List<VisiblePointRecord> calVisibility(Camera[] cams)
    {
        List<VisiblePointRecord> record = new List<VisiblePointRecord>();

        foreach (Vector3 vertexLocal in vertices)
        {
            Vector3 vertex = meshfilter.gameObject.transform.localToWorldMatrix.MultiplyPoint(vertexLocal);
            VisiblePointRecord r;
            r.xyz = vertex;
            r.uv = new List<Vector2>();
            r.visible = new List<bool>();
            foreach (Camera cam in cams)
            {
                RaycastHit hit;
                Vector3 dir = vertex - cam.gameObject.transform.position;

                Vector3 camCoord = cam.worldToCameraMatrix.MultiplyPoint(vertex);
                camCoord /= camCoord.z;
                PinholeCameraModel m = cam.gameObject.GetComponent<PinholeCameraModel>();

                Vector2 pxixelCoord = new Vector2(camCoord.x * m.fx + m.cx, camCoord.y * m.fy + m.cy);
                //Debug.Log(pxixelCoord);
                r.uv.Add(pxixelCoord);
                if (Physics.Raycast(cam.gameObject.transform.position, dir.normalized, out hit))
                {
                    //Debug.Log(hit.distance - dir.magnitude);
                    if (Mathf.Abs(hit.distance - dir.magnitude) < 1e-2)
                    {
                        Debug.DrawLine(cam.gameObject.transform.position, vertex, Color.white, 1000);
                        r.visible.Add(true);
                    }
                    else
                    {
                        r.visible.Add(false);
                    }
                }
                else
                {
                    r.visible.Add(false);
                }

            }
            record.Add(r);
        }
        return record;
    }

    void Update()
    {
    }
}