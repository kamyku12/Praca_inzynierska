using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class ParkingSpots : MonoBehaviour
{
    public Transform[] parkingSpots;
    public GameObject car;
    void Awake()
    {
        Random rnd = new Random(Guid.NewGuid().GetHashCode());
        foreach (Transform spot in parkingSpots)
        {
            // Prawdopodobieñstwo ustawienia auta na danym miejscu parkingowym wynosi 80%
            if (rnd.NextDouble() <= 0.8)
            {
                // Prawdopodobieñstwo przesuniêcia w lewo czy w prawo wynosi 50%
                int lr = rnd.NextDouble() <= 0.3 ? 1 : -1;

                // Prawdopodobieñstwo przesuniêcia siê auta wynosi 20%
                float offset = rnd.NextDouble() <= 0.2 ? (float)(rnd.NextDouble() * 1.5 + 0.5) : 0;
                Instantiate(car, spot.position + new Vector3(offset, 0.14f, 0), Quaternion.identity);
            }
        }
    }
}
