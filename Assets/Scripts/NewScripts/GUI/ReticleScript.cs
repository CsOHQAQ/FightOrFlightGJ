using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReticleScript : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera viewCamera;
    public bool useVerticalFOV = true; // Horizontal FOV for ultrawide support
    [SerializeField] private float referenceScreenHeight = 1080f;

    private float fovTan;

    [Header("Reticle Lines")]
    public RectTransform topLine;
    public RectTransform bottomLine;
    public RectTransform leftLine;
    public RectTransform rightLine;

    [Header("Gap Settings")]
    public bool useLinearMapping = true;
    public float baseGap = 10f;
    public float gapScale = 3f;
    public float maxGap = 200f;
    public float smoothSpeed = 10f;

    private float currentGap = 0f;

    // The additional offset from firing "kick"
    private float fireKickOffset = 0f;
    [SerializeField]
    private float fireKickMaxValue = 10f;    // how big the reticle "jumps" 
    [SerializeField]
    private float fireKickDuration = 0.2f;   // total time for the effect
    private bool isKicking = false;

    [Header("References")]
    public PlayerCharacter playerCharacter;

    void Start()
    {
        if (playerCharacter == null)
        {
            playerCharacter = GameManager.Instance.PlayerCharacter;
        }
        CacheFOV();
        if (!viewCamera) viewCamera = Camera.main;
        // (A) Subscribe to a "WeaponFired" event (option 1: do it via PlayerCharacter)
        // E.g. if PlayerCharacter re-raises an event:
        // playerCharacter.OnWeaponFired += OnWeaponFired;

        // or (B) If you prefer: 
        //   - Listen to player's "OnPlayerEquipped" to get the current weapon
        //   - Then weapon.OnFired += OnWeaponFired
    }

    void Update()
    {
        // 1) Compute the normal spread-based gap
        float spreadAngle = GetSpreadAngleFromPlayer();
        float targetGap = CalculateGap(spreadAngle);

        // 2) Add the current fireKickOffset
        float combinedGap = targetGap + fireKickOffset;

        // 3) Lerp the 'currentGap' to that combined value
        currentGap = Mathf.Lerp(currentGap, combinedGap, Time.deltaTime * smoothSpeed);

        // 4) Clamp
        currentGap = Mathf.Clamp(currentGap, 0f, maxGap);

        // 5) Apply to reticle lines
        ApplyReticlePositions(currentGap);
    }

    /// <summary>
    /// Called when the player or weapon indicates "Weapon Fired."
    /// We begin a short "kick" effect that adds an offset to reticle.
    /// </summary>
    public void OnWeaponFired()
    {
        // Start the coroutine if not already
        if (!isKicking)
        {
            StartCoroutine(HandleReticleKick());
        }
        else
        {
            // If you want repeated shots to "stack" or refresh the effect,
            // you can reset the timer or re-start the coroutine
            // For simplicity, let's just re-start
            StopCoroutine(HandleReticleKick());
            StartCoroutine(HandleReticleKick());
        }
    }

    private IEnumerator HandleReticleKick()
    {
        isKicking = true;

        float timer = 0f;
        float peak = fireKickMaxValue; 

        // We'll animate fireKickOffset from peak back to 0 over fireKickDuration 
        // with an "ease out" or a small "back" effect
        while (timer < fireKickDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fireKickDuration);

            // Easing function - an "EaseOutQuad" as an example
            float easedT = 1f - Mathf.Pow(1f - t, 2f); 
            // Alternatively, you could do an overshoot/back approach

            // We want it to start at peak and go to 0, so "offset = peak * (1 - easedT)"
            // meaning at t=0 => offset = peak, t=1 => offset = 0
            fireKickOffset = peak * (1f - easedT);

            yield return null;
        }

        fireKickOffset = 0f;
        isKicking = false;
    }

    float GetSpreadAngleFromPlayer()
    {
        if (playerCharacter == null) return 0f;
        var asc = playerCharacter.GetAbilitySystemComponent();
        if (asc == null) return 0f;

        var attrSet = asc.AttributeSet as PlayerCharacterAttributeSet;
        if (attrSet == null) return 0f;

        return attrSet.BaseSpreadAngle.CurrentValue;
    }

    float CalculateGap(float spreadAngleDegrees)
    {
        if (!viewCamera || fovTan <= Mathf.Epsilon)
        {
            CacheFOV();
            if (!viewCamera) return 0f;
        }

        // Convert spread angle to screen space
        float spreadTan = Mathf.Tan(spreadAngleDegrees * Mathf.Deg2Rad / 2);
        float screenRatio = spreadTan / fovTan;
        
        // Convert to pixel space
        float screenGap = screenRatio * referenceScreenHeight / 2f;
        
        // Apply non-linear perception curve
        screenGap = Mathf.Pow(screenGap, 0.9f);
        
        return Mathf.Clamp(baseGap + screenGap * gapScale, 0f, maxGap);
    }

    void ApplyReticlePositions(float gap)
    {
        if (topLine    != null) topLine.anchoredPosition    = new Vector2(0f, +gap);
        if (bottomLine != null) bottomLine.anchoredPosition = new Vector2(0f, -gap);
        if (leftLine   != null) leftLine.anchoredPosition   = new Vector2(-gap, 0f);
        if (rightLine  != null) rightLine.anchoredPosition  = new Vector2(+gap, 0f);
    }

    void CacheFOV()
    {
        if (!viewCamera) return;
        
        float fov = useVerticalFOV ? 
            viewCamera.fieldOfView :
            2 * Mathf.Atan(Mathf.Tan(viewCamera.fieldOfView * Mathf.Deg2Rad / 2) * 
            viewCamera.aspect) * Mathf.Rad2Deg;

        fovTan = Mathf.Tan(fov * Mathf.Deg2Rad / 2);
    }
}
