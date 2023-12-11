using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObservationForRL : MonoBehaviour
{
    // Observation properties 

    // Velocity of a car
    public Vector3 velocity;
    public Transform carFrontPoint;
    public Transform carBackPoint;
    // Degree of rotation in relation to the forward axis of parking spot
    public float rotation;
    public int[] sensorDistances;
    // Is a car inside parking spot
    public bool isCarInsideSpot;
    // Corners of required gameObjects
    Vector3[] spotPoints;
    Vector3 carParameters;

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
    public GameObject parkingSpot;
    // Game object of car, used to calculate distance
    public GameObject carBody;
    public GameObject[] sensors;
    public List<Material> materials;
    public LearningArtificialBrain learning;

    // --------------------------

    // Start is called before the first frame update
    void Start()
    {
        velocity = Vector3.zero;
        rotation = 0f;
        distance = CalculateDistance();
        sensorDistances = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
    }

    // Update is called once per frame
    void Update()
    {
        carParameters = transform.GetChild(3).GetComponent<MeshFilter>().mesh.bounds.extents; ;
        velocity = rb.velocity;
        rotation = CalculateRotationDifference();
        distance = CalculateDistance();
        sensorDistances = GetSensorDistancesValues();
        isCarInsideSpot = parkingSpot.tag == "taken";

        DrawLines();
    }

    public float CalculateRotationDifference()
    {
        float yRotationCar = transform.rotation.eulerAngles.y;
        float yRotationSpot = parkingSpot.transform.rotation.eulerAngles.y;

        float rotationDifference = yRotationSpot - yRotationCar + 90.0f;

        // Ensure rotation is in range of -180 and 180

        if (rotationDifference > 180f)
        {
            rotationDifference -= 360f;
        }
        else if (rotationDifference < -180f)
        {
            rotationDifference += 360f;
        }

        return rotationDifference;
    }

    public float[] CalculateDistance()
    {
        float[] newDistance = new float[2];

        Vector3 spotPosition = parkingSpot.transform.position;
        spotPoints = new Vector3[]{ new Vector3(spotPosition.x + carParameters.z, 0, spotPosition.z),
                                new Vector3(spotPosition.x - carParameters.z, 0, spotPosition.z) };

        // Front point of the parking spot

        Vector3[] carPoints = new Vector3[] { new Vector3(carFrontPoint.position.x, 0, carFrontPoint.position.z), 
                                new Vector3(carBackPoint.position.x, 0, carBackPoint.position.z) };
        newDistance[0] = Vector3.Distance(carPoints[0], spotPoints[0]);
        newDistance[1] = Vector3.Distance(carPoints[1], spotPoints[1]);

        return newDistance;
    }

    private void DrawLines()
    {
        Vector3[] carPoints = new Vector3[]{carFrontPoint.position, carBackPoint.position};
        for (int i = 0; i < spotPoints.Length; i++)
        {
            Debug.DrawLine(carPoints[i], spotPoints[i]);
        }
    }

    public string GetObservations()
    {
        string observations = $"{velocity.x}:{velocity.y}:{velocity.z}|{rotation}|{isCarInsideSpot}|{learning.GetTimer()}|";
        for (int i = 0; i < distance.Length; i++)
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

    private int[] GetSensorDistancesValues()
    {
        int[] newValues = new int[sensors.Length];

        for(int index = 0; index < sensors.Length; index += 1)
        {
            MeshRenderer sensorRenderer = sensors[index].GetComponent<MeshRenderer>();
            newValues[index] = sensorRenderer.enabled ? materials.IndexOf(sensorRenderer.material) + 1 : 0;
        }

        return newValues;
    }
}
