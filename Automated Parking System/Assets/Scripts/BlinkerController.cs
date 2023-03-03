using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlinkerController : MonoBehaviour
{
    public List<Light> leftBlinkers;
    public List<Light> rightBlinkers;
    bool right, left, on;
    float counterLeft, counterRight;

    // Start is called before the first frame update
    void Start()
    {
        right = left = false;
        on = true;
        foreach(Light light in leftBlinkers)
            light.enabled = left;

        foreach (Light light in rightBlinkers)
            light.enabled = right;
    }

    // Update is called once per frame
    void Update()
    {
        if(left)
        {
            if(counterLeft >= 0.7)
            {
                foreach(Light light in leftBlinkers)
                    light.enabled = on;

                counterLeft = 0;
                on = !on;
            }
            counterLeft += Time.deltaTime;
        }

        if(right)
        {
            if (counterRight >= 0.7)
            {
                foreach (Light light in rightBlinkers)
                    light.enabled = on;

                counterRight = 0;
                on = !on;
            }
            counterRight += Time.deltaTime;
        }
    }

    public void LeftBlinker()
    {
        left = !left;
        right = false;
        foreach (Light light in rightBlinkers)
            light.enabled = false;
        counterLeft = 0;
    }

    public void RightBlinker()
    {
        right = !right;
        left = false;
        foreach (Light light in leftBlinkers)
            light.enabled = false;
        counterRight = 0;
    }
}
