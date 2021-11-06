using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

public class CamManager : MonoBehaviour
{
    public GameObject prefab;
    public int count = 4;
    public bool isLoop = false;
    public float radian = Mathf.PI;
    public bool camerVertical = false;
    // unit : cm
    public float camHeight= 100;
    public float radius = 150;
    public List<GameObject> cameras;
    void Start()
    {
        isLoop = radian > Mathf.PI ? true : false;
        cameras = new List<GameObject>();
        for(int i = 0; i < (count+(isLoop ? 0:1)); i++)
        {
            float percentag = (float)i / (float)count;
            float interpoRadian = radian * percentag;
            GameObject cam = Instantiate(prefab, new Vector3(radius * Mathf.Cos(interpoRadian), camHeight, radius*Mathf.Sin(interpoRadian)), Quaternion.identity);
            cam.transform.parent = transform;
            cam.transform.LookAt(new Vector3(0, camHeight, 0), Vector3.up);
            if(camerVertical)
                cam.transform.Rotate(Vector3.forward, 90);
            cameras.Add(cam);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
