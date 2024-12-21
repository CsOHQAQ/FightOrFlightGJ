
using UnityEngine;
using System;

/// <summary>
/// Base AttributeSet class similar to UAttributeSet in Unreal. 
/// Users can subclass this and add GameplayAttribute fields for their attributes.
/// The virtual methods allow custom logic (like clamping or triggering events) before/after changes.
/// </summary>
public class AttributeSet : MonoBehaviour
{
    /// <summary>
    /// Called just before any modification happens to an attribute's current value.
    /// NewValue can be changed (e.g., to clamp it) before it's applied.
    /// In Unreal: PreAttributeChange(FGameplayAttribute Attribute, float& NewValue)
    /// </summary>
    /// <param name="attributeName">The name of the attribute that will be modified.</param>
    /// <param name="newValue">Reference to the new value that is about to be set. This can be modified to enforce rules like clamping.</param>
    public virtual void PreAttributeChange(string attributeName, ref float newValue)
    {
        // Pseudocode:
        // For example, if attributeName == "Health"
        //     newValue = Mathf.Clamp(newValue, 0, MaxHealth);
        // This ensures health doesn't drop below 0 or exceed max.
    }

    /// <summary>
    /// Called just after any modification happens to an attribute's current value.
    /// In Unreal: PostAttributeChange(FGameplayAttribute Attribute, float OldValue, float NewValue)
    /// </summary>
    /// <param name="attributeName">The name of the attribute that was modified.</param>
    /// <param name="oldValue">The old attribute value before the change.</param>
    /// <param name="newValue">The new attribute value after the change.</param>
    public virtual void PostAttributeChange(string attributeName, float oldValue, float newValue)
    {
        // Pseudocode:
        // If Health changed from 50 to 0, we could trigger a "OnDeath" event here.
        // Or if Strength increased, we could recalculate attack damage, etc.
    }

    /// <summary>
    /// Called just before any modification happens to an attribute's base value.
    /// This is analogous to PreAttributeBaseChange in Unreal.
    /// You can enforce clamping or other rules on the base value here, but should not trigger gameplay events.
    /// </summary>
    /// <param name="attributeName">The attribute whose base value will change.</param>
    /// <param name="newValue">Reference to the new base value. You can modify this to enforce constraints.</param>
    public virtual void PreAttributeBaseChange(string attributeName, ref float newValue)
    {
        // Pseudocode:
        // For base Health, maybe clamp it so baseValue never falls below 50 or above 200.
        // newValue = Mathf.Clamp(newValue, 50, 200);
    }

    /// <summary>
    /// Called just after any modification happens to an attribute's base value.
    /// This is analogous to PostAttributeBaseChange in Unreal.
    /// </summary>
    /// <param name="attributeName">The attribute whose base value changed.</param>
    /// <param name="oldValue">The old base value before the change.</param>
    /// <param name="newValue">The new base value after the change.</param>
    public virtual void PostAttributeBaseChange(string attributeName, float oldValue, float newValue)
    {
        // Pseudocode:
        // Maybe log or adjust dependent attributes. For example, if base Health increased,
        // we might also increase current health proportionally.
    }

    /// <summary>
    /// In Unreal, OnAttributeAggregatorCreated is called when an aggregator is created for an attribute.
    /// Aggregators combine multiple modifications over time.
    /// In Unity, you might not have the same aggregator concept. 
    /// If you do implement something like it, you can provide a hook here.
    ///
    /// Pseudocode:
    /// This could be used to set up evaluation metadata on an aggregator if you had a similar system.
    /// For now, let's just define an empty function or a placeholder.
    /// </summary>
    /// <param name="attributeName">The attribute for which aggregator is created.</param>
    /// <param name="aggregator">A hypothetical aggregator object that accumulates changes.</param>
    public virtual void OnAttributeAggregatorCreated(string attributeName, object aggregator)
    {
        // Pseudocode:
        // aggregator.EvaluationMetaData.SomeProperty = someValue;
    }

    // Additional utility methods or data initialization could be placed here.
}
