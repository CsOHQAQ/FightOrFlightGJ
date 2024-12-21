using System;

[Serializable]
public class AttributeModifier
{
    public float Value { get; private set; }
    public ModifierType Type { get; private set; }
    public object Source { get; private set; } // Optional: track what added the modifier

    public AttributeModifier(float value, ModifierType type, object source = null)
    {
        Value = value;
        Type = type;
        Source = source;
    }
}
public enum ModifierType
{
    AddBase,          // Add to BaseValue
    MultiplyAdditive, // Additive multipliers
    DivideAdditive,   // Additive divisors
    MultiplyCompound, // Compounded multipliers
    AddFinal          // Add to the final value
}