using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;

public class StartTest : MonoBehaviour
{
    public ParkingSpots parkingSpots;
    public ObservationForRL observation;
    public NavigateToParkingSpotCorrectLine correctLine;
    public NavigateToParkingSpotBreaking breaking;
    public NavigateToParkingSpotRealSpeed realSpeed;
    public GameObject startingPoint;
    

    private void Awake()
    {
        Debug.Log("Placing Cars");

        parkingSpots.PlaceCars();

        GameObject targetSpot = parkingSpots.GetRandomSpotOnRight();


        if(targetSpot)
        {
            // Place checkpoint collider in front of targetSpot
            GameObject checkpoint = new GameObject("checkpoint");
            BoxCollider collider = checkpoint.AddComponent<BoxCollider>();
            checkpoint.tag = "checkpoint";
            checkpoint.transform.position = new Vector3(targetSpot.transform.position.x - 1.288f, 0.5f, targetSpot.transform.position.z - 0.155f);
            collider.size = new Vector3(4.17f, 2.38f, 6.95f);
            collider.isTrigger = true;

            // Assign parking spot to scripts
            observation.parkingSpot = targetSpot;
            startingPoint.transform.position = new Vector3(startingPoint.transform.position.x, 
                startingPoint.transform.position.y, 
                targetSpot.transform.position.x);


            if(correctLine)
            {
                correctLine.targetTransform = targetSpot.transform;
                correctLine.parkingSpot = targetSpot;
                GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");

                // Convert the array to a List for easier removal
                List<GameObject> gameObjectsList = new List<GameObject>(cars);

                // Use the Remove method to remove the specified GameObject
                gameObjectsList.Remove(transform.gameObject);

                // Convert the List back to an array
                cars = gameObjectsList.ToArray();

                correctLine.carsObstacles = cars;

                List<Transform> carTransforms = new List<Transform>();
                foreach(GameObject car in cars)
                {
                    carTransforms.Add(car.transform);
                }

                correctLine.savedPositions = carTransforms.ToArray();

            } else if (breaking)
            {
                breaking.targetTransform = targetSpot.transform;
                breaking.parkingSpot = targetSpot;
                GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");

                // Convert the array to a List for easier removal
                List<GameObject> gameObjectsList = new List<GameObject>(cars);

                // Use the Remove method to remove the specified GameObject
                gameObjectsList.Remove(transform.gameObject);

                // Convert the List back to an array
                cars = gameObjectsList.ToArray();

                breaking.carsObstacles = cars;

                List<Transform> carTransforms = new List<Transform>();
                foreach (GameObject car in cars)
                {
                    carTransforms.Add(car.transform);
                }

                breaking.savedPositions = carTransforms.ToArray();
            } else if (realSpeed)
            {
                realSpeed.targetTransform = targetSpot.transform;
                realSpeed.parkingSpot = targetSpot;
                GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");

                // Convert the array to a List for easier removal
                List<GameObject> gameObjectsList = new List<GameObject>(cars);

                // Use the Remove method to remove the specified GameObject
                gameObjectsList.Remove(transform.gameObject);

                // Convert the List back to an array
                cars = gameObjectsList.ToArray();

                realSpeed.carsObstacles = cars;

                List<Transform> carTransforms = new List<Transform>();
                foreach (GameObject car in cars)
                {
                    carTransforms.Add(car.transform);
                }

                realSpeed.savedPositions = carTransforms.ToArray();
            }
        }
    }
}
