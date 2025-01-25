using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A specialized Enemy that manages an "eye" which opens and closes via a set of sprites.
/// Also tracks the player with a pupil handler.
/// </summary>
public class EyeMonster : Enemy
{
    [Header("References")]
    [SerializeField] private PupilHandler pupilHandler; 

    [Header("Eye Sprites")]
    [Tooltip("List of sprites from fully closed (index 0) to fully open (last index).")]
    [SerializeField] private List<Sprite> eyeSprites;

    [Header("Eye Animation Settings")]
    [Tooltip("How long it takes to go from closed to fully open.")]
    [SerializeField] private float openDuration = 1f;

    [Tooltip("How long it takes to go from open to fully closed.")]
    [SerializeField] private float closeDuration = 1f;

    [Tooltip("At which sprite index does the pupil become visible. E.g., 2 might be half-open.")]
    [SerializeField] private int pupilVisibleSpriteIndex = 2;

    [Header("Projectile Settings")]
    [SerializeField] private Rigidbody FireBlastPrefab;
    [SerializeField] private float _timeBetweenShots = 2.0f;
    [SerializeField] private float _flameBlastSpeed = 10f;

    // Eye state events
    public event Action OnEyeFullyOpened;
    public event Action OnEyeFullyClosed;

    // The current index in the sprite list
    private int currentSpriteIndex = 0;

    /// <summary>
    /// Is the eye fully closed?
    /// The eye is considered closed if the currentSpriteIndex == 0.
    /// </summary>
    public bool EyeIsClosed
    {
        get { return currentSpriteIndex <= 0; }
    }

    private void Start()
    {
        base.Start();
        // Initialize to fully closed
        if (eyeSprites.Count > 0)
        {
            currentSpriteIndex = 0;
            spriteRenderer.sprite = eyeSprites[currentSpriteIndex];
        }
        else
        {
            Debug.LogWarning("EyeMonster: No sprites assigned!");
        }

        UpdatePupilVisibility();
    }

    /// <summary>
    /// Called externally or from AI to open the eye from its current state up to fully open.
    /// </summary>
    public void OpenEye()
    {
        StopAllCoroutines();
        StartCoroutine(OpenEyeCoroutine());
    }

    /// <summary>
    /// Called externally or from AI to close the eye from its current state back to fully closed.
    /// </summary>
    public void CloseEye()
    {
        StopAllCoroutines();
        StartCoroutine(CloseEyeCoroutine());
    }

    /// <summary>
    /// Tells the pupil handler to track a particular target. 
    /// For example, we might call this from an AI script or from Update with the player's transform.
    /// </summary>
    public void TrackGameObject(Transform target)
    {
        // Only track the pupil if we have one
        // The pupil handler might handle whether it is visible or not
        pupilHandler?.TrackGameObject(target);
    }

    /// <summary>
    /// Coroutine that animates from the current sprite index up to the last index in eyeSprites.
    /// At the end, triggers OnEyeFullyOpened event.
    /// </summary>
    private IEnumerator OpenEyeCoroutine()
    {
        if (eyeSprites.Count == 0) yield break;

        int startIndex = currentSpriteIndex;
        int endIndex = eyeSprites.Count - 1;
        float journeyTime = 0f;

        // The number of steps we need to animate between
        float totalSteps = endIndex - startIndex;
        if (totalSteps <= 0f) // Already fully open or invalid
        {
            // Already at or beyond last sprite
            currentSpriteIndex = endIndex;
            spriteRenderer.sprite = eyeSprites[currentSpriteIndex];
            UpdatePupilVisibility();
            OnEyeFullyOpened?.Invoke();
            yield break;
        }

        while (journeyTime < openDuration)
        {
            journeyTime += Time.deltaTime;
            float t = Mathf.Clamp01(journeyTime / openDuration);

            // Lerp the sprite index from startIndex to endIndex
            float floatIndex = Mathf.Lerp(startIndex, endIndex, t);
            int newIndex = Mathf.RoundToInt(floatIndex);

            newIndex = Mathf.Clamp(newIndex, 0, eyeSprites.Count - 1);
            if (newIndex != currentSpriteIndex)
            {
                currentSpriteIndex = newIndex;
                spriteRenderer.sprite = eyeSprites[currentSpriteIndex];
                UpdatePupilVisibility();
            }

            yield return null;
        }

        // Ensure final sprite is the last
        currentSpriteIndex = endIndex;
        spriteRenderer.sprite = eyeSprites[currentSpriteIndex];
        UpdatePupilVisibility();
        OnEyeFullyOpened?.Invoke();
    }

    /// <summary>
    /// Coroutine that animates from the current sprite index down to 0 (fully closed).
    /// At the end, triggers OnEyeFullyClosed event.
    /// </summary>
    private IEnumerator CloseEyeCoroutine()
    {
        if (eyeSprites.Count == 0) yield break;

        int startIndex = currentSpriteIndex;
        int endIndex = 0;
        float journeyTime = 0f;

        float totalSteps = startIndex - endIndex;
        if (totalSteps <= 0f) // Already closed or invalid
        {
            currentSpriteIndex = 0;
            spriteRenderer.sprite = eyeSprites[currentSpriteIndex];
            UpdatePupilVisibility();
            OnEyeFullyClosed?.Invoke();
            yield break;
        }

        while (journeyTime < closeDuration)
        {
            journeyTime += Time.deltaTime;
            float t = Mathf.Clamp01(journeyTime / closeDuration);

            float floatIndex = Mathf.Lerp(startIndex, endIndex, t);
            int newIndex = Mathf.RoundToInt(floatIndex);

            newIndex = Mathf.Clamp(newIndex, 0, eyeSprites.Count - 1);
            if (newIndex != currentSpriteIndex)
            {
                currentSpriteIndex = newIndex;
                spriteRenderer.sprite = eyeSprites[currentSpriteIndex];
                UpdatePupilVisibility();
            }

            yield return null;
        }

        currentSpriteIndex = endIndex;
        spriteRenderer.sprite = eyeSprites[currentSpriteIndex];
        UpdatePupilVisibility();
        OnEyeFullyClosed?.Invoke();
    }

    /// <summary>
    /// Called whenever we change sprite index. 
    /// If the currentSpriteIndex < pupilVisibleSpriteIndex, hide the pupil. 
    /// Otherwise show it. 
    /// </summary>
    private void UpdatePupilVisibility()
    {
        if (pupilHandler == null) return;

        bool pupilVisible = (currentSpriteIndex >= pupilVisibleSpriteIndex);
        pupilHandler.hidePupil = !pupilVisible;
    }
}
