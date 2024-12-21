using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GameplayAttribute
{
    [SerializeField] private float baseValue = 0f;
    private readonly List<AttributeModifier> modifiers = new List<AttributeModifier>();

    public float BaseValue
    {
        get => baseValue;
        set => baseValue = value;
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
                        multiplyAdditive += modifier.Value; // Additive multiplier
                        break;
                    case ModifierType.DivideAdditive:
                        divideAdditive *= 1 + modifier.Value; // Treat as a percentage
                        break;
                    case ModifierType.MultiplyCompound:
                        multiplyCompound *= 1 + modifier.Value; // Compound multiplier
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
        modifiers.Add(modifier);
    }

    public void RemoveModifier(AttributeModifier modifier)
    {
        modifiers.Remove(modifier);
    }

    public void ClearModifiersFromSource(object source)
    {
        modifiers.RemoveAll(modifier => modifier.Source == source);
    }
}
