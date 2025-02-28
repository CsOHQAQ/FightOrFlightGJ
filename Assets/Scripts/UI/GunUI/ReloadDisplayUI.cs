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

    [Header("State Colors")]
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color inWindowColor = new Color(1f, 0.5f, 0f); // Orange
    [SerializeField] Color afterWindowColor = Color.red;
    [SerializeField] Color failedColor = Color.red;
    [SerializeField] float failedFlashDuration = 0.3f;

    private Color currentMainColor;        // Color for the main reload bar
    private Color currentOverloadColor;    // Color for the overload bars
    private Coroutine failedFlashCoroutine;

    /// <summary>
    /// Called from AmmoDisplayUI (or similar) with the current OverloadState
    /// and whether the player has already failed this reload attempt.
    /// </summary>
    public void UpdateStateColors(OverloadState state, bool failedThisReload)
    {
        // If we just failed overload, do a quick flash to failedColor
        if (failedThisReload && failedFlashCoroutine == null)
        {
            failedFlashCoroutine = StartCoroutine(HandleFailedFlash());
        }

        switch(state)
        {
            case OverloadState.BeforeWindow:
                currentMainColor = normalColor;
                currentOverloadColor = normalColor;
                break;
            case OverloadState.InWindow:
                currentMainColor = inWindowColor;
                currentOverloadColor = inWindowColor;
                break;
            case OverloadState.AfterWindow:
                currentMainColor = afterWindowColor;
                currentOverloadColor = afterWindowColor;
                break;
        }

        ApplyColors(); 
    }

    /// <summary>
    /// A brief flash effect that lerps the colors to 'failedColor' 
    /// and then returns to the normal/hot color after the duration.
    /// </summary>
    private IEnumerator HandleFailedFlash()
    {
        float elapsed = 0f;
        // We'll store the original colors for the reload bar, but also 
        // gather them from the child images of left/right overload bars.

        Color startReloadColor = reloadFilledImage != null
            ? reloadFilledImage.color
            : Color.white;

        // We'll just assume the overload bars share the same color 
        // (we track them as currentOverloadColor). 
        // But if they differ, you can store them individually.
        Color startOverloadColor = (leftOverloadBar != null) 
            ? leftOverloadBar.color 
            : Color.white;

        while(elapsed < failedFlashDuration)
        {
            float t = elapsed / failedFlashDuration;
            // Lerp the main bar
            if (reloadFilledImage != null)
            {
                reloadFilledImage.color = Color.Lerp(startReloadColor, failedColor, t);
            }
            // Lerp all child images in leftOverloadBar
            if (leftOverloadBar != null)
            {
                SetImageColorRecursively(leftOverloadBar.transform, Color.Lerp(startOverloadColor, failedColor, t));
            }
            // Lerp all child images in rightOverloadBar
            if (rightOverloadBar != null)
            {
                SetImageColorRecursively(rightOverloadBar.transform, Color.Lerp(startOverloadColor, failedColor, t));
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // At the end, revert to the currentMainColor/currentOverloadColor
        if (reloadFilledImage != null)
        {
            reloadFilledImage.color = currentMainColor;
        }
        if (leftOverloadBar != null)
        {
            SetImageColorRecursively(leftOverloadBar.transform, currentOverloadColor);
        }
        if (rightOverloadBar != null)
        {
            SetImageColorRecursively(rightOverloadBar.transform, currentOverloadColor);
        }

        failedFlashCoroutine = null;
    }

    /// <summary>
    /// Applies the current colors to the reload bar & overload bars 
    /// if we are not in the middle of a flash.
    /// </summary>
    private void ApplyColors()
    {
        // If we are currently flashing from a fail, 
        // don't override that color
        if (failedFlashCoroutine != null) 
            return; 

        if (reloadFilledImage != null)
        {
            reloadFilledImage.color = currentMainColor;
        }
        if (leftOverloadBar != null)
        {
            SetImageColorRecursively(leftOverloadBar.transform, currentOverloadColor);
        }
        if (rightOverloadBar != null)
        {
            SetImageColorRecursively(rightOverloadBar.transform, currentOverloadColor);
        }
    }

    /// <summary>
    /// Recursively sets 'color' to all child Image components under 'parentTransform'.
    /// </summary>
    private void SetImageColorRecursively(Transform parentTransform, Color color)
    {
        // Optionally, set the parent's Image color if it has one
        Image parentImage = parentTransform.GetComponent<Image>();
        if (parentImage != null)
        {
            parentImage.color = color;
        }

        // Then set all children
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            SetImageColorRecursively(parentTransform.GetChild(i), color);
        }
    }

    /// <summary>
    /// Updates the progress bar for reload.
    /// Called each frame by the manager script if reloading.
    /// </summary>
    public void UpdateReloadProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
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
    /// Initializes positions of the overload bars based on the reload window percentage.
    /// Called once at the start of reload if desired.
    /// </summary>
    public void InitializeOverloadWindowUI(float windowPercentage)
    {
        if (reloadFilledImage == null || leftOverloadBar == null || rightOverloadBar == null)
        {
            Debug.LogWarning("Ensure all required images are assigned!");
            return;
        }

        windowPercentage = Mathf.Clamp01(windowPercentage);

        RectTransform reloadRectTransform = reloadFilledImage.rectTransform;
        Vector2 size = reloadRectTransform.sizeDelta;
        Vector2 midPoint = reloadRectTransform.localPosition;
        float halfWindowSize = (size.x * windowPercentage) / 2;

        Vector2 leftBarPosition = midPoint - new Vector2(halfWindowSize, 0);
        Vector2 rightBarPosition = midPoint + new Vector2(halfWindowSize, 0);

        leftOverloadBar.rectTransform.localPosition = leftBarPosition;
        rightOverloadBar.rectTransform.localPosition = rightBarPosition;
    }
}
