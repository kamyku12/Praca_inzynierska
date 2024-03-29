using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Linq;

public class NavigateToParkingSpotCorrectLine : Agent
{
    [SerializeField] public Transform targetTransform;
    [SerializeField] private CarControllerMlAgents carController;
    [SerializeField] private Transform startingSpot;
    [SerializeField] private ObservationForRL observations;
    [SerializeField] public GameObject parkingSpot;
    [SerializeField] public GameObject[] carsObstacles;
    [SerializeField] public Transform[] savedPositions;


    public float xMinRange;
    public float xMaxRange;
    public float zMinRange;
    public float zMaxRange;
    public float randomRotationRange;
    public float allowableRotationError;
    public float allowableDistanceError;

    // For tests
    public bool start;
    public bool stop;
    public bool hit;
    public bool correct;

    public int correctCount = 0;
    public int hitCount = 0;
    public int timeoutCount = 0;
    public int episode = 0;

    public override void OnEpisodeBegin()
    {
        episode++;
        if(correct)
        {
            correctCount++;
            correct = false;
        } else if(hit)
        {
            hitCount++;
            hit = false;
        } else
        {
            timeoutCount++;
        }

        if(episode >= 1000)
        {
            Debug.Log($"Correct: {correctCount}\nHit: {hitCount}\nTimeout: {timeoutCount}");
            UnityEditor.EditorApplication.isPlaying = false;
        }
        

        carController.ResetValues();
        transform.position = startingSpot.position + new Vector3(Random.Range(xMinRange, xMaxRange), 0, Random.Range(zMinRange, zMaxRange));
        transform.rotation = Quaternion.Euler(0, Random.Range(-randomRotationRange, randomRotationRange), 0);
        for (int index = 0; index < carsObstacles.Length; index += 1)
        {
            carsObstacles[index].transform.position = savedPositions[index].position;
            carsObstacles[index].transform.rotation = savedPositions[index].rotation;
            carsObstacles[index].GetComponent<Rigidbody>().velocity = Vector3.zero;
            carsObstacles[index].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int motor = actions.DiscreteActions[0];
        int steering = actions.DiscreteActions[1];

        carController.inputMotor = motor - 1;
        carController.inputSteering = steering - 1;

        // Add reward after every action taken
        AddReward(-1f / MaxStep);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);

        sensor.AddObservation(targetTransform.position);

        sensor.AddObservation(observations.velocity);

        sensor.AddObservation(observations.rotation);

        foreach(float distance in observations.distance)
        {
            sensor.AddObservation(distance);
        }

        foreach(float distance in observations.sensorDistances)
        {
            sensor.AddObservation(distance);
        }

        sensor.AddObservation(observations.middleParkingSpot);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "parkingSpot" &&
           other.transform.parent.gameObject.tag == "taken" &&
           other.transform.parent.gameObject.Equals(parkingSpot))
        {
            if(observations.rotation > allowableRotationError || observations.rotation < -allowableRotationError)
            {
                Debug.Log("Parked incorrectly");
                AddReward(1.5f);
            }
            else
            {
                // If summed distances between car front and back point to front and back point of parking spot
                // Is less than allowableDistanceError
                if (observations.distance[0] + observations.distance[1] <= allowableDistanceError)
                {
                    Debug.Log("Parked correctly");
                    AddReward(5f);
                    EndEpisode();
                    correct = true;
                } else
                {
                    Debug.Log("Not deep enough");
                    AddReward(3.5f);
                }

            }
        } else if(other.tag == "checkpoint")
        {
            Debug.Log("You are closer to parking spot");
            AddReward(0.5f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "wall" || collision.gameObject.tag == "Car")
        {
            Debug.Log($"HITTED {collision.gameObject.tag.ToUpper()}");
            AddReward(-6f);
            EndEpisode();
            hit = true;
        }
    }
}
