using UnityEngine;

public class ActiveGameplayEffect
{
    public GameplayEffectHandle Handle;
    public GameplayEffect SourceEffect;
    public IAbilitySystemComponent Target;
    public IAbilitySystemComponent Instigator;
    public float StartTime;
    public float Duration;

    // Future: Current stacks, periodic logic, etc.
}
