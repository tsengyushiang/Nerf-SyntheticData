using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class MatrixExtensions
{
    public static Quaternion ExtractRotation(this Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    public static Vector3 ExtractPosition(this Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }

    public static Vector3 ExtractScale(this Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
}

[System.Serializable]
public class CameraExtrinsics
{
    public List<CameraExtrinsic> extrinsics;
}
[System.Serializable]
public class CameraExtrinsic
{
    public List<float> extrinsic;
    public string id;
    public Matrix4x4 getMat()
    {
        Matrix4x4 mat = new Matrix4x4();
        Vector4 row0 = new Vector4(
            extrinsic[0], //rx
            extrinsic[1], //ux
            extrinsic[2], //lx
            extrinsic[3]  //px
        );
        Vector4 row1 = new Vector4(
            extrinsic[4], //ry
            extrinsic[5], //uy
            extrinsic[6], //ly
            extrinsic[7] //py
        );
        Vector4 row2 = new Vector4(
            extrinsic[8], //rz
            extrinsic[9], //uz
            extrinsic[10], //lz
            extrinsic[11] //pz
        );
        Vector4 row3 = new Vector4(
            extrinsic[12],
            extrinsic[13],
            extrinsic[14],
            extrinsic[15]
        );


        mat.SetRow(0, row0);
        mat.SetRow(1, row1);
        mat.SetRow(2, row2);
        mat.SetRow(3, row3);

        return mat;
    }
}

    public class VritualRoute : MonoBehaviour
{
    public CamManager camManager;
    public int framesForEachCamPair = 30;
    public string externalExtrinsic = @"C:\Users\yushiang\Desktop\projects\DSNeRF\data\yushiang-hat-15view_factor1\CameraExtrinsics-colmapExport.json";
    public string virutalPathFile = Application.streamingAssetsPath + "/render_pose.json";
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 110, 200, 50), "Init/Save VirtualRoute"))
        {
            if (transform.childCount == 0)
            {
                List<GameObject> cameras = camManager.cameras;
                foreach (GameObject obj in cameras)
                {
                    GameObject copyCam = Instantiate(obj,transform);
                }
            }
            saveRenderPose();
        }
        if (GUI.Button(new Rect(210, 110, 200, 50), "LoadExtrinsics"))
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            CameraExtrinsics data = LoadJsonFile();
            foreach(CameraExtrinsic extrinsic in data.extrinsics)
            {
                GameObject cam = Instantiate(new GameObject(), transform);
                cam.name = extrinsic.id;
                Matrix4x4 mat = extrinsic.getMat();
                cam.transform.localScale = mat.ExtractScale();
                cam.transform.rotation = mat.ExtractRotation();
                cam.transform.position = mat.ExtractPosition();
            }
        }
    }
    public CameraExtrinsics LoadJsonFile()
    {
        CameraExtrinsics data = new CameraExtrinsics();
        if (!File.Exists(externalExtrinsic))
        {
            return data;
        }
        System.IO.StreamReader sr = new System.IO.StreamReader(externalExtrinsic);
        if (sr == null)
        {
            return data;
        }
        string json = "{\"extrinsics\":"+sr.ReadToEnd()+"}";
        if (json.Length > 0)
        {
            data = JsonUtility.FromJson<CameraExtrinsics>(json);
        }
        return data;
    }

    public void saveRenderPose()
    {
        List<Matrix4x4> matrixs = GenVirtualRoute(framesForEachCamPair);
        RawJsonExporter.saveMatrixRoute(matrixs, virutalPathFile);
    }

    List<Matrix4x4> GenVirtualRoute(int framesForEachCamPair)
    {
        List<Matrix4x4> matrixs = new List<Matrix4x4>();
        List<Transform> transforms = new List<Transform>();
        foreach (Transform child in transform)
        {
            transforms.Add(child);
        }
        if (transforms.Count>0 && camManager.isLoop)
        {
            transforms.Add(transforms[0]);
        }

        for (int i=0;i< transforms.Count-1;++i)
        {
            Transform cam = transforms[i];
            Transform nextcam = transforms[i+1];

            for (int j=0;j< framesForEachCamPair; ++j)
            {
                float percentage = (float)j / (float)framesForEachCamPair;
                Quaternion q = Quaternion.Lerp(cam.rotation, nextcam.rotation, percentage);
                Vector3 p = Vector3.Lerp(cam.position, nextcam.position, percentage);
                Matrix4x4 transform = new Matrix4x4();
                transform.SetTRS(p, q, new Vector3(0.1f, 0.1f, 0.1f));

                matrixs.Add(transform);
                //Matrix4x4 transform = cam.transform.localToWorldMatrix;
                Vector3 zero = transform.MultiplyPoint(Vector3.zero);
                Vector3 up = transform.MultiplyPoint(Vector3.up);
                Vector3 right = transform.MultiplyPoint(Vector3.right);
                Vector3 forward = transform.MultiplyPoint(Vector3.forward);

                Debug.DrawLine(zero, up, Color.green);
                Debug.DrawLine(zero, right, Color.red);
                Debug.DrawLine(zero, forward, Color.blue);
            }
        }
        return matrixs;
    }

    void Update()
    {
        GenVirtualRoute(framesForEachCamPair);
    }
}
