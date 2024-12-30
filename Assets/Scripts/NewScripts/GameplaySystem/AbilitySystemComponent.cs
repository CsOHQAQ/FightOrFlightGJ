using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySystemComponent : MonoBehaviour
{
    // This could reference one or more AttributeSet objects
    [SerializeField] 
    private AttributeSet attributeSet;
    public AttributeSet AttributeSet{get{return attributeSet;}}

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

        // Check for existing spec with the same effect
        foreach (var kvp in activeEffectSpecs)
        {
            GameplayEffectSpec existingSpec = kvp.Value;

            // Match effect and source for stacking
            if (existingSpec.Effect == effect && existingSpec.SourceActor == source)
            {
                if (effect.StackingPolicy == StackingPolicy.AddStack)
                {
                    if (!existingSpec.IsAtMaxStacks())
                    {
                        existingSpec.AddStack();
                        existingSpec.CalculateAllModifiers();
                    }
                    return new GameplayEffectSpecHandle { HandleID = kvp.Key };
                }
                else if (effect.StackingPolicy == StackingPolicy.RefreshDuration)
                {
                    existingSpec.StartTime = Time.time;
                    return new GameplayEffectSpecHandle { HandleID = kvp.Key };
                }
                else if (effect.StackingPolicy == StackingPolicy.AggregateDuration)
                {
                    existingSpec.SetStackCount(existingSpec.StackCount + 1);
                    existingSpec.CalculateAllModifiers();
                    existingSpec.StartTime = Time.time; // Optional: adjust duration logic
                    return new GameplayEffectSpecHandle { HandleID = kvp.Key };
                }
            }
        }

        // If no existing spec matches, create a new one
        var spec = new GameplayEffectSpec(effect, effectLevel, source, target);
        spec.CalculateAllModifiers();

        int handleID = nextEffectSpecID++;
        activeEffectSpecs[handleID] = spec;

        var handle = new GameplayEffectSpecHandle { HandleID = handleID };

        if (effect.DurationPolicy == DurationPolicy.Instant || effect.Duration ==0f)
        {
            ExecuteEffectInstant(spec);
            activeEffectSpecs.Remove(handleID);
        }
        else
        {
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

            if (spec.Effect.DurationPolicy != DurationPolicy.Instant)
            {
                float elapsed = Time.time - spec.StartTime;

                // Handle periodic logic
                if (spec.Effect.IsPeriodic && Time.time >= spec.NextTickTime)
                {
                    spec.ApplyPeriodicTick();
                    spec.NextTickTime += spec.Effect.Period; // Schedule the next tick
                }

                // Check if the effect has expired
                if (elapsed > spec.Effect.Duration)
                {
                    if (finishedEffects == null)
                        finishedEffects = new List<int>();
                    finishedEffects.Add(handleID);

                    // Also remove the ongoing modifiers
                    RemoveOngoingModifier(spec);
                    continue;
                }
            }
            else if (spec.Effect.DurationPolicy == DurationPolicy.Instant)
            {
                if (finishedEffects == null)
                    finishedEffects = new List<int>();
                finishedEffects.Add(handleID);
            }
        }

        // Clean up finished effects
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

    public void RemoveActiveGameplayEffectBySourceEffect(GameplayEffect gameplayEffect, AbilitySystemComponent instigator = null, int stacksToRemove = -1)
    {
        if (gameplayEffect == null)
        {
            Debug.LogWarning("RemoveActiveGameplayEffectBySourceEffect failed: No gameplay effect provided.");
            return;
        }

        foreach (var kvp in activeEffectSpecs)
        {
            int handleID = kvp.Key;
            GameplayEffectSpec spec = kvp.Value;

            if (spec.Effect != gameplayEffect)
                continue;

            if (instigator != null && spec.SourceActor.GetComponent<AbilitySystemComponent>() != instigator)
                continue;

            if (stacksToRemove == -1 || stacksToRemove >= spec.StackCount)
            {
                // Remove all stacks and the effect
                RemoveOngoingModifier(spec);
                activeEffectSpecs.Remove(handleID);
            }
            else
            {
                // Reduce the stack count
                spec.RemoveStack();
                spec.CalculateAllModifiers();
            }
        }
    }

    public void DebugPrintActiveEffects()
    {
        foreach (var kvp in activeEffectSpecs)
        {
            GameplayEffectSpec spec = kvp.Value;
            Debug.Log($"Effect: {spec.Effect.name}, Stacks: {spec.StackCount}");
        }
    }

}
