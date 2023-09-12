using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LearningArtificialBrain : MonoBehaviour
{
    public float episodeTimer;
    public float episodeLength;

    public bool learning;

    public Transform parkingSpotCoordinates;

    private void Update()
    {
        if(learning)
        {
            if(episodeTimer < episodeLength)
            {
                episodeTimer += Time.deltaTime;
                return;
            }
        }

    }

    public void SetLearning(bool learning)
    {
        this.learning = learning;
    }
}
