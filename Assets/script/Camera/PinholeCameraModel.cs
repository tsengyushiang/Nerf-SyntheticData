using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinholeCameraModel : MonoBehaviour
{
    Camera cam;
    public int width=1280;
    public int height=720;
    public float fx= 612.5118408203125f;
    public float fy= 612.4344482421875f;
    public float cx= 636.660400390625f;
    public float cy= 366.721923828125f;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();

        this.changeCameraParam(width,height,fx,fy,cx,cy);
        cam.targetTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
    }

    public void changeCameraParam(int width, int height, float fx,float fy,float px,float py)
    {       
        // referecne : https://answers.unity.com/questions/814701/how-to-simulate-unity-pinhole-camera-from-its-intr.html

        float f = 50.0f; // f can be arbitrary, as long as sensor_size is resized to to make ax,ay consistient
        float sizeX = f * width / fx;
        float sizeY = f * height / fy;

        float shiftX = -(px - width / 2.0f) / width;
        float shiftY = (py - height / 2.0f) / height;

        cam.sensorSize = new Vector2(sizeX, sizeY);     // in mm, mx = 1000/x, my = 1000/y
        cam.focalLength = f;                            // in mm, ax = f * mx, ay = f * my
        cam.lensShift = new Vector2(shiftX, shiftY);    // W/2,H/w for (0,0), 1.0 shift in full W/H in image plane
    }
}
