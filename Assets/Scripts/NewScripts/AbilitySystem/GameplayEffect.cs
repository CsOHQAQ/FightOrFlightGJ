using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameplayEffect", menuName = "AbilitySystem/GameplayEffect")]
public class GameplayEffect : ScriptableObject
{
    public List<GameplayEffectModifier> Modifiers = new List<GameplayEffectModifier>();
    public float Duration = 0f; // 0 or negative means instant; positive means timed effect
    public bool IsInfinite = false;

    // Stacking, periodic effects, etc. can be added later.
}

[Serializable]
public struct GameplayEffectHandle
{
    public int HandleID; // This could be a unique ID assigned when the effect is applied.
}