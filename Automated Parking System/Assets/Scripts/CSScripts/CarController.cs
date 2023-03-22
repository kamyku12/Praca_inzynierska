using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    public float motor;
    public float steering;
    public float brakeStrength;
    bool selfDriving;
    bool brake;
    SelfDriving sd;


    public void Awake()
    {
        brake = false;
        selfDriving = false;
        sd = GetComponent<SelfDriving>();
    }

    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if(collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    public void FixedUpdate()
    {
        Debug.Log(brake);
        if (!selfDriving)
        {
            motor = maxMotorTorque * Input.GetAxis("Vertical");
            steering = maxSteeringAngle * Input.GetAxis("Horizontal");
            brake = Input.GetKey(KeyCode.Space);
        }
        else
        {
            motor = maxMotorTorque * sd.GetMotor();
            steering = maxSteeringAngle * sd.GetSteering();
        }

        foreach(AxleInfo axleInfo in axleInfos)
        {
            if(axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if(axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
                if (brake)
                {
                    axleInfo.leftWheel.brakeTorque = brakeStrength;
                    axleInfo.rightWheel.brakeTorque = brakeStrength;
                }
            }
            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }

    public void ChangeDrivingMode()
    {
        selfDriving = !selfDriving;
    }
}

[System.Serializable]
public class AxleInfo 
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}
