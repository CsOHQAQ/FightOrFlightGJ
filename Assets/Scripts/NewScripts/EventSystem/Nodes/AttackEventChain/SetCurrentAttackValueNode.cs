using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCurrentAttackValueNode : IEventNode
{
    public int Priority => 10000;
    public AttributeReference AttackAttributeReference;
    
    public SetCurrentAttackValueNode(AttributeReference attackAttributeReference)
    {
        AttackAttributeReference = attackAttributeReference;
    }

    public void Process(EventContext context) {

        //Debug.Log("----------------------------------------------------------------------------------------------------");
        if (context == null || context.AttackInfo == null) return;
        
        bool found=false;
        
        context.AttackInfo.BaseDamage = context.Source.GetAbilitySystemComponent().GetAttributeValue(AttackAttributeReference,out found);
        Debug.Log("Attack Info Damage is Set: " + context.AttackInfo.BaseDamage);
        //context.AttackInfo;
        //Target should contain ICharacter at this point now
        //ICharacter character = context.Target as ICharacter;
        //context.HitData.FinalDamage = context.AttackInfo.BaseDamage;
        //character.TakeDamage(context.HitData.FinalDamage);
        //Probably should be added to a new Node after this:
        //context.Target.OnHit(context.HitData);
    }
}
