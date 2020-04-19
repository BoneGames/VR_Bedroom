using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealWorldCam : MonoBehaviour
{
    public Renderer moniter;
    public bool selfieCam;
    // Start is called before the first frame update
    private WebCamTexture backCamera;
    int screenWidth, screenHeight;
    // Use this for initialization
    void Start()
    {
        screenWidth = (int)transform.localScale.x * 1000;
        screenHeight = (int)transform.localScale.z * 1000;
        moniter = GetComponent<Renderer>();
        backCamera = GetCamTex(selfieCam);
        moniter.material.mainTexture = backCamera;
        backCamera.Play();
    }

    WebCamTexture GetCamTex(bool selfieCam)
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.Log("No Camera Detected");

            return null;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if (!devices[i].isFrontFacing && selfieCam)
            {
                return new WebCamTexture(devices[i].name, screenWidth, screenHeight);
            }
            if (devices[i].isFrontFacing && !selfieCam)
            {
                return new WebCamTexture(devices[i].name, screenWidth, screenHeight);
            }
        }

        Debug.Log("could not find desired cam");
        return null; ;
    }
}
