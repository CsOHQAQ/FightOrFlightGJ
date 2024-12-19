using UnityEngine;

public interface IAbilitySystemComponent
{
    /// <summary>
    /// Gets the current value of a specified attribute.
    /// </summary>
    float GetAttributeValue(string attributeName);

    /// <summary>
    /// Modifies an attribute by a given amount. Positive for increments, negative for decrements.
    /// Implementations should ensure min/max clamping.
    /// </summary>
    void ModifyAttributeValue(string attributeName, float amount);

    /// <summary>
    /// Sets an attribute to a specific value, bypassing calculations. Usually used sparingly.
    /// </summary>
    void SetAttributeValue(string attributeName, float newValue);

    /// <summary>
    /// Applies a GameplayEffect to this AbilitySystemComponent.
    /// Returns a handle or ID that can be used to remove or query the effect.
    /// </summary>
    GameplayEffectHandle ApplyGameplayEffect(GameplayEffect effect, IAbilitySystemComponent instigator);

    /// <summary>
    /// Removes a GameplayEffect by handle if active.
    /// </summary>
    void RemoveGameplayEffect(GameplayEffectHandle handle);

    /// <summary>
    /// Event triggered when an attribute changes.
    /// Could be a delegate or event, for now we leave it as a placeholder.
    /// </summary>
    event System.Action<string, float, float> OnAttributeChanged;
}
