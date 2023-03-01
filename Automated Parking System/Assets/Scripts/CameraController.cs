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
            transform.position = car.position + new Vector3(0, 7, -12);
            transform.rotation = Quaternion.Euler(30, 0, 0);
        }
        else
        {
            transform.position = car.position + new Vector3(0, 1.2f, 0);
            transform.rotation = car.rotation;
        }
    }

    public void ChangeView()
    {
        birdView = !birdView;
    }
}
