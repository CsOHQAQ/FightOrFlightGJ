using UnityEngine;
using System;
using System.Collections.Generic;

public enum DurationPolicy
{
    Instant,
    HasDuration,
    Infinite
}

public enum StackingPolicy
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
    public DurationPolicy DurationPolicy = DurationPolicy.Instant;
    
    [Tooltip("Used if DurationPolicy is HasDuration. If 0 or negative and policy is HasDuration, treat it as instant.")]
    public float Duration = 0f;

    [Tooltip("If > 0, this effect will tick periodically at this interval (e.g., DOT effects). Periodic GameplayEffects are treated like instant GameplayEffects and change the BaseValue.")]
    public float Period = 0f;

    [Tooltip("If true and DurationPolicy is not Instant, periodic logic applies at periodic intervals.")]
    public bool IsPeriodic => Period > 0f && DurationPolicy != DurationPolicy.Instant;

    [Header("Stacking")]
    public StackingPolicy StackingPolicy = StackingPolicy.None;
    public int MaxStackCount = 1; // If stacking is allowed, how many stacks max.

    [Tooltip("If true, the effect remains until removed. Overrules DurationPolicy if set incorrectly.")]
    public bool IsInfinite;  // If this is true, consider DurationPolicy = DurationPolicy.Infinite internally.

    private void OnValidate()
    {
        // Ensure DurationPolicy aligns with IsInfinite
        if (IsInfinite)
        {
            DurationPolicy = DurationPolicy.Infinite;
        }
        else
        {
            if (DurationPolicy == DurationPolicy.Infinite)
            {
                // If not infinite requested but policy is infinite, revert if needed.
                // Or do nothing, developer chooses what makes sense.
            }
        }

        // If DurationPolicy is Instant, ignore Duration and Period
        if (DurationPolicy == DurationPolicy.Instant)
        {
            Duration = 0f;
            // Periodic doesn't make sense for Instant effects
        }

        // If DurationPolicy is HasDuration but Duration <= 0, treat as Instant
        //if (DurationPolicy == DurationPolicy.HasDuration && Duration <= 0)
        //{
        //    DurationPolicy = DurationPolicy.Instant;
        //}

        // If infinite, no need for Duration
        if (DurationPolicy == DurationPolicy.Infinite)
        {
            Duration = float.PositiveInfinity;
        }
    }
}

[Serializable]
public struct GameplayEffectSpecHandle
{
    public int HandleID; // This could be a unique ID assigned when the effect is applied.
}

[Serializable]
public struct FGameplayModifierInfo
{
    public AttributeReference TargetAttribute;
    public ModifierType ModifierOperation;

    // Instead of float Magnitude, we store a more flexible data structure:
    public GameplayEffectModifierMagnitude ModifierMagnitude;
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
    AttributeBonusMagnitude
}

[Serializable]
public struct GameplayEffectModifierMagnitude
{
    [SerializeField] public GameplayEffectMagnitudeCalculation magnitudeCalculationType;

    // We'll keep a base float for simple or "ScalableFloat" usage
    [SerializeField] public float flatMagnitude;

    // If it's attribute-based, we'll use an embedded AttributeBasedFloat
    [SerializeField] public AttributeBasedFloat attributeBased;

    // Additional fields for custom or set-by-caller logic could go here.

    public float CalculateMagnitude(GameplayEffectSpec spec, out bool foundAttribute)
    {
        foundAttribute = true;

        switch (magnitudeCalculationType)
        {
            case GameplayEffectMagnitudeCalculation.ScalableFloat:
                // For simplicity, treat flatMagnitude as a direct float or a curve-based logic
                return flatMagnitude;

            case GameplayEffectMagnitudeCalculation.AttributeBased:
                if (attributeBased != null)
                {
                    return attributeBased.CalculateMagnitude(spec, out foundAttribute);
                }
                // If not assigned, fallback
                foundAttribute = false;
                return 0f;

            // case GameplayEffectMagnitudeCalculation.CustomCalculationClass:
            //     // Potentially instantiate or call a custom class
            //     break;
            // case GameplayEffectMagnitudeCalculation.SetByCaller:
            //     // Logic for code or blueprint setting magnitude at runtime
            //     break;

            default:
                return flatMagnitude;
        }
    }
}

    // Constructor
public class GameplayEffectSpec
{
    public GameplayEffect Effect;
    public float Level;
    public float StartTime;
    public float NextTickTime; // Tracks the next time the periodic effect should tick

    private GameObject sourceActor;
    public GameObject SourceActor { get { return sourceActor; } }

    private GameObject targetActor;
    public GameObject TargetActor { get { return targetActor; } }

    public int StackCount { get; private set; } = 1;

    private Dictionary<GameplayEffectAttributeCaptureDefinition, float> snapshotData 
        = new Dictionary<GameplayEffectAttributeCaptureDefinition, float>();

    public List<GameplayEffectModifiedAttribute> ModifiedAttributes = new List<GameplayEffectModifiedAttribute>();

