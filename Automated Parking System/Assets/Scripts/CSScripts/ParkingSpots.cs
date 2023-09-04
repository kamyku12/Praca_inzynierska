using System;
using UnityEngine;
using Random = System.Random;

public class ParkingSpots : MonoBehaviour
{
    public GameObject[] parkingSpots;
    public GameObject car;
    void Awake()
    {
        Random rnd = new Random(Guid.NewGuid().GetHashCode());
        foreach (GameObject spot in parkingSpots)
        {
            // Probability of generating car in given spot is 80%
            if (rnd.NextDouble() <= 0.8)
            {
                // Probability of moving cat to the left is 50%
                int lr = rnd.NextDouble() <= 0.3 ? 1 : -1;

                // Probability of moving car is 20%
                float offset = rnd.NextDouble() <= 0.2 ? (float)(rnd.NextDouble() * 1.5 + 0.5) : 0;
                Instantiate(car, spot.transform.position + new Vector3(offset, 0.14f, 0), Quaternion.identity);
                spot.tag = "taken";
            }
        }
    }
}
