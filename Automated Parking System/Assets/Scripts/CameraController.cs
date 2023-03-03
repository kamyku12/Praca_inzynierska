using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    bool birdView;
    public Transform car;

    void Start()
    {
        birdView = true;
    }

    void Update()
    {
        if (birdView)
        {
            transform.SetPositionAndRotation(car.position + new Vector3(0, 7, -12), Quaternion.Euler(30, 0, 0));
        }
        else
        {
            
            transform.position = car.position + new Vector3(-0.3f, 1.1f, 0.1f);
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.RotateAround(car.position, new Vector3(0, 1, 0), car.rotation.eulerAngles.y);
        }
    }

    public void ChangeView()
    {
        birdView = !birdView;
    }
}
