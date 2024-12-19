using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySystemComponent : MonoBehaviour, IAbilitySystemComponent
{
    // Attribute management
    private AttributeSet attributeSet;
    // Active gameplay effects keyed by their handle ID
    private Dictionary<int, ActiveGameplayEffect> activeEffects = new Dictionary<int, ActiveGameplayEffect>();
    private int nextHandleID = 1;

    public event Action<string, float, float> OnAttributeChanged;

    public void InitializeAttributes(IEnumerable<AttributeDefinition> definitions)
    {
        attributeSet = new AttributeSet(definitions);
    }

    // IAbilitySystemComponent interface implementations
    public float GetAttributeValue(string attributeName)
    {
        if (attributeSet == null)
        {
            Debug.LogWarning("AttributeSet not initialized.");
            return 0f;
        }
        return attributeSet.GetValue(attributeName);
    }

    public void ModifyAttributeValue(string attributeName, float amount)
    {
        if (attributeSet == null)
        {
            Debug.LogWarning("AttributeSet not initialized.");
            return;
        }

        float oldVal = attributeSet.GetValue(attributeName);
        attributeSet.ModifyValue(attributeName, amount);
        float newVal = attributeSet.GetValue(attributeName);

        if (Mathf.Abs(oldVal - newVal) > Mathf.Epsilon)
        {
            OnAttributeChanged?.Invoke(attributeName, oldVal, newVal);
        }
    }

    public void SetAttributeValue(string attributeName, float newValue)
    {
        if (attributeSet == null)
        {
            Debug.LogWarning("AttributeSet not initialized.");
            return;
        }

        float oldVal = attributeSet.GetValue(attributeName);
        attributeSet.SetValue(attributeName, newValue);
        float updatedVal = attributeSet.GetValue(attributeName);

        if (Mathf.Abs(oldVal - updatedVal) > Mathf.Epsilon)
        {
            OnAttributeChanged?.Invoke(attributeName, oldVal, updatedVal);
        }
    }

    public GameplayEffectHandle ApplyGameplayEffect(GameplayEffect effect, IAbilitySystemComponent instigator)
    {
        var handle = new GameplayEffectHandle { HandleID = nextHandleID++ };
        var active = new ActiveGameplayEffect
        {
            Handle = handle,
            SourceEffect = effect,
            Target = this,
            Instigator = instigator,
            StartTime = Time.time,
            Duration = effect.IsInfinite ? float.PositiveInfinity : effect.Duration
        };

        activeEffects[handle.HandleID] = active;

        // Apply the modifiers immediately for simplicity
        ApplyModifiers(active);

        return handle;
    }

    public void RemoveGameplayEffect(GameplayEffectHandle handle)
    {
        if (activeEffects.TryGetValue(handle.HandleID, out ActiveGameplayEffect active))
        {
            // If we had temporary changes to revert, we would do so here.
            // For now we assume instant changes that don't revert automatically.
            activeEffects.Remove(handle.HandleID);
        }
    }

    // Internal methods
    private void ApplyModifiers(ActiveGameplayEffect active)
    {
        foreach (var mod in active.SourceEffect.Modifiers)
        {
            ApplyModifierToTarget(mod, active.Target);
        }
    }

    private void ApplyModifierToTarget(GameplayEffectModifier mod, IAbilitySystemComponent target)
    {
        float oldValue = target.GetAttributeValue(mod.TargetAttribute);
        float newValue = oldValue;

        switch (mod.ModifierType)
        {
            case GameplayModifierType.Add:
                newValue += mod.Magnitude;
                break;
            case GameplayModifierType.Multiply:
                newValue *= mod.Magnitude;
                break;
            case GameplayModifierType.Override:
                newValue = mod.Magnitude;
                break;
        }

        target.SetAttributeValue(mod.TargetAttribute, newValue);
    }
}
