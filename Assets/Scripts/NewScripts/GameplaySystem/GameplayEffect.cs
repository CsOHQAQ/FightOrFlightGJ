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
    public List<FGameplayModifierInfo> Modifiers = new List<FGameplayModifierInfo>();

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

[Serializable]
public struct FGameplayModifierInfo
{
    public AttributeReference TargetAttribute;
    public ModifierType ModifierOperation; // Add, Multiply, Override
    public float Magnitude; // Or could be replaced by a Calculation Class later
}

public enum GameplayEffectMagnitudeCalculation
{
    /** Use a simple, scalable float for the calculation. */
    ScalableFloat,
    /** Perform a calculation based upon an attribute. */
    AttributeBased,
    /** Perform a custom calculation, capable of capturing and acting on multiple attributes, in either BP or native. */
    //CustomCalculationClass,	
    /** This magnitude will be set explicitly by the code/blueprint that creates the spec. */
    //SetByCaller,
}

public enum AttributeBasedFloatCalculationType
{
    /** Use the final evaluated magnitude of the attribute. */
    AttributeMagnitude,
    /** Use the base value of the attribute. */
    AttributeBaseValue,
    /** Use the "bonus" evaluated magnitude of the attribute: Equivalent to (FinalMag - BaseValue). */
    AttributeBonusMagnitude,
    /** Use a calculated magnitude stopping with the evaluation of the specified "Final Channel" */
    //AttributeMagnitudeEvaluatedUpToChannel
}

[Serializable]
public struct GameplayEffectModifierMagnitude
{
    [SerializeField]
    private GameplayEffectMagnitudeCalculation MagnitudeCalculationType;
    [SerializeField]
    float magnitude;


    


}

public class GameplayEffectSpec
{
    public GameplayEffect Effect;
    public float Level;
    public float StartTime;
    
    //public Dictionary<GameplayAttribute, float> CalculatedModifiers;
    public List<GameplayEffectModifiedAttribute> ModifiedAttributes;
    public GameplayEffectSpec(GameplayEffect effect, float level)
    {
        Effect = effect;
        Level = level;
        StartTime = Time.time;
        //CalculatedModifiers = new Dictionary<string, float>();
    }

}

public struct GameplayEffectModifiedAttribute
{
    public GameplayAttribute Attribute;
    public float TotalMagnitude;
}


[Serializable]
public class AttributeBasedFloat
{
    // Coefficient to the attribute calculation
    [SerializeField] private float coefficient = 1f;

    // Additive value to the attribute calculation, added in before the coefficient applies
    [SerializeField] private float preMultiplyAdditiveValue = 0f;

    // Additive value to the attribute calculation, added in after the coefficient applies
    [SerializeField] private float postMultiplyAdditiveValue = 0f;

    // Attribute backing the calculation
    [SerializeField] private GameplayEffectAttributeCaptureDefinition backingAttribute;

    // If a curve table entry is specified, the attribute will be used as a lookup into the curve instead of using the attribute directly
    [SerializeField] private AnimationCurve attributeCurve;

    // Calculation policy in regards to the attribute
    [SerializeField] private AttributeBasedFloatCalculationType attributeCalculationType = AttributeBasedFloatCalculationType.AttributeMagnitude;

    // Filter to use on source tags
    [SerializeField] private GameplayTagContainer sourceTagFilter;

    // Filter to use on target tags
    [SerializeField] private GameplayTagContainer targetTagFilter;

    // Constructor
    public AttributeBasedFloat()
    {
        coefficient = 1f;
        preMultiplyAdditiveValue = 0f;
        postMultiplyAdditiveValue = 0f;
        backingAttribute = new GameplayEffectAttributeCaptureDefinition();
        attributeCalculationType = AttributeBasedFloatCalculationType.AttributeMagnitude;
    }

    /// <summary>
    /// Calculate and return the magnitude of the float based on the specified gameplay effect spec.
    /// Assumes the existence of the required captured attribute within the spec.
    /// </summary>
    /// <param name="relevantSpec">Gameplay effect spec providing the backing attribute capture.</param>
    /// <returns>Evaluated magnitude based upon the spec and calculation policy.</returns>
    public float CalculateMagnitude(GameplayEffectSpec relevantSpec)
    {
        // Placeholder implementation. Replace with your actual calculation logic.
        float baseValue = backingAttribute.GetAttributeValue(relevantSpec);

        float preAdd = baseValue + preMultiplyAdditiveValue;
        float scaledValue = preAdd * coefficient;
        float finalValue = scaledValue + postMultiplyAdditiveValue;

        if (attributeCurve != null)
        {
            finalValue = attributeCurve.Evaluate(finalValue);
        }

        return finalValue;
    }

    // Equality and inequality operators
    public override bool Equals(object obj)
    {
        if (obj is AttributeBasedFloat other)
        {
            return coefficient == other.coefficient &&
                   preMultiplyAdditiveValue == other.preMultiplyAdditiveValue &&
                   postMultiplyAdditiveValue == other.postMultiplyAdditiveValue &&
                   Equals(backingAttribute, other.backingAttribute) &&
                   attributeCalculationType == other.attributeCalculationType &&
                   Equals(sourceTagFilter, other.sourceTagFilter) &&
                   Equals(targetTagFilter, other.targetTagFilter);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(coefficient, preMultiplyAdditiveValue, postMultiplyAdditiveValue, backingAttribute, attributeCalculationType, sourceTagFilter, targetTagFilter);
    }

    public static bool operator ==(AttributeBasedFloat left, AttributeBasedFloat right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(AttributeBasedFloat left, AttributeBasedFloat right)
    {
        return !Equals(left, right);
    }
}
