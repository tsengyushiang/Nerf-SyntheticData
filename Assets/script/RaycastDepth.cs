using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastDepth : MonoBehaviour
{
    public List<uint> rayCastDepthRaw(int width,int height,float cx,float cy,float fx,float fy,float depthscale)
    {
        Quaternion q = transform.rotation;

        List<uint> raycastDepth = new List<uint>();
        for (int j = 0; j <height ; j++)
        {
            for(int i = 0; i < width; i++)
            {
                Vector3 pixelDir = new Vector3(((float)i - cx) / fx, ((float)(height-1-j) - cy) / fy, 1.0f);
                Vector3 worldDir = q * pixelDir.normalized;
                RaycastHit hit;
                if (Physics.Raycast(transform.position, worldDir, out hit))
                {
                    raycastDepth.Add((uint)(hit.distance * depthscale));
                    Debug.DrawLine(transform.position, transform.position + hit.distance* worldDir, Color.white, 1000);
                }
                else
                {
                    raycastDepth.Add(0);
                }
            }
        }

        return raycastDepth;
    }
}
