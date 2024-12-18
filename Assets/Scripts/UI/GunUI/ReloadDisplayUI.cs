using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloadDisplayUI : MonoBehaviour
{
    [SerializeField]
    private Image reloadFilledImage; // Reference to the reload progress Image

    [SerializeField]
    private Image leftOverloadBar;

    [SerializeField]
    private Image rightOverloadBar;

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

    /// <summary>
    /// Initializes the positions of the overload bars based on the reload window percentage.
    /// </summary>
    /// <param name="windowPercentage">The percentage (0 to 1) that determines the size of the reload window.</param>
    public void InitializeOverloadWindowUI(float windowPercentage)
    {

        Debug.Log(windowPercentage);
        if (reloadFilledImage == null || leftOverloadBar == null || rightOverloadBar == null)
        {
            Debug.LogWarning("Ensure all required images (reloadFilledImage, leftOverloadBar, rightOverloadBar) are assigned!");
            return;
        }

        // Ensure the percentage is clamped between 0 and 1
        windowPercentage = Mathf.Clamp01(windowPercentage);

        // Get the size of the reloadFilledImage
        RectTransform reloadRectTransform = reloadFilledImage.rectTransform;
        Vector2 size = reloadRectTransform.sizeDelta;

        // Calculate the midpoint of the reload bar
        Vector2 midPoint = reloadRectTransform.localPosition;

        // Calculate the offset for the overload bars based on the percentage
        float halfWindowSize = (size.x * windowPercentage) / 2;

        // Update the positions of the left and right overload bars
        Vector2 leftBarPosition = midPoint - new Vector2(halfWindowSize, 0);
        Vector2 rightBarPosition = midPoint + new Vector2(halfWindowSize, 0);

        leftOverloadBar.rectTransform.localPosition = leftBarPosition;
        rightOverloadBar.rectTransform.localPosition = rightBarPosition;
    }
}
