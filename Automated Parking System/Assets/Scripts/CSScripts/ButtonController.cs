using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public CameraController cameraController;
    public BlinkerController blinkerController;
    public CarController carController;
    public ParkingSpotChecker parkingSpotChecker;
    public SelfDriving selfDriving;
    public LearningArtificialBrain learning;
    [SerializeField] private GameObject lookForSpotButton;

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
        selfDriving.StartStop();
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

    public void ToggleLearning()
    {
        ChangeDrivingMode();
        learning.ToggleLearning();
    }
}
