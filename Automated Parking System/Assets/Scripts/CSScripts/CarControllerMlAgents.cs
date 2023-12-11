using System;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerMlAgents : MonoBehaviour
{
    // values used in controlling car movement
    public float motor;
    public float steering;
    public bool braking;
    public float inputMotorWithStep;
    public float inputSteeringWithStep;
    public int inputMotor;
    public int inputSteering;
    public bool inputBraking;
    // -------------------------------------

    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    public float brakeStrength;
    public Light[] brakeLights;
    bool selfDriving;

    private void Start()
    {
        inputMotor = 0;
        inputSteering = 0;
        inputMotorWithStep = 0.0f;
        inputSteeringWithStep = 0.0f;
    }

    public void FixedUpdate()
    {
        ApplyControl();

        ApplyStep(inputMotor, ref inputMotorWithStep);
        ApplyStep(inputSteering, ref inputSteeringWithStep);

        RotateWheels(steering, motor, braking);

        BrakeLights();
    }

    private void ApplyStep(int receivedValue, ref float inputValue)
    {
        // Apply step
        if (receivedValue == 0)
        {
            inputValue = 0;
        }
        else if (receivedValue < 0)
        {
            if (inputValue > receivedValue)
            {
                inputValue -= 0.5f * Time.deltaTime;
            }
        }
        else /* receivedValue > 0 */
        {
            if (inputValue < receivedValue)
            {
                inputValue += 0.5f * Time.deltaTime;
            }
        }
    }

    private void ApplyControl()
    {
        motor = maxMotorTorque * inputMotorWithStep;
        steering = maxSteeringAngle * inputSteeringWithStep;
        braking = inputBraking;
    }

    private void RotateWheels(float steering, float motor, bool brake)
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
            light.enabled = braking;
        }
    }

    public void ChangeDrivingMode()
    {
        selfDriving = !selfDriving;
    }

    public void ResetValues()
    {
        motor = 0.0f;
        steering = 0.0f;
        braking = false;
        inputMotor = 0;
        inputSteering = 0;
        RotateWheels(0, 0, false);
        transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
}
