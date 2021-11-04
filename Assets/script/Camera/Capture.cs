using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Capture : MonoBehaviour
{    public void CaptureMask(string name)
    {
        Camera cam = GetComponent<Camera>();
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
        Texture2D Image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;

        Texture2D output = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        for (int i = 0; i < cam.targetTexture.width; i++)
        {
            for (int j = 0; j < cam.targetTexture.height; j++)
            {
                if (Image.GetPixel(i, j).a > 0.5f)
                    output.SetPixel(i, j, new Color(1f, 1f, 1f, 1f));
                else
                    output.SetPixel(i, j, new Color(0f, 0f, 0f, 1f));
            }
        }
        Destroy(Image);

        output.Apply();
        RenderTexture.active = currentRT;
        var Bytes = output.EncodeToPNG();

        File.WriteAllBytes(name, Bytes);
    }
    public void CaptureColor(string name)
    {
        Camera cam = GetComponent<Camera>();
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;

        cam.Render();

        Texture2D Image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;

        var Bytes = Image.EncodeToPNG();
        Destroy(Image);

        File.WriteAllBytes(name, Bytes);
    }
    public void CaptureRaw(string name)
    {
        float depthScale = 1e-3f;
        // raycast get depth value
        PinholeCameraModel m = GetComponent<PinholeCameraModel>();
        RaycastDepth depthSensor = GetComponent<RaycastDepth>();
        uint[] depthRaw = depthSensor.rayCastDepthRaw(m.width,m.height,m.cx,m.cy,m.fx,m.fy, 1.0f/depthScale).ToArray();

        // fetch color
        Camera cam = GetComponent<Camera>();
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
        cam.Render();
        Texture2D Image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;
        Color[] colorRaw = Image.GetPixels();

        // save json file
        RawJsonExporter.savRawJson(m, colorRaw, depthRaw, depthScale, name);
    }
}
