using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObservationForRL : MonoBehaviour
{
    // Observation properties 

    // Velocity of a car
    public Vector3 velocity;
    // Degree of rotation in relation to the forward axis of parking spot
    public float rotation;
    // Is a car inside parking spot
    public bool isCarInsideSpot;
    // Corners of required gameObjects
    Vector3[] spotCorners;
    Vector3[] carCorners;

    // Parking spot dimensions
    public float psWidth = 3.1f;
    public float psLength = 6.0f;

    // -----------------------

    // Array of distance values
    // 0 - left  upper  corner
    // 1 - right upper  corner
    // 2 - left  bottom corner
    // 3 - right bottom corner
    public float[] distance;

    // -----------------------

    // Objects to read value from

    // Rigidbody of a car
    public Rigidbody rb;
    // Game object of parking spot, used to calculate distance
    public GameObject parkinSpot;
    // Game object of car, used to calculate distance
    public GameObject carBody;
    public LearningArtificialBrain learning;

    // --------------------------

    // Epislons for observations to make them 0 if it is realy small number

    public float rotationEpsilon = 0.003f;
    public float velocityEpsilon = 0.003f;

    // --------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        velocity = Vector3.zero;
        rotation = 0f;
        distance = CalculateDistance();
    }

    // Update is called once per frame
    void Update()
    {
        velocity = new Vector3(ApplyEpsilon(rb.velocity.x, velocityEpsilon), ApplyEpsilon(rb.velocity.y, velocityEpsilon), ApplyEpsilon(rb.velocity.z, velocityEpsilon));
        rotation = CalculateRotationDifference();
        distance = CalculateDistance();
        isCarInsideSpot = parkinSpot.tag == "taken";

        DrawLines();
    }

    public float CalculateRotationDifference()
    {
        float zRotationCar = transform.rotation.eulerAngles.z;
        float zRotationSpot = parkinSpot.transform.rotation.eulerAngles.z;

        float rotationDifference = zRotationSpot - zRotationCar;

        // Ensure rotation is in range of -180 and 180

        if (rotationDifference > 180f)
        {
            rotationDifference -= 360f;
        }
        else if (rotationDifference < -180f)
        {
            rotationDifference += 360f;
        }

        rotationDifference = ApplyEpsilon(rotationDifference, rotationEpsilon);

        return rotationDifference;
    }

    public float[] CalculateDistance()
    {
        float[] newDistance = new float[4];
        Bounds spotBounds = parkinSpot.GetComponent<Renderer>().bounds;
        spotCorners = AssignCorners(spotBounds, true);

        Bounds carBounds = carBody.GetComponent<MeshCollider>().bounds;
        carCorners = AssignCorners(carBounds, false);

        for(int i = 0; i < newDistance.Length; i++)
        {
            newDistance[i] = Vector3.Distance(carCorners[i], spotCorners[i]);
        }

        
        return newDistance;
    }


    // Function to set value to zero if it is in range <-epsilon; epsilon>
    public float ApplyEpsilon(float value, float epsilon)
    {
        float newValue = value;
        if ((newValue > 0f && newValue <= epsilon) || (newValue < 0f && newValue >= -epsilon))
        {
            newValue = 0;
        }

        return newValue;
    }

    // Method to assign right/left upper/bottom corner of bounds to array
    public Vector3[] AssignCorners(Bounds bounds, bool spot)
    {
        Vector3[] array = new Vector3[4];
        array[0] = new Vector3(bounds.min.x - (spot ? psLength / 2 : 0), bounds.min.y, bounds.max.z + (spot ? psWidth / 2 : 0));
        array[1] = new Vector3(bounds.max.x + (spot ? psLength / 2 : 0), bounds.min.y, bounds.max.z + (spot ? psWidth / 2 : 0));
        array[2] = new Vector3(bounds.min.x - (spot ? psLength / 2 : 0), bounds.min.y, bounds.min.z - (spot ? psWidth / 2 : 0));
        array[3] = new Vector3(bounds.max.x + (spot ? psLength / 2 : 0), bounds.min.y, bounds.min.z - (spot ? psWidth / 2 : 0));

        return array;
    }

    private void DrawLines()
    {
        for (int i = 0; i < carCorners.Length; i++)
        {
            Debug.DrawLine(carCorners[i], spotCorners[i]);
        }
    }

    public string GetObservations()
    {
        string observations = $"{velocity.x}:{velocity.y}:{velocity.z}|{rotation}|{isCarInsideSpot}|{learning.GetTimer()}|";
        for(int i = 0; i < distance.Length; i++)
        {
            observations = $"{observations}{distance[i]}";
            if (i < distance.Length - 1)
            {
                observations = $"{observations}:";
            }
        }

        // print(observations);
        return observations;
    }
}
