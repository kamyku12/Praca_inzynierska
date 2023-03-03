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
        Random rnd = new Random();
        foreach (Transform spot in parkingSpots)
        {
            if(rnd.NextDouble() <= 0.6)
                Instantiate(car, spot.position + new Vector3((float)rnd.NextDouble(), 0.14f, 0), Quaternion.identity);
        }
    }
}
