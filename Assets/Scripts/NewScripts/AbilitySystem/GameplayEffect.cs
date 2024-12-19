using UnityEngine;
using System;
using System.Collections.Generic;

public enum EDurationPolicy
{
    Instant,
    HasDuration,
    Infinite
}

public enum EStackingPolicy
{
    None,
    AddStack,
    RefreshDuration,
    AggregateDuration
    // Other policies can be added as needed.
}

[CreateAssetMenu(fileName = "GameplayEffect", menuName = "AbilitySystem/GameplayEffect")]
public class GameplayEffect : ScriptableObject
{
    [Header("Modifiers")]
    public List<GameplayEffectModifier> Modifiers = new List<GameplayEffectModifier>();

    [Header("Duration & Period")]
    public EDurationPolicy DurationPolicy = EDurationPolicy.Instant;
    
    [Tooltip("Used if DurationPolicy is HasDuration. If 0 or negative and policy is HasDuration, treat it as instant.")]
    public float Duration = 0f;

    [Tooltip("If > 0, this effect will tick periodically at this interval (e.g., DOT effects).")]
    public float Period = 0f;

    [Tooltip("If true and DurationPolicy is HasDuration, periodic logic applies at periodic intervals.")]
    public bool IsPeriodic => Period > 0f && DurationPolicy == EDurationPolicy.HasDuration;

    [Header("Stacking")]
    public EStackingPolicy StackingPolicy = EStackingPolicy.None;
    public int MaxStackCount = 1; // If stacking is allowed, how many stacks max.

    [Tooltip("If true, the effect remains until removed. Overrules DurationPolicy if set incorrectly.")]
    public bool IsInfinite;  // If this is true, consider DurationPolicy = EDurationPolicy.Infinite internally.

    private void OnValidate()
    {
        // Ensure DurationPolicy aligns with IsInfinite
        if (IsInfinite)
        {
            DurationPolicy = EDurationPolicy.Infinite;
        }
        else
        {
            if (DurationPolicy == EDurationPolicy.Infinite)
            {
                // If not infinite requested but policy is infinite, revert if needed.
                // Or do nothing, developer chooses what makes sense.
            }
        }

        // If DurationPolicy is Instant, ignore Duration and Period
        if (DurationPolicy == EDurationPolicy.Instant)
        {
            Duration = 0f;
            // Periodic doesn't make sense for Instant effects
        }

        // If DurationPolicy is HasDuration but Duration <= 0, treat as Instant
        if (DurationPolicy == EDurationPolicy.HasDuration && Duration <= 0)
        {
            DurationPolicy = EDurationPolicy.Instant;
        }

        // If infinite, no need for Duration
        if (DurationPolicy == EDurationPolicy.Infinite)
        {
            Duration = float.PositiveInfinity;
        }
    }
}

[Serializable]
public struct GameplayEffectHandle
{
    public int HandleID; // This could be a unique ID assigned when the effect is applied.
}