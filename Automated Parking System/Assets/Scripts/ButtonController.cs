using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    Camera cam;
    private void Start()
    {
        cam = Camera.main;
    }

    public void ChangeView()
    {
        cam.GetComponent<CameraController>().ChangeView();
    }
}
