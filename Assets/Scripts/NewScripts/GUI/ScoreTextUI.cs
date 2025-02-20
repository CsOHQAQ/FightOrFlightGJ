using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Import the TextMeshPro namespace

public class ScoreUI : MonoBehaviour
{
    // Cache the TextMeshProUGUI component
    private TextMeshProUGUI scoreText;

    void Start()
    {
        // Get the TextMeshProUGUI component attached to this GameObject
        scoreText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // Update the text to display the current score from the GameManager
        scoreText.text = "Score: " + GameManager.Instance.CurrentScore;
    }
}
