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
    private Collider pupilCollider;

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
    private float attackCoolDownTimer=0f;
    public float AttackCoolDownTimer { get{return attackCoolDownTimer;}}
    // Eye state events
    public event Action OnEyeFullyOpened;
    public event Action OnEyeFullyClosed;
    private bool isEyeOpen = false;
    public bool IsEyeOpen{get{return isEyeOpen;}}
    private bool isEyeOpening = false;
    private bool isEyeClosing =false;
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
        pupilCollider = GetComponent<Collider>();
        UpdatePupilVisibility();
        OnEyeFullyOpened+=OnFullyOpened;
        OnEyeFullyClosed+=OnFullyClosed;
    }

    private void OnDestroy()
    {
        OnEyeFullyOpened-=OnFullyOpened;
        OnEyeFullyClosed-=OnFullyClosed;
    }
    protected void Update()
    {
        base.Update();
        attackCoolDownTimer = Mathf.Max(0f,attackCoolDownTimer-Time.deltaTime);
        
    }
    
    public virtual void Attack()
    {
        attackFeedback.PlayFeedbacks();
        EventContext attackContext = new EventContext
        {
            // The AI character is the "Source" of the attack
            Source = this, 
            Target = playerCharacter,
            AttackInfo = new AttackData
            {
                BaseDamage = 10f,         // or set from some stat
                ProjectilePrefab = FireBlastPrefab.gameObject,
            }
            
            // HitData is left empty, since the collision hasn't happened yet
        };
        Rigidbody flameBlast = GameObject.Instantiate(FireBlastPrefab, pupilHandler.transform.position, Quaternion.identity);
        Vector3 direction = (playerCharacter.transform.position - pupilHandler.transform.position).normalized;
        Projectile projectile = flameBlast.GetComponent<Projectile>();
            if (projectile != null)
            {
                // Pass the same context + direction so the projectile 
                // can reference AttackInfo when it hits the target
                projectile.Setup(attackContext, direction);
            }
        CloseEye();
        //StateMachine.ChangeState(ChaseState);
    }
    /// <summary>
    /// Called externally or from AI to open the eye from its current state up to fully open.
    /// </summary>
    public void OpenEye()
    {
        if(isEyeOpening) {return;}
        StopCoroutine(CloseEyeCoroutine());
        //StopAllCoroutines();
        //Debug.LogWarning("Trying to Open Eye");
        isEyeOpening = true;
        StartCoroutine(OpenEyeCoroutine());
        
    }

    /// <summary>
    /// Called externally or from AI to close the eye from its current state back to fully closed.
    /// </summary>
    public void CloseEye()
    {
        if(isEyeClosing) {return;}
        isEyeOpen = false;
        //StopAllCoroutines();
        StopCoroutine(OpenEyeCoroutine());
        //Debug.LogWarning("Trying to Close Eye");
        isEyeClosing= true;
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

        attackCoolDownTimer = _timeBetweenShots;
        
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
        
        // New collider logic
        if (pupilCollider != null)
        {
            pupilCollider.enabled = pupilVisible;
        }
    }

    private void OnFullyOpened()
    {
        isEyeOpen = true;
        isEyeOpening = false;
    }

    private void OnFullyClosed()
    {
        isEyeClosing = false;
    }
}
