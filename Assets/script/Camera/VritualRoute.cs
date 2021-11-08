using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VritualRoute : MonoBehaviour
{
    public CamManager camManager;
    public int framesForEachCamPair = 30;
    
    public string virutalPathFile = Application.streamingAssetsPath + "/render_pose.json";
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 110, 200, 50), "GenVirtualRoute"))
        {
            saveRenderPose();
        }
    }
    public void saveRenderPose()
    {
        List<Matrix4x4> matrixs = GenVirtualRoute(framesForEachCamPair);
        RawJsonExporter.saveMatrixRoute(matrixs, virutalPathFile);
    }

    List<Matrix4x4> GenVirtualRoute(int framesForEachCamPair)
    {
        List<Matrix4x4> matrixs = new List<Matrix4x4>();
        List<GameObject> cameras = camManager.cameras;
        if (camManager.isLoop)
        {
            cameras.Add(cameras[0]);
        }
        for (int i=0;i<cameras.Count-1;++i)
        {
            GameObject cam = cameras[i];
            GameObject nextcam = cameras[i+1];

            for (int j=0;j< framesForEachCamPair; ++j)
            {
                float percentage = (float)j / (float)framesForEachCamPair;
                Quaternion q = Quaternion.Lerp(cam.transform.rotation, nextcam.transform.rotation, percentage);
                Vector3 p = Vector3.Lerp(cam.transform.position, nextcam.transform.position, percentage);
                Matrix4x4 transform = new Matrix4x4();
                transform.SetTRS(p, q, new Vector3(0.1f, 0.1f, 0.1f));

                matrixs.Add(transform);
                //Matrix4x4 transform = cam.transform.localToWorldMatrix;
                Vector3 zero = transform.MultiplyPoint(Vector3.zero);
                Vector3 up = transform.MultiplyPoint(Vector3.up);
                Vector3 right = transform.MultiplyPoint(Vector3.right);
                Vector3 forward = transform.MultiplyPoint(Vector3.forward);

                Debug.DrawLine(zero, up, Color.green, 1000);
                Debug.DrawLine(zero, right, Color.red, 1000);
                Debug.DrawLine(zero, forward, Color.blue, 1000);
            }
        }
        return matrixs;
    }
}
