using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class ParkingSpots : MonoBehaviour
{
    public GameObject[] parkingSpots;
    public GameObject car;

    public float carInSpotProbability;
    public float moveCarToLeftProbability;
    public float moveCarProbability;

    public void PlaceCars()
    {
        Random rnd = new Random(Guid.NewGuid().GetHashCode());
        foreach (GameObject spot in parkingSpots)
        {
            // Prawdopodobieństwo auta w miejscu parkingowym
            if (rnd.NextDouble() <= carInSpotProbability)
            {
                // Prawdopodobieństwo przesunięcia auta w lewo
                int lr = rnd.NextDouble() <= moveCarToLeftProbability ? -1 : 1;

                // Prawdopodobieństwo przesunięcia auta
                float offset = rnd.NextDouble() <= moveCarProbability ? (float)(rnd.NextDouble() * 1 + 0.25) : 0;
                Instantiate(car, spot.transform.position + new Vector3(offset * lr, 0.14f, 0), Quaternion.Euler(0, 90f, 0));
                spot.tag = "taken";
            }
        }
    }

    public GameObject GetRandomSpotOnRight()
    {
        List<GameObject> notTakenParkingSpotsOnRight = new List<GameObject>();
        foreach(GameObject spot in parkingSpots)
        {
            if(spot.tag == "notTaken" && spot.transform.localPosition.z > 0)
            {
                notTakenParkingSpotsOnRight.Add(spot);
            }
        }

        if(notTakenParkingSpotsOnRight.Count == 0)
        {
            return null;
        }

        // Get a random index
        int randomIndex = new Random().Next(notTakenParkingSpotsOnRight.Count);

        return notTakenParkingSpotsOnRight[randomIndex];
    }

}
