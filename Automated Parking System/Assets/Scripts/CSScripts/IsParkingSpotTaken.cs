using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsParkingSpotTaken : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "CarBody")
        {
            transform.parent.gameObject.tag = "taken";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "CarBody")
        {
            transform.parent.gameObject.tag = "notTaken";
        }
    }
}
