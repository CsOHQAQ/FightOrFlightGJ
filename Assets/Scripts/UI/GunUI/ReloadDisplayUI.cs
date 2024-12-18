using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloadDisplayUI : MonoBehaviour
{
    [SerializeField]
    private Image reloadFilledImage; // Reference to the reload progress Image

    /// <summary>
    /// Updates the progress bar for reload.
    /// </summary>
    /// <param name="progress">The current reload progress as a float (0 to 1).</param>
    public void UpdateReloadProgress(float progress)
    {
        // Ensure the progress value is clamped between 0 and 1
        progress = Mathf.Clamp01(progress);

        // Update the fill amount of the image
        if (reloadFilledImage != null)
        {
            reloadFilledImage.fillAmount = progress;
        }
        else
        {
            Debug.LogWarning("ReloadFilledImage is not assigned!");
        }
    }
}
