using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SteeringWheelController : MonoBehaviour
{
    CarController carController;
    Transform wheel;
    float maxAngle;
    void Start()
    {
        carController = GetComponentInParent<CarController>();
        maxAngle = carController.maxSteeringAngle;
        foreach (AxleInfo ainfo in carController.axleInfos)
        {
            if (ainfo.steering)
                wheel = ainfo.rightWheel.gameObject.transform.GetChild(0);
        }
    }

    void Update()
    {
        transform.eulerAngles = new Vector3(transform.parent.rotation.eulerAngles.x, transform.parent.eulerAngles.y, -(Input.GetAxis("Horizontal") * 180));
    }
}
