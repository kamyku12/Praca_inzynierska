using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateEpisodeNumberDisplay : MonoBehaviour
{
    TextMeshProUGUI episodeNumberText;
    public LearningArtificialBrain learning;
    void Start()
    {
        episodeNumberText = transform.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        episodeNumberText.text = "Number: " + learning.GetEpisodeNumber().ToString();
    }
}
