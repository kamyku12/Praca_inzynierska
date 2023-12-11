using UnityEngine;

public class SteeringWheelControllerMlAgents : MonoBehaviour
{
    CarControllerMlAgents carController;
    Transform wheel;
    float maxAngle;
    void Start()
    {
        carController = GetComponentInParent<CarControllerMlAgents>();
        maxAngle = carController.maxSteeringAngle;
        foreach (AxleInfo ainfo in carController.axleInfos)
        {
            if (ainfo.steering) 
            {
                wheel = ainfo.rightWheel.gameObject.transform.GetChild(0);
                break;
            }
        }
    }

    void Update()
    {
        transform.eulerAngles = new Vector3(transform.parent.rotation.eulerAngles.x, transform.parent.eulerAngles.y, -(carController.steering / maxAngle * 180));
    }
}
