using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    bool birdView;
    public Transform car, birdCamera, povCamera;

    void Start()
    {
        birdView = true;
        birdCamera = car.GetChild(0);
        povCamera = car.GetChild(1);
    }

    void Update()
    {
        if (birdView)
        {
            // Ustaw pozycjê i rotacje kamery na punkt w prefabie oraz rotacje na rotacje auta na osi y i pochyl j¹ o 45 stopni do przodu
            transform.SetPositionAndRotation(birdCamera.position, Quaternion.Euler(45, car.rotation.eulerAngles.y, 0));
        }
        else
        {
            // Ustaw pozycjê i rotacje kamery na punkt w prefabie oraz rotacje na rotacje auta na osi y
            transform.SetPositionAndRotation(povCamera.position, Quaternion.Euler(0, car.rotation.eulerAngles.y, 0));
        }
    }

    public void ChangeView()
    {
        birdView = !birdView;
    }
}
