using System;
using UnityEngine;

/// <summary>
/// Enumeration for options of where to capture gameplay attributes from for gameplay effects.
/// </summary>
public enum GameplayEffectAttributeCaptureSource
{
    /// <summary>
    /// Source (caster) of the gameplay effect.
    /// </summary>
    Source,

    /// <summary>
    /// Target (recipient) of the gameplay effect.
    /// </summary>
    Target
}

/// <summary>
/// Struct defining gameplay attribute capture options for gameplay effects.
/// </summary>
[Serializable]
public class GameplayEffectAttributeCaptureDefinition
{
    /// <summary>
    /// Gameplay attribute to capture.
    /// </summary>
    [SerializeField] private GameplayAttribute attributeToCapture;

    /// <summary>
    /// Source of the gameplay attribute.
    /// </summary>
    [SerializeField] private GameplayEffectAttributeCaptureSource attributeSource = GameplayEffectAttributeCaptureSource.Source;

    /// <summary>
    /// Whether the attribute should be snapshotted or not.
    /// </summary>
    [SerializeField] private bool bSnapshot = false;

    // Default constructor
    public GameplayEffectAttributeCaptureDefinition()
    {
        attributeSource = GameplayEffectAttributeCaptureSource.Source;
        bSnapshot = false;
    }

    // Parameterized constructor
    public GameplayEffectAttributeCaptureDefinition(GameplayAttribute inAttribute, GameplayEffectAttributeCaptureSource inSource, bool inSnapshot)
    {
        attributeToCapture = inAttribute;
        attributeSource = inSource;
        bSnapshot = inSnapshot;
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public override bool Equals(object obj)
    {
        if (obj is GameplayEffectAttributeCaptureDefinition other)
        {
            return attributeToCapture.Equals(other.attributeToCapture) &&
                   attributeSource == other.attributeSource &&
                   bSnapshot == other.bSnapshot;
        }
        return false;
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(GameplayEffectAttributeCaptureDefinition left, GameplayEffectAttributeCaptureDefinition right)
    {
        return !(left == right);
    }

    public static bool operator ==(GameplayEffectAttributeCaptureDefinition left, GameplayEffectAttributeCaptureDefinition right)
    {
        if (left is null || right is null)
        {
            return ReferenceEquals(left, right);
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Generates a hash code for the capture definition.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(attributeToCapture, attributeSource, bSnapshot);
    }

    /// <summary>
    /// Converts the object to a simple string representation.
    /// </summary>
    public override string ToString()
    {
        return $"Attribute: {attributeToCapture}, Source: {attributeSource}, Snapshot: {bSnapshot}";
    }
}
