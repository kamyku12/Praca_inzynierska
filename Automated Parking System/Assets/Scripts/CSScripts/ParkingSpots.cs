using System;
using UnityEngine;
using Random = System.Random;

public class ParkingSpots : MonoBehaviour
{
    public GameObject[] parkingSpots;
    public GameObject car;

    public float carInSpotProbability;
    public float moveCarToLeftProbability;
    public float moveCarProbability;
    void Awake()
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
                float offset = rnd.NextDouble() <= moveCarProbability ? (float)(rnd.NextDouble() * 1.5 + 0.5) : 0;
                Instantiate(car, spot.transform.position + new Vector3(offset * lr, 0.14f, 0), Quaternion.identity);
                spot.tag = "taken";
            }
        }
    }
}