    public GameplayEffectSpec(GameplayEffect effect, float level, GameObject inSource, GameObject inTarget)
    {
        Effect = effect;
        Level = level;
        StartTime = Time.time;
        sourceActor = inSource;
        targetActor = inTarget;

        if (Effect != null)
        {
            // If effect has attribute-based captures and snapshot is required, store them
            foreach (var modInfo in Effect.Modifiers)
            {
                if (modInfo.ModifierMagnitude.magnitudeCalculationType == GameplayEffectMagnitudeCalculation.AttributeBased)
                {
                    var backing = modInfo.ModifierMagnitude.attributeBased;
                    var captureDef = backing.backingAttribute;

                    if (captureDef.IsSnapshot())
                    {
                        SnapshotAttribute(captureDef);
                    }
                }
            }
        }

        if (Effect.IsPeriodic)
        {
            NextTickTime = StartTime + Effect.Period;
        }
    }

    // Example stack logic:
    public void AddStack()
    {
        if (Effect.StackingPolicy == StackingPolicy.AddStack && StackCount < Effect.MaxStackCount)
        {
            StackCount++;
        }
    }
    public void RemoveStack()
    {
        if (StackCount > 0)
        {
            StackCount--;
        }
    }
    public void SetStackCount(int count)
    {
        StackCount = Mathf.Clamp(count, 0, Effect.MaxStackCount);
    }
    public bool IsAtMaxStacks()
    {
        return StackCount >= Effect.MaxStackCount;
    }

    public void CalculateAllModifiers()
    {
        ModifiedAttributes.Clear();

        foreach (var modInfo in Effect.Modifiers)
        {
            bool foundAttribute;
            float magnitudeValue = modInfo.ModifierMagnitude.CalculateMagnitude(this, out foundAttribute);

            if (!foundAttribute)
                continue;

            GameplayEffectModifiedAttribute modified = new GameplayEffectModifiedAttribute
            {
                TargetAttributeRef = modInfo.TargetAttribute,
                TotalMagnitude = magnitudeValue,
                ModifierType = modInfo.ModifierOperation
            };
            ModifiedAttributes.Add(modified);
        }
    }

    /// <summary>
    /// Retrieves the aggregated/current attribute value or snapshot if present.
    /// </summary>
    public float GetCapturedAttributeValue(GameplayEffectAttributeCaptureDefinition captureDef, out bool foundAttribute)
    {
        // If snapshot is used...
        if (captureDef.IsSnapshot() && snapshotData.ContainsKey(captureDef))
        {
            foundAttribute = true;
            return snapshotData[captureDef];
        }

        GameObject relevantActor = (captureDef.GetAttributeSource() == GameplayEffectAttributeCaptureSource.Source)
            ? sourceActor
            : targetActor;

        foundAttribute = false;
        if (relevantActor == null)
        {
            return 0f;
        }

        // Acquire final/current aggregated value from ASC
        var asc = relevantActor.GetComponent<AbilitySystemComponent>();
        if (asc == null) return 0f;

        return asc.GetAttributeValue(captureDef.GetGameplayAttributeReference(), out foundAttribute);
    }

    /// <summary>
    /// Similar to GetCapturedAttributeValue, but retrieves the base value rather than the current.
    /// Useful for AttributeBaseValue or computing bonus.
    /// </summary>
    public float GetCapturedAttributeBaseValue(GameplayEffectAttributeCaptureDefinition captureDef, out bool foundAttribute)
    {
        // If snapshot logic is relevant, do it here as well
        // For now, we do dynamic fetch
        GameObject relevantActor = (captureDef.GetAttributeSource() == GameplayEffectAttributeCaptureSource.Source)
            ? sourceActor
            : targetActor;

        foundAttribute = false;
        if (relevantActor == null)
        {
            return 0f;
        }

        var asc = relevantActor.GetComponent<AbilitySystemComponent>();
        if (asc == null) return 0f;

        return asc.GetAttributeBaseValue(captureDef.GetGameplayAttributeReference(), out foundAttribute);
    }

    private void SnapshotAttribute(GameplayEffectAttributeCaptureDefinition captureDef)
    {
        bool found;
        float currentValue = GetCapturedAttributeValueDynamic(captureDef, out found);
        if (found)
        {
            snapshotData[captureDef] = currentValue;
        }
    }

    private float GetCapturedAttributeValueDynamic(GameplayEffectAttributeCaptureDefinition captureDef, out bool foundAttribute)
    {
        foundAttribute = false;
        GameObject relevantActor = (captureDef.GetAttributeSource() == GameplayEffectAttributeCaptureSource.Source)
            ? sourceActor
            : targetActor;

        if (relevantActor == null)
            return 0f;

        var asc = relevantActor.GetComponent<AbilitySystemComponent>();
        if (asc == null)
            return 0f;

        return asc.GetAttributeValue(captureDef.GetGameplayAttributeReference(), out foundAttribute);
    }

