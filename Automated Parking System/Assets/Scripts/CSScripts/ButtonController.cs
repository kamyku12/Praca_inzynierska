using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    Camera cam;
    public Transform car;
    CameraController cameraController;
    BlinkerController blinkerController;
    private void Start()
    {
        cam = Camera.main;
        cameraController = cam.GetComponent<CameraController>();
        blinkerController = car.GetComponent<BlinkerController>();
    }

    public void ChangeView()
    {
        cameraController.ChangeView();
    }

    public void LeftBlinker()
    {
        blinkerController.LeftBlinker();
    }

    public void RightBlinker()
    {
        blinkerController.RightBlinker();
    }
}
