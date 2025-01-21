using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCalculationNode : IEventNode<EventContext>
{
    public int Priority => 10;

    // Could store a config for minMultiplier, maxMultiplier, etc.
    private float minRandomFactor = 0.925f;
    private float maxRandomFactor = 1.075f;

    public void Process(EventContext context)
    {
        if (context == null || context.AttackInfo == null || context.HitData == null) return;

        // 1) Base damage from context
        float baseDamage = context.AttackInfo.BaseDamage;

        // 2) Apply random factor
        float randomFactor = UnityEngine.Random.Range(minRandomFactor, maxRandomFactor);
        float finalDamage = baseDamage * randomFactor;

        // 3) If you have other multipliers (like critical hits, element advantage), do them here
        // e.g., finalDamage *= 2 if context.HitData.WasCrit?

        // 4) Assign finalDamage
        context.HitData.FinalDamage = finalDamage;

        // 5) Apply it to the target
        ICharacter character = context.Target as ICharacter;
        character.TakeDamage(context);

        // 6) Trigger OnHit
        context.Target.OnHit(context.HitData);

        if(character.Faction == Faction.ENEMY)
        {
            // 7) Damage UI
            Vector3 enemyPosition = (context.Target != null) 
                ? context.HitData.HitInfo.HitPoint
                : Vector3.zero;

            DamageNumberManager.Instance.ShowDamageNumber(
                enemyPosition,
                context.HitData.FinalDamage, 
                context.HitData.WasCrit,
                context
            );
        }

    }
}
