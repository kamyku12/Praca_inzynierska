using System;
using UnityEngine;

public class LearningArtificialBrain : MonoBehaviour
{
    public float episodeTimer;
    public float episodeLength;
    public int episodeNumber;

    public bool learning;
    public bool canRun;

    public Transform parkingSpotCoordinates;
    public Transform startingPoint;

    public CarController carController;
    public SelfDriving selfDriving;

    public Rigidbody rigidbody;

    private void Start()
    {
        canRun = true;
    }

    private void Update()
    {
        if(learning && canRun)
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
        Quaternion randomRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360f), 0);


        transform.position = randomPosition;
        transform.rotation = randomRotation;

        // Reset every value and stop rigidBody from moving
        carController.ResetValues();
        selfDriving.ResetValues();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        selfDriving.SetSendingDataEvent(SendingDataEvents.NewEpisode);
        canRun = false;
    }

    public void SetCanRun(bool newValue)
    {
        canRun = newValue;
    }
}
