using System;
using UnityEngine;

public class LearningArtificialBrain : MonoBehaviour
{
    public float episodeTimer;
    public float episodeLength;
    public int episodeNumber;

    public bool learning;

    public Transform parkingSpotCoordinates;
    public Transform startingPoint;

    public CarController carController;

    private void Update()
    {
        if(learning)
        {
            if(episodeTimer < episodeLength)
            {
                episodeTimer += Time.deltaTime;
                return;
            }

            ResetEpisode();
        }
    }

    public void SetLearning(bool learning)
    {
        this.learning = learning;
    }

    public void ToggleLearning()
    {
        learning = !learning;
    }

    public string GetTimer()
    {
        return Math.Round(episodeTimer, 2).ToString();
    }

    public int GetEpisodeNumber()
    {
        return episodeNumber;
    }

    public void ResetEpisode()
    {
        episodeTimer = 0.0f;
        episodeNumber += 1;

        Vector3 randomPosition = startingPoint.position + new Vector3(UnityEngine.Random.value, 0.15f, UnityEngine.Random.value);
        Quaternion randomRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, UnityEngine.Random.Range(0, 360f), transform.rotation.eulerAngles.z);


        transform.position = randomPosition;
        transform.rotation = randomRotation;

        carController.ResetValues();
    }
}
