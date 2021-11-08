using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class c2wMaxtrixs
{
    public List<c2wMaxtrix> matrixs = new List<c2wMaxtrix>();
}

[System.Serializable]
public class c2wMaxtrix
{
    public float[] elements = new float[16];
}

public class CamerInfo
{
    public List<int> colormap_raw;
    public List<uint> depthmap_raw;
    public float depthscale;
    public int width;
    public int height;
    public float fx;
    public float fy;
    public float ppx;
    public float ppy;
}

public class RawJsonExporter
{
    public static void saveRawJson(PinholeCameraModel m, List<int> colorArray, List<uint> depthArray,float depthsacle,string filepath)
    {
        CamerInfo caminfo = new CamerInfo();
        caminfo.depthmap_raw = depthArray;
        caminfo.colormap_raw = colorArray;
        caminfo.depthscale = depthsacle;
        caminfo.width = m.width;
        caminfo.height = m.height;
        caminfo.fx = m.fx;
        caminfo.fy = m.fy;
        caminfo.ppx = m.cx;
        caminfo.ppy = m.cy;
        var outputString = JsonUtility.ToJson(caminfo);
        System.IO.File.WriteAllText(filepath, outputString);
        Debug.Log(filepath);
    }

    public static void saveMatrixRoute(List<Matrix4x4> matrixs, string filepath)
    {
        c2wMaxtrixs Ms = new c2wMaxtrixs();
        foreach(Matrix4x4 m in matrixs)
        {
            c2wMaxtrix c2w = new c2wMaxtrix();
            c2w.elements[0] = m.m00;
            c2w.elements[1] = m.m01;
            c2w.elements[2] = m.m02;
            c2w.elements[3] = m.m03;

            c2w.elements[4] = m.m10;
            c2w.elements[5] = m.m11;
            c2w.elements[6] = m.m12;
            c2w.elements[7] = m.m13;

            c2w.elements[8] = m.m20;
            c2w.elements[9] = m.m21;
            c2w.elements[10] = m.m22;
            c2w.elements[11] = m.m23;

            c2w.elements[12] = m.m30;
            c2w.elements[13] = m.m31;
            c2w.elements[14] = m.m32;
            c2w.elements[15] = m.m33;
            Ms.matrixs.Add(c2w);
        }
        string potion = JsonUtility.ToJson(Ms);
        System.IO.File.WriteAllText(filepath, potion);
    }
}
