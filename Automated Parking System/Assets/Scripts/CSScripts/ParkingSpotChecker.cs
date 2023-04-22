using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class ParkingSpotChecker : MonoBehaviour
{
    [SerializeField] public GameObject[] rightSensors;
    [SerializeField] public GameObject[] leftSensors;
    [SerializeField] public Material[] colors;
    [SerializeField] public GameObject[] parkingSpots;
    [SerializeField] public GameObject frontSensor;
    [SerializeField] public GameObject backSensor;
    [SerializeField] public Light leftBlinker;
    [SerializeField] public GameObject posibleParkingSpot;
    Vector3 carParameters, placeToSetPossibleParkingSpot;
    Ray rayToBack, rayToFront;
    RaycastHit hitToBack, hitToFront, hitForParkingSpot;
    public bool rightSpotInstantiate, leftSpotInstantiate, lookForSpot, leftSide, parkingSpot;
    void Start()
    {
        parkingSpot = false;
        placeToSetPossibleParkingSpot = Vector3.zero;
        rightSpotInstantiate = leftSpotInstantiate = true;
        lookForSpot = leftSide = false;
        carParameters = transform.GetChild(3).GetComponent<MeshFilter>().mesh.bounds.extents;
    }

    // Update is called once per frame
    void Update()
    {
        if (lookForSpot)
        {
            if (leftSide)
            {
                LeftSide();
            }
            else
            {
                rightSide();
            }
        }
    }

    void LeftSide()
    {
        Vector3 center = leftSensors[1].transform.position;
        Vector3 frontCorner = leftSensors[0].transform.position;
        Vector3 backCorner = leftSensors[0].transform.position;

        CastRaysForSensors(center, frontCorner, backCorner);

        bool canFit = hitToBack.distance + hitToFront.distance >= carParameters.x + 0.3;
        bool isLongEnough = leftSensors[1].GetComponent<MeshRenderer>().material == colors[2] || !leftSensors[1].GetComponent<MeshRenderer>().enabled;
        if (canFit && isLongEnough && leftSpotInstantiate)
        {
            CastRaysForSpots(center, Vector3.back);

            CheckParkingSpots();

            PlacePossibleParkingSpot(ref leftSpotInstantiate);
        }
    }

    void rightSide()
    {
        Vector3 center = rightSensors[1].transform.position;
        Vector3 frontCorner = rightSensors[0].transform.position;
        Vector3 backCorner = rightSensors[0].transform.position;

        CastRaysForSensors(center, frontCorner, backCorner);

        bool canFit = hitToBack.distance + hitToFront.distance >= carParameters.x + 0.3;
        bool isLongEnough = rightSensors[1].GetComponent<MeshRenderer>().material == colors[2] || !rightSensors[1].GetComponent<MeshRenderer>().enabled;
        if (canFit && isLongEnough && rightSpotInstantiate)
        {
            CastRaysForSpots(center, Vector3.forward);

            CheckParkingSpots();

            PlacePossibleParkingSpot(ref rightSpotInstantiate);
        }
    }

    private void CastRaysForSensors(Vector3 center, Vector3 frontCorner, Vector3 backCorner)
    {
        Vector3 directionToFrontCorner = frontCorner - center;
        Vector3 directionToBackCorner = backCorner - center;

        rayToFront = new Ray(center, directionToFrontCorner);
        rayToBack = new Ray(center, directionToBackCorner);
        Physics.Raycast(rayToBack, out hitToBack);
        Physics.Raycast(rayToFront, out hitToFront);

        Debug.DrawRay(center, directionToFrontCorner);
        Debug.DrawRay(center, directionToBackCorner);
    }

    private void CastRaysForSpots(Vector3 center, Vector3 dir)
    {
        Quaternion rotation = Quaternion.AngleAxis(transform.rotation.eulerAngles.y - 90.0f, Vector3.up);
        Ray rayForSpot = new Ray(center, center - (center + rotation * (dir * 5)));
        Physics.Raycast(rayForSpot, out hitForParkingSpot, Mathf.Infinity, LayerMask.GetMask("ParkingSpot"));
        Debug.DrawRay(center, center - (center + rotation * (dir * 5)));
    }

    private void CheckParkingSpots()
    {
        foreach (GameObject pSpot in parkingSpots)
        {
            if (hitForParkingSpot.collider == pSpot.GetComponent<BoxCollider>() && pSpot.tag == "notTaken")
            {
                float distanceToSpot = (transform.position - pSpot.transform.position).magnitude;
                if (distanceToSpot <= 6.0f)
                {
                    parkingSpot = true;
                    placeToSetPossibleParkingSpot = pSpot.transform.position;
                }
            }
        }
    }

    private void PlacePossibleParkingSpot(ref bool side)
    {
        if (parkingSpot)
        {
            Instantiate(posibleParkingSpot, placeToSetPossibleParkingSpot, Quaternion.Euler(0, -90, 0));
            side = false;
            parkingSpot = false;
        }
    }
}
