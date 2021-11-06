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
    public List<int> color_raw;
    public List<uint> depth_raw;
}

public class RawJsonExporter
{
    public static void savRawJson(PinholeCameraModel m,Color[] rgbaArray,uint[] depthArray,float depthsacle,string filepath)
    {
        //CamerInfo caminfo = new CamerInfo();
        //for(int i = 0; i < m.width; i++)
        //{
        //    for(int j = 0; j < m.height; j++)
        //    {

        //    }
        //}
        //var class = new MyList();
        //var outputString = JsonUtility.ToJson(class);
        //File.WriteAllText("C:\\MyFile.json", outputString);

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
