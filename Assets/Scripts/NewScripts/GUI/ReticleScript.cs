using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A script to adjust a 4-line reticle (top/bottom/left/right) 
/// based on the "current spread angle" from the player's attributes.
/// </summary>
public class ReticleScript : MonoBehaviour
{
    [Header("Reticle Lines")]
    public RectTransform topLine;
    public RectTransform bottomLine;
    public RectTransform leftLine;
    public RectTransform rightLine;

    [Header("Gap Settings")]
    [Tooltip("If true, uses a linear approach. If false, uses a tangent-based approach.")]
    public bool useLinearMapping = true;

    [Tooltip("A baseline gap, even if spread angle = 0.")]
    public float baseGap = 10f;

    [Tooltip("Additional scale for angle => gap. For linear: gap = baseGap + spreadAngle * scale.\n" +
             "For trig: gap = scale * tan(spreadAngle/2).")]
    public float gapScale = 3f;

    [Tooltip("Reticle will not exceed this gap in pixels.")]
    public float maxGap = 200f;

    [Tooltip("Reticle lines will smoothly move to the new gap.")]
    public float smoothSpeed = 10f;

    // Lerp state
    private float currentGap = 0f;


    public PlayerCharacter PlayerCharacter;

    private void Start()
    {
        // If not assigned in Inspector, try to find
        if (PlayerCharacter == null)
        {
            PlayerCharacter = GameManager.Instance.PlayerCharacter;
        }

        if (topLine == null || bottomLine == null || leftLine == null || rightLine == null)
        {
            Debug.LogError("ReticleScript: One or more line references not set!");
        }
    }

    private void Update()
    {
        // 1) Get the "spread angle" from the player's attribute set.
        float spreadAngle = GetSpreadAngleFromPlayer();

        // 2) Convert angle to target gap
        float targetGap = CalculateGap(spreadAngle);

        // 3) Smoothly lerp
        currentGap = Mathf.Lerp(currentGap, targetGap, Time.deltaTime * smoothSpeed);

        // 4) Clamp
        currentGap = Mathf.Clamp(currentGap, 0f, maxGap);

        // 5) Apply gap to reticle lines
        ApplyReticlePositions(currentGap);
    }

    /// <summary>
    /// Reads the player's spread angle from their attribute set.
    /// E.g. BaseSpreadAngle.CurrentValue
    /// If not found, returns 0f.
    /// </summary>
    float GetSpreadAngleFromPlayer()
    {
        if (PlayerCharacter == null) return 0f;

        // Retrieve the player's attribute set if present
        var asc = PlayerCharacter.GetAbilitySystemComponent();
        if (asc == null) return 0f;

        var attrSet = asc.AttributeSet as PlayerCharacterAttributeSet;
        if (attrSet == null) return 0f;

        // The property you specifically mentioned:
        float angle = attrSet.BaseSpreadAngle.CurrentValue;
        return angle; // e.g. if it's 5 => 5 degrees
    }

    /// <summary>
    /// Converts the spread angle to a gap in pixels.
    /// </summary>
    float CalculateGap(float spreadAngleDegrees)
    {
        if (useLinearMapping)
        {
            // gap = baseGap + spreadAngle * gapScale
            return baseGap + (spreadAngleDegrees * gapScale);
        }
        else
        {
            // trig approach
            float halfAngleRadians = (spreadAngleDegrees * 0.5f) * Mathf.Deg2Rad;
            float tanVal = Mathf.Tan(halfAngleRadians);
            return gapScale * tanVal;
        }
    }

    /// <summary>
    /// Offsets the four lines away from (0,0) by 'gap' pixels.
    /// We assume the parent pivot is center at (0,0).
    /// </summary>
    void ApplyReticlePositions(float gap)
    {
        if (topLine != null)
            topLine.anchoredPosition    = new Vector2(0f, +gap);

        if (bottomLine != null)
            bottomLine.anchoredPosition = new Vector2(0f, -gap);

        if (leftLine != null)
            leftLine.anchoredPosition   = new Vector2(-gap, 0f);

        if (rightLine != null)
            rightLine.anchoredPosition  = new Vector2(+gap, 0f);
    }
}
