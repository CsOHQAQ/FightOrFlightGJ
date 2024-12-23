using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySystemComponent : MonoBehaviour
{
    // This could reference one or more AttributeSet objects
    [SerializeField] 
    private AttributeSet attributeSet;

    // Active specs keyed by an int handle
    private Dictionary<int, GameplayEffectSpec> activeEffectSpecs = new Dictionary<int, GameplayEffectSpec>();
    private int nextEffectSpecID = 1;

    // Event for attribute changes, if you still want direct notifications
    public event Action<AttributeReference, float, float> OnAttributeChanged;

    private void Awake() {
        attributeSet = GetComponent<AttributeSet>();
    }

    //-----------------------------
    // 1) Public Apply Methods
    //-----------------------------
    public GameplayEffectSpecHandle ApplyEffectToSelf(GameplayEffect effect, float effectLevel)
    {
        return ApplyEffectInternal(effect, effectLevel, this.gameObject, this.gameObject);
    }

    public GameplayEffectSpecHandle ApplyEffectToTarget(GameplayEffect effect, float effectLevel, GameObject target)
    {
        return ApplyEffectInternal(effect, effectLevel, this.gameObject, target);
    }

    //-----------------------------
    // 2) Internal Apply Logic
    //-----------------------------
    private GameplayEffectSpecHandle ApplyEffectInternal(GameplayEffect effect, float effectLevel, GameObject source, GameObject target)
    {
        if (effect == null)
        {
            Debug.LogWarning("ApplyEffectInternal failed: No effect provided.");
            return new GameplayEffectSpecHandle { HandleID = 0 };
        }

        // 1. Create spec
        var spec = new GameplayEffectSpec(effect, effectLevel, source, target);
        // 2. Calculate all modifiers
        spec.CalculateAllModifiers();

        // 3. Store it with a new handle
        int handleID = nextEffectSpecID++;
        activeEffectSpecs[handleID] = spec;

        var handle = new GameplayEffectSpecHandle { HandleID = handleID };

        // 4. If instant, apply base changes directly and optionally remove
        if (effect.DurationPolicy == EDurationPolicy.Instant)
        {
            ExecuteEffectInstant(spec);
            // remove from dictionary if truly one-shot
            if (activeEffectSpecs.ContainsKey(handleID))
            {
                activeEffectSpecs.Remove(handleID);
            }
        }
        else
        {
            // 5. Duration/Infinite => add modifiers to attributes, store spec for future removal
            ExecuteEffectOngoing(spec);
        }

        return handle;
    }

    //-----------------------------
    // 3) Execution Logic
    //-----------------------------
    private void ExecuteEffectInstant(GameplayEffectSpec spec)
    {
        // Typically modifies base value once or “executes” a one-shot effect
        foreach (var modifiedAttr in spec.ModifiedAttributes)
        {
            ApplyInstantBaseChange(modifiedAttr);
        }
    }

    private void ExecuteEffectOngoing(GameplayEffectSpec spec)
    {
        // Add each modified attribute as an AttributeModifier, so the attribute’s CurrentValue is influenced
        // When the effect expires or is removed, we remove those modifiers
        foreach (var modifiedAttr in spec.ModifiedAttributes)
        {
            AddOngoingModifier(spec, modifiedAttr);
        }
    }

    //-----------------------------
    // 4) Instant: Base Value Changes
    //-----------------------------
    private void ApplyInstantBaseChange(GameplayEffectModifiedAttribute modifiedAttr)
    {
        // This means we do a one-time adjustment to the base value
        bool foundAttr;
        float oldBase = GetAttributeBaseValue(modifiedAttr.TargetAttributeRef, out foundAttr);
        if (!foundAttr) return;

        float newBase = oldBase;
        switch (modifiedAttr.ModifierType)
        {
            case ModifierType.AddBase:
                newBase = oldBase + modifiedAttr.TotalMagnitude;
                break;
            // Could handle override, multiply, etc. differently if you wish
            default:
                newBase = oldBase + modifiedAttr.TotalMagnitude;
                break;
        }

        SetAttributeBaseValue(modifiedAttr.TargetAttributeRef, newBase);
        // Fire an event if you want
        OnAttributeChanged?.Invoke(modifiedAttr.TargetAttributeRef, oldBase, newBase);
    }

    //-----------------------------
    // 5) Ongoing: Attach AttributeModifier
    //-----------------------------
    private void AddOngoingModifier(GameplayEffectSpec spec, GameplayEffectModifiedAttribute modifiedAttr)
    {
        // We create an AttributeModifier object
        var mod = new AttributeModifier(modifiedAttr.TotalMagnitude, modifiedAttr.ModifierType, source: spec);

        // Then we add it to the target attribute’s list of modifiers
        bool found;
        var gameplayAttribute = GetGameplayAttributeObject(modifiedAttr.TargetAttributeRef, out found);
        if (!found) return;

        gameplayAttribute.AddModifier(mod);

        // Optionally we can do a “OnAttributeChanged” event if we want immediate feedback
        // float oldCurrent = ???;
        // float newCurrent = gameplayAttribute.CurrentValue;
        // OnAttributeChanged?.Invoke(modifiedAttr.TargetAttributeRef, oldCurrent, newCurrent);
    }

    //-----------------------------
    // 6) Remove/Expire
    //-----------------------------
    private void RemoveOngoingModifier(GameplayEffectSpec spec)
    {
        // For each modified attribute in spec, remove the attribute modifier we added
        foreach (var modifiedAttr in spec.ModifiedAttributes)
        {
            bool found;
            var gameplayAttribute = GetGameplayAttributeObject(modifiedAttr.TargetAttributeRef, out found);
            if (found)
            {
                // We remove all modifiers from this “spec” source
                gameplayAttribute.ClearModifiersFromSource(spec);
            }
        }
    }

    //-----------------------------
    // 7) Update for Duration/Periodic
    //-----------------------------
    private void Update()
    {
        List<int> finishedEffects = null;

        foreach (var kvp in activeEffectSpecs)
        {
            int handleID = kvp.Key;
            GameplayEffectSpec spec = kvp.Value;

            if (spec.Effect.DurationPolicy == EDurationPolicy.HasDuration)
            {
                float elapsed = Time.time - spec.StartTime;
                if (elapsed > spec.Effect.Duration)
                {
                    // effect is done, remove from dictionary
                    if (finishedEffects == null) 
                        finishedEffects = new List<int>();
                    finishedEffects.Add(handleID);

                    // Also remove the ongoing modifiers
                    RemoveOngoingModifier(spec);
                    continue;
                }

                // If periodic, you might recalc or re-apply in certain intervals
                if (spec.Effect.IsPeriodic)
                {
                    // e.g. check nextTickTime, re-calc, re-apply or handle aggregator
                }
            }
            else if (spec.Effect.DurationPolicy == EDurationPolicy.Instant)
            {
                // Typically remove it right away if we haven't
                if (finishedEffects == null) 
                    finishedEffects = new List<int>();
                finishedEffects.Add(handleID);
            }
            // if infinite => do nothing, only removed by user
        }

        if (finishedEffects != null)
        {
            foreach (int handleId in finishedEffects)
            {
                activeEffectSpecs.Remove(handleId);
            }
        }
    }

    //-----------------------------
    // 8) Utility for Looking Up/Removing Effects
    //-----------------------------
    public GameplayEffectSpec GetGameplayEffectSpec(GameplayEffectSpecHandle handle)
    {
        if (activeEffectSpecs.TryGetValue(handle.HandleID, out var spec))
            return spec;
        return null;
    }

    public void RemoveEffectSpec(GameplayEffectSpecHandle handle)
    {
        if (activeEffectSpecs.TryGetValue(handle.HandleID, out var spec))
        {
            // remove modifiers
            RemoveOngoingModifier(spec);
        }
        activeEffectSpecs.Remove(handle.HandleID);
    }

    //-----------------------------
    // 9) Reflection-based Get/Set for base & current
    //    BUT we no longer directly set current for ongoing
    //-----------------------------
    public float GetAttributeValue(AttributeReference attributeRef, out bool foundAttribute)
    {
        foundAttribute = false;
        if (!attributeRef.IsValid()) return 0f;

        var fieldInfo = attributeRef.GetFieldInfo();
        if (fieldInfo == null) return 0f;

        var gameplayAttribute = fieldInfo.GetValue(attributeSet) as GameplayAttribute;
        if (gameplayAttribute == null) return 0f;

        foundAttribute = true;
        return gameplayAttribute.CurrentValue;
    }


    public float GetAttributeBaseValue(AttributeReference attributeRef, out bool foundAttribute)
    {
        foundAttribute = false;
        if (!attributeRef.IsValid()) return 0f;

        var fieldInfo = attributeRef.GetFieldInfo();
        if (fieldInfo == null) return 0f;

        var gameplayAttribute = fieldInfo.GetValue(attributeSet) as GameplayAttribute;
        if (gameplayAttribute == null) return 0f;

        foundAttribute = true;
        return gameplayAttribute.BaseValue;
    }

    public void SetAttributeBaseValue(AttributeReference attributeRef, float newBaseValue)
    {
        if (!attributeRef.IsValid()) return;
        var fieldInfo = attributeRef.GetFieldInfo();
        if (fieldInfo == null) return;

        var gameplayAttribute = fieldInfo.GetValue(attributeSet) as GameplayAttribute;
        if (gameplayAttribute == null) return;

        gameplayAttribute.BaseValue = newBaseValue;
    }

    //-----------------------------
    // 10) Access the underlying GameplayAttribute
    //     so we can add/remove modifiers
    //-----------------------------
    private GameplayAttribute GetGameplayAttributeObject(AttributeReference attributeRef, out bool found)
    {
        found = false;
        if (!attributeRef.IsValid()) return null;

        var fieldInfo = attributeRef.GetFieldInfo();
        if (fieldInfo == null) return null;

        var gameplayAttribute = fieldInfo.GetValue(attributeSet) as GameplayAttribute;
        if (gameplayAttribute != null)
        {
            found = true;
        }
        return gameplayAttribute;
    }
}
