using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;

public class ExportColmap : MonoBehaviour
{
    public VerticesVisibility vertexIterator;
    public CamManager camManager;

    string[] names;
    GameObject[] cams;

    string imagefolder = Application.streamingAssetsPath + "/images/";
    string imagefolder1 = Application.streamingAssetsPath + "/images_1/";
    string maskfolder = Application.streamingAssetsPath + "/masks_1/";
    string sparseReconBinary = Application.streamingAssetsPath + "/sparse/0";
    string sparseReconFolder = Application.streamingAssetsPath + "/sparse/unity";
    void Start()
    {
        List<string> camName = new List<string>();
        cams = camManager.cameras.ToArray();
        for(int i = 0; i < cams.Length; i++)
        {
            camName.Add(string.Format("camer_{0}", i));
        }
        names = camName.ToArray();
    }
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 50), "save colmap model"))
        {
            Directory.CreateDirectory(sparseReconBinary);
            Directory.CreateDirectory(sparseReconFolder);
            Directory.CreateDirectory(imagefolder);
            Directory.CreateDirectory(imagefolder1);
            Directory.CreateDirectory(maskfolder);

            List<Camera> camComponents = new List<Camera>();
            foreach(GameObject obj in cams)
            {
                camComponents.Add(obj.GetComponent<Camera>());
            }            
            List<VisiblePointRecord> result = vertexIterator.calVisibility(camComponents.ToArray());

            writeCameraCapture(imagefolder);
            writeCameraCapture(imagefolder1);
            writeCameraMask(maskfolder);
            writeCameraIntrinsics();
            writeCamerapose(result);
            writepoints3d(result);
            UnityEngine.Debug.Log("export colmap data done, get your result at " + Application.streamingAssetsPath);
            convertColmapModelTXT2Bin();
        }

        if (GUI.Button(new Rect(10, 60, 200, 50), "save raw data json"))
        {
            writeCameraRaw();
        }
    }
    void convertColmapModelTXT2Bin()
    {
        Process p = Process.Start(new ProcessStartInfo(@"C:\Users\yushiang\Downloads\COLMAP-3.6-windows-cuda\COLMAP.bat")
        {
            Arguments = "model_converter --input_path " + sparseReconFolder + " --output_path " + sparseReconBinary + " --output_type BIN"
        }) ;
    }
    void writepoints3d(List<VisiblePointRecord> result)
    {
        /*
            # 3D point list with one line of data per point:
            #   POINT3D_ID, X, Y, Z, R, G, B, ERROR, TRACK[] as (IMAGE_ID, POINT2D_IDX)
            # Number of points: 3, mean track length: 3.3334
            63390 1.67241 0.292931 0.609726 115 121 122 1.33927 16 6542 15 7345 6 6714 14 7227
            63376 2.01848 0.108877 -0.0260841 102 209 250 1.73449 16 6519 15 7322 14 7212 8 3991
         */
        string path = sparseReconFolder + "/points3D.txt";
        StreamWriter writer = new StreamWriter(path, false);

        int[] imagePixelid = new int[cams.Length];
        for(int i = 0; i < cams.Length; i++)
        {
            imagePixelid[i] = 0;
        }
        int pointIndex = 0;
        foreach(VisiblePointRecord r in result)
        {
            int visbleCamCount = 0;
            foreach(bool b in r.visible)
            {
                if (b)
                    visbleCamCount++;
            }
            if (visbleCamCount >= 2)
            {
                ++pointIndex;
                //write Point is valid
                writer.Write(
                  string.Format("{0} {1} {2} {3} {4} {5} {6} {7} ",
                  pointIndex, r.xyz.x, r.xyz.y, r.xyz.z, 0, 0, 0, 1e-3
                 ));

                //write corresponding 2d coordinates
                for(int i=0; i < cams.Length; i++)
                {
                    if (r.visible[i])
                    {
                        imagePixelid[i]++;
                        writer.Write(
                          string.Format("{0} {1} ",i+1, imagePixelid[i]));
                    }
                }

                writer.Write("\n");
            }
        }

        writer.Close();
    }
    void writeCameraIntrinsics()
    {
        string path = sparseReconFolder + "/cameras.txt";
        /*
            # Camera list with one line of data per camera:
            #   CAMERA_ID, MODEL, WIDTH, HEIGHT, PARAMS[]
            # Number of cameras: 3
            1 SIMPLE_PINHOLE 3072 2304 2559.81 1536 1152
        */
        StreamWriter writer = new StreamWriter(path, false);

        for (int i = 0; i < cams.Length; i++)
        {
            PinholeCameraModel model= cams[i].GetComponent<PinholeCameraModel>();
            writer.WriteLine(
              string.Format("{0} {1} {2} {3} {4} {5} {6} {7}",
              i + 1, "PINHOLE", model.width, model.height, model.fx, model.fy, model.cx, model.cy
             ));
        }

        writer.Close();
    }
    public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
        return q;
    }
    void writeCamerapose(List<VisiblePointRecord> result)
    {
        string path = sparseReconFolder + "/images.txt";
        /*
            # Image list with two lines of data per image:
            #   IMAGE_ID, QW, QX, QY, QZ, TX, TY, TZ, CAMERA_ID, NAME
            #   POINTS2D[] as (X, Y, POINT3D_ID)
            # Number of images: 2, mean observations per image: 2
            1 0.851773 0.0165051 0.503764 -0.142941 -0.737434 1.02973 3.74354 1 P1180141.JPG
            2362.39 248.498 58396 1784.7 268.254 59027 1784.7 268.254 -1
        */
        StreamWriter writer = new StreamWriter(path, false);

        int[] imagePixelid = new int[cams.Length];
        for (int i = 0; i < cams.Length; i++)
        {
            GameObject cam = cams[i];
            Vector3 T = cam.transform.position;
            Matrix4x4 rotatMat = cam.transform.localToWorldMatrix;
            rotatMat.m03 = 0;
            rotatMat.m13 = 0;
            rotatMat.m23 = 0;
            Matrix4x4 R = rotatMat.inverse;
            Quaternion Q = QuaternionFromMatrix(R);
            Vector3 tvec = -R.MultiplyPoint(T);
            writer.WriteLine(
              string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}",
              i + 1, Q.w, Q.x, Q.y, Q.z, tvec.x, tvec.y, tvec.z, i+1, names[i] + ".png"
             ));

            imagePixelid[i] = 0;
            int pointIndex = 0;
            foreach (VisiblePointRecord r in result)
            {
                int visbleCamCount = 0;
                foreach (bool b in r.visible)
                {
                    if (b)
                        visbleCamCount++;
                }
                if (visbleCamCount >= 2)
                {
                    ++pointIndex;
                    if (r.visible[i])
                    {
                        imagePixelid[i]++;
                        writer.Write(
                          string.Format("{0} {1} {2} ", r.uv[i].x, r.uv[i].y, pointIndex)
                          );
                    }
                }
            }
            writer.Write("\n");
        }
        writer.Close();
    }

    void writeCameraCapture(string imagefolder)
    {
        for (int i = 0; i < names.Length; ++i)
        {
            cams[i].GetComponent<Capture>().CaptureColor(imagefolder + names[i] + ".png");
        }
    }
    void writeCameraMask(string maskfolder)
    {
        for (int i = 0; i < names.Length; ++i)
        {
            cams[i].GetComponent<Capture>().CaptureMask(maskfolder + names[i] + ".png");
        }
    }
    void writeCameraRaw()
    {
        for (int i = 0; i < names.Length; ++i)
        {
            cams[i].GetComponent<Capture>().CaptureRaw(Application.streamingAssetsPath + "/" + names[i] + ".json");
        }
    }
}
