using Unity.VisualScripting;
using UnityEngine;

public class ParkingSpotChecker : MonoBehaviour
{
    [SerializeField] public GameObject[] rightSensors;
    [SerializeField] public GameObject[] leftSensors;
    [SerializeField] public GameObject frontSensor;
    [SerializeField] public GameObject backSensor;
    [SerializeField] public Material[] colors;
    [SerializeField] public Light leftBlinker;
    [SerializeField] public GameObject posibleParkingSpot;
    Vector3 carParameters;
    Ray rayToBack, rayToFront;
    RaycastHit hitToBack, hitToFront;
    void Start()
    {
        carParameters = transform.GetChild(3).GetComponent<MeshFilter>().mesh.bounds.extents;
    }

    // Update is called once per frame
    void Update()
    {
        if (leftBlinker.enabled)
        {
            rayToBack = new Ray(leftSensors[1].transform.position, leftSensors[2].transform.position);
            rayToFront = new Ray(leftSensors[1].transform.position, leftSensors[0].transform.position);
            Physics.Raycast(rayToBack, out hitToBack);
            Physics.Raycast(rayToFront, out hitToFront);
            bool canFit = hitToBack.distance + hitToFront.distance >= carParameters.x + 0.3;
            bool isLongEnough = leftSensors[1].GetComponent<MeshRenderer>().material == colors[2] || !leftSensors[1].GetComponent<MeshRenderer>().enabled;
            if ( canFit && isLongEnough)
            {
                Instantiate(posibleParkingSpot, leftSensors[1].transform);
            }
        }
        else
        {
            
            rayToBack = new Ray(transform.TransformPoint(rightSensors[1].transform.position), transform.TransformPoint(rightSensors[2].transform.position));
            rayToFront = new Ray(transform.TransformPoint(rightSensors[1].transform.position), transform.TransformPoint(rightSensors[0].transform.position));
            Physics.Raycast(rayToBack, out hitToBack);
            Debug.DrawRay(transform.TransformPoint(rightSensors[1].transform.position), transform.TransformPoint(rightSensors[2].transform.position));
            Debug.DrawRay(transform.TransformPoint(rightSensors[1].transform.position), transform.TransformPoint(rightSensors[0].transform.position));
            Physics.Raycast(rayToFront, out hitToFront);
            bool canFit = hitToBack.distance + hitToFront.distance >= carParameters.x + 0.3;
            bool isLongEnough = rightSensors[1].GetComponent<MeshRenderer>().material == colors[2] || !rightSensors[1].GetComponent<MeshRenderer>().enabled;
            if (canFit && isLongEnough)
            {
                Instantiate(posibleParkingSpot, rightSensors[1].transform);
            }
        }
    }
}