    public void ApplyPeriodicTick()
    {
        // For each modified attr, apply the magnitude as a "one-time" effect to base
        // or do advanced logic. This is your design choice.
        foreach (var modifiedAttr in ModifiedAttributes)
        {
            bool foundAttr;
            var asc = targetActor.GetComponent<AbilitySystemComponent>();
            float oldBase = asc.GetAttributeBaseValue(modifiedAttr.TargetAttributeRef, out foundAttr);
            if (!foundAttr) continue;

            float newBase = oldBase + modifiedAttr.TotalMagnitude;
            asc.SetAttributeBaseValue(modifiedAttr.TargetAttributeRef, newBase);
        }
    }
}

[Serializable]
public struct GameplayEffectModifiedAttribute
{
    /// <summary>
    /// The AttributeReference identifying which attribute to modify (via reflection).
    /// </summary>
    public AttributeReference TargetAttributeRef;

    /// <summary>
    /// The final computed "magnitude" for the effect's change (e.g. +10).
    /// </summary>
    public float TotalMagnitude;

    /// <summary>
    /// The ModifierType (AddBase, MultiplyAdditive, etc.) that tells the attribute how to incorporate this magnitude.
    /// </summary>
    public ModifierType ModifierType;
}



[Serializable]
public class AttributeBasedFloat
{
    [SerializeField] private float coefficient = 1f;
    [SerializeField] private float preMultiplyAdditiveValue = 0f;
    [SerializeField] private float postMultiplyAdditiveValue = 0f;

    [SerializeField] public GameplayEffectAttributeCaptureDefinition backingAttribute;
    [SerializeField] private AnimationCurve attributeCurve;
    [SerializeField] private AttributeBasedFloatCalculationType attributeCalculationType = AttributeBasedFloatCalculationType.AttributeMagnitude;

    [SerializeField] private GameplayTagContainer sourceTagFilter;
    [SerializeField] private GameplayTagContainer targetTagFilter;

    public AttributeBasedFloat()
    {
        coefficient = 1f;
        preMultiplyAdditiveValue = 0f;
        postMultiplyAdditiveValue = 0f;
        backingAttribute = new GameplayEffectAttributeCaptureDefinition();
        attributeCalculationType = AttributeBasedFloatCalculationType.AttributeMagnitude;
        attributeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    }

    public float CalculateMagnitude(GameplayEffectSpec relevantSpec, out bool foundAttribute)
    {
        // We'll fetch the aggregator-based current value from the spec
        float currentVal = relevantSpec.GetCapturedAttributeValue(backingAttribute, out foundAttribute);
        if (!foundAttribute)
        {
            Debug.LogWarning($"[AttributeBasedFloat] Could not find current attribute for {backingAttribute.GetGameplayAttributeReference()}");
            return 0f;
        }

        float finalInputValue = currentVal; // default to "AttributeMagnitude" behavior

        switch (attributeCalculationType)
        {
            case AttributeBasedFloatCalculationType.AttributeMagnitude:
            {
                // We already have the aggregator-based currentVal
                finalInputValue = currentVal;
                break;
            }
            case AttributeBasedFloatCalculationType.AttributeBaseValue:
            {
                // We want to retrieve the base value
                float baseVal = relevantSpec.GetCapturedAttributeBaseValue(backingAttribute, out bool foundBase);
                if (!foundBase)
                {
                    Debug.LogWarning($"[AttributeBasedFloat] Could not find base attribute for {backingAttribute.GetGameplayAttributeReference()}");
                    baseVal = 0f;
                }
                finalInputValue = baseVal;
                break;
            }
            case AttributeBasedFloatCalculationType.AttributeBonusMagnitude:
            {
                // "Bonus" = currentVal - baseVal
                float baseVal = relevantSpec.GetCapturedAttributeBaseValue(backingAttribute, out bool foundBase2);
                if (!foundBase2)
                {
                    Debug.LogWarning($"[AttributeBasedFloat] Could not find base attribute for {backingAttribute.GetGameplayAttributeReference()}");
                    baseVal = 0f;
                }
                float bonus = currentVal - baseVal;
                finalInputValue = bonus;
                break;
            }
        }

        // Now apply preMultiplyAdditiveValue, coefficient, postMultiplyAdditiveValue, and curve
        float preAdd = finalInputValue + preMultiplyAdditiveValue;
        float scaledValue = preAdd * coefficient;
        float finalValue = scaledValue + postMultiplyAdditiveValue;

        if (attributeCurve != null && attributeCurve.keys.Length > 0)
        {
            finalValue = attributeCurve.Evaluate(finalValue);
        }

        Debug.Log($"[AttributeBasedFloat] CalculationType={attributeCalculationType}, " +
                  $"CurrentVal={currentVal}, PreAdd={preMultiplyAdditiveValue}, Coefficient={coefficient}, " +
                  $"PostAdd={postMultiplyAdditiveValue}, FinalValue={finalValue}");

        return finalValue;
    }


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
