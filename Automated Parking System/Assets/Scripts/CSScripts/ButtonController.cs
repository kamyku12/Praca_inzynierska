using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    Camera cam;
    public Transform car;
    CameraController cameraController;
    BlinkerController blinkerController;
    CarController carController;
    ParkingSpotChecker parkingSpotChecker;
    GameObject possibleParkingSpot;
    [SerializeField] public GameObject lookForSpotButton;
    private void Start()
    {
        cam = Camera.main;
        cameraController = cam.GetComponent<CameraController>();
        blinkerController = car.GetComponent<BlinkerController>();
        carController = car.GetComponent<CarController>();
        parkingSpotChecker = car.GetComponent<ParkingSpotChecker>();
    }

    public void ChangeView()
    {
        cameraController.ChangeView();
    }

    public void LeftBlinker()
    {
        blinkerController.LeftBlinker();
        parkingSpotChecker.leftSide = !parkingSpotChecker.leftSide;
    }

    public void RightBlinker()
    {
        blinkerController.RightBlinker();
        parkingSpotChecker.leftSide = false;
    }

    public void ChangeDrivingMode()
    {
        carController.ChangeDrivingMode();
    }

    public void LookForSpots()
    {
        parkingSpotChecker.lookForSpot = !parkingSpotChecker.lookForSpot;
        if(!parkingSpotChecker.lookForSpot)
        {
            lookForSpotButton.GetComponent<Image>().color = Color.red;
            parkingSpotChecker.leftSpotInstantiate = true;
            parkingSpotChecker.rightSpotInstantiate = true;
            GameObject[] allPossibleParkingSpots = GameObject.FindGameObjectsWithTag("possibleParkingSpot");
            foreach(GameObject possibleParkingSpot in allPossibleParkingSpots)
            {
                Destroy(possibleParkingSpot);
            }
        }
        else
        {
            lookForSpotButton.GetComponent<Image>().color = Color.green;
        }
    }
}
