using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateEpisodeTimer : MonoBehaviour
{
    TextMeshProUGUI episodeTimer;
    public LearningArtificialBrain learning;
    
    void Start()
    {
        episodeTimer = transform.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        episodeTimer.text = "Timer: " + learning.GetTimer();
    }
}
