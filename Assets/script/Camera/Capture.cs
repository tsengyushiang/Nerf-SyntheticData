using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Capture : MonoBehaviour
{
    public int FileCounter = 0;
    public void CaptureColor(string names)
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
 
        File.WriteAllBytes(names, Bytes);
        FileCounter++;
    }
}
