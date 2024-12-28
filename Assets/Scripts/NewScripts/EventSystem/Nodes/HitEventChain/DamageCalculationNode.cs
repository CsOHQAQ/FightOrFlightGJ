using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCalculationNode : IEventNode<EventContext> {
    public int Priority => 10;

    public void Process(EventContext context) {
        if (context == null || context.AttackInfo == null || context.HitData == null) return;
        
        //Target should contain ICharacter at this point now
        ICharacter character = context.Target as ICharacter;
        context.HitData.FinalDamage = context.AttackInfo.BaseDamage;
        
        character.TakeDamage(context);
        //Probably should be added to a new Node after this:
        context.Target.OnHit(context.HitData);

        // Damage UI Related
        float finalDamage = context.HitData.FinalDamage;
        bool isCrit = context.HitData.WasCrit;
        Vector3 enemyPosition = context.Target != null 
            ? context.HitData.HitInfo.HitPoint
            : Vector3.zero;

        
        DamageNumberManager.Instance.ShowDamageNumber(enemyPosition, finalDamage, isCrit, context);
    }


}

