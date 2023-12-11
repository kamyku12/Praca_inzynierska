using System;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    // values used in controlling car movement
    public float motor;
    public float steering;
    public bool brake;
    public float inputMotorFromSelfDrive;
    public float inputSteeringFromSelfDrive;
    // -------------------------------------

    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    public float brakeStrength;
    public Light[] brakeLights;
    bool selfDriving;
    public bool braking;
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

            motor = maxMotorTorque * inputMotorFromSelfDrive;
            steering = maxSteeringAngle * inputSteeringFromSelfDrive;

            brake = Convert.ToBoolean(sd.GetBrake());
        }
    }

    /* Apply step has to imitate steps inside Input.GetAxis where values are
        from range <-1; 1> instead just 1, 0, -1
     */
    private void ApplyStep()
    {
        ApplyStepForMotor();

        ApplyStepForSteering();

    }

    private void ApplyStepForMotor()
    {
        float receivedMotor = sd.GetMotor();
        // Apply step for motor of self driving
        if (receivedMotor == 0)
        {
            inputMotorFromSelfDrive = 0;
        }
        else if (receivedMotor < 0)
        {
            if (inputMotorFromSelfDrive > receivedMotor)
            {
                inputMotorFromSelfDrive -= 0.5f * Time.deltaTime;
            }
        }
        else /* motor > 0 */
        {
            if (inputMotorFromSelfDrive < receivedMotor)
            {
                inputMotorFromSelfDrive += 0.5f * Time.deltaTime;
            }
        }
    }

    private void ApplyStepForSteering()
    {
        float receivedSteering = sd.GetSteering();
        // Apply step for steering of self driving
        if (receivedSteering == 0)
        {
            inputSteeringFromSelfDrive = 0;
        }
        else if (receivedSteering < 0)
        {
            if (inputSteeringFromSelfDrive > receivedSteering)
            {
                inputSteeringFromSelfDrive -= 0.5f * Time.deltaTime;
            }
        }
        else /* steering > 0 */
        {
            if (inputSteeringFromSelfDrive < receivedSteering)
            {
                inputSteeringFromSelfDrive += 0.5f * Time.deltaTime;
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

    public void ResetValues()
    {
        motor = 0.0f;
        steering = 0.0f;
        brake = false;
        inputMotorFromSelfDrive = 0.0f;
        inputSteeringFromSelfDrive = 0.0f;
    }
}
