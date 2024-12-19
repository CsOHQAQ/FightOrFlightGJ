using System;

[Serializable]
public struct GameplayEffectModifier
{
    public string TargetAttribute;
    public GameplayModifierType ModifierType; // Add, Multiply, Override
    public float Magnitude; // Or could be replaced by a Calculation Class later
}

public enum GameplayModifierType
{
    Add,
    Multiply,
    Override
}
