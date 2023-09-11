using System;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    public float motor;
    public float motorFromSelfDrive;
    public float steering;
    public float steeringFromSelfDrive;
    public bool breaking;
    public float brakeStrength;
    public Light[] brakeLights;
    bool selfDriving;
    public bool brake;
    public SelfDriving sd;

    public void FixedUpdate()
    {
        ApplyControl();

        RotateWheels();

        BrakeLights();

        ApplyStep();
    }

    private void ApplyControl()
    {
        if (!selfDriving)
        {
            motor = maxMotorTorque * Input.GetAxis("Vertical");
            steering = maxSteeringAngle * Input.GetAxis("Horizontal");

            brake = Input.GetKey(KeyCode.Space);
        }
        else
        {

            motor = maxMotorTorque * motorFromSelfDrive;
            print(motorFromSelfDrive);
            steering = maxSteeringAngle * steeringFromSelfDrive;

            brake = Convert.ToBoolean(sd.GetBrake());
        }
    }

    private void ApplyStep()
    {
        ApplyStepForMotor();

        ApplyStepForSteering();

    }

    private void ApplyStepForMotor()
    {
        if (sd.GetMotor() == 0)
        {
            motorFromSelfDrive = 0;
        }
        else if (sd.GetMotor() == -1)
        {
            if (motorFromSelfDrive > -1)
            {
                motorFromSelfDrive -= 0.1f * Time.deltaTime;
            }
        }
        else /* motor == 1 */
        {
            if (motorFromSelfDrive < 1)
            {
                motorFromSelfDrive += 0.1f * Time.deltaTime;
            }
        }
    }

    private void ApplyStepForSteering()
    {
        if (sd.GetMotor() == 0)
        {
            steeringFromSelfDrive = 0;
        }
        else if (sd.GetMotor() == -1)
        {
            if (steeringFromSelfDrive > -1)
            {
                steeringFromSelfDrive -= 0.05f * Time.deltaTime;
            }
        }
        else /* motor == 1 */
        {
            if (steeringFromSelfDrive < 1)
            {
                steeringFromSelfDrive += 0.05f * Time.deltaTime;
            }
        }
    }

    private void RotateWheels()
    {
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;

                // If brake == true, set brakeStrength, else set to 0
                axleInfo.leftWheel.brakeTorque = brake ? brakeStrength : 0;
                axleInfo.rightWheel.brakeTorque = brake ? brakeStrength : 0;
            }
            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }

    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
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

    private void BrakeLights()
    {
        foreach (var light in brakeLights)
        {
            light.enabled = brake;
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
