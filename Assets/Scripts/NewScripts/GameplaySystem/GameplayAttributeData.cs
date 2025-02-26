using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GameplayAttribute
{
    [SerializeField] private float baseValue = 0f;
    private readonly List<AttributeModifier> modifiers = new List<AttributeModifier>();

    public event Action<float, float> OnValueChanged;

    public float BaseValue
    {
        get => baseValue;
        set
        {
            if (baseValue == value) return;
            float oldValue = CurrentValue;
            baseValue = value;
            float newValue = CurrentValue;
            if (oldValue != newValue)
            {
                OnValueChanged?.Invoke(oldValue, newValue);
            }
        }
    }
    
    public float CurrentValue
    {
        get
        {
            float additiveSum = 0f;
            float multiplyAdditive = 1f;
            float divideAdditive = 1f;
            float multiplyCompound = 1f;
            float finalAdd = 0f;

            foreach (var modifier in modifiers)
            {
                switch (modifier.Type)
                {
                    case ModifierType.AddBase:
                        additiveSum += modifier.Value;
                        break;
                    case ModifierType.MultiplyAdditive:
                        multiplyAdditive += modifier.Value;
                        break;
                    case ModifierType.DivideAdditive:
                        divideAdditive *= 1 + modifier.Value;
                        break;
                    case ModifierType.MultiplyCompound:
                        multiplyCompound *= 1 + modifier.Value;
                        break;
                    case ModifierType.AddFinal:
                        finalAdd += modifier.Value;
                        break;
                }
            }

            float currentValue = ((baseValue + additiveSum) * multiplyAdditive / divideAdditive * multiplyCompound) + finalAdd;
            return currentValue;
        }
    }

    public void AddModifier(AttributeModifier modifier)
    {
        float oldValue = CurrentValue;
        modifiers.Add(modifier);
        float newValue = CurrentValue;
        if (oldValue != newValue)
        {
            OnValueChanged?.Invoke(oldValue, newValue);
        }
    }

    public void RemoveModifier(AttributeModifier modifier)
    {
        float oldValue = CurrentValue;
        bool removed = modifiers.Remove(modifier);
        if (removed)
        {
            float newValue = CurrentValue;
            if (oldValue != newValue)
            {
                OnValueChanged?.Invoke(oldValue, newValue);
            }
        }
    }

    public void ClearModifiersFromSource(object source)
    {
        float oldValue = CurrentValue;
        modifiers.RemoveAll(modifier => modifier.Source == source);
        float newValue = CurrentValue;
        if (oldValue != newValue)
        {
            OnValueChanged?.Invoke(oldValue, newValue);
        }
    }
}