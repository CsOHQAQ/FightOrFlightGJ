using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCalculationNode : IEventNode {
    public int Priority => 10;

    public void Process(EventContext context) {
        if (context == null || context.AttackInfo == null || context.HitData == null) return;

        context.HitData.FinalDamage = context.AttackInfo.BaseDamage;
        
    }
}


[CreateAssetMenu(fileName = "DamageCalculationNode", menuName = "EventNodes/DamageCalculationNode")]
public class DamageCalculationNodeAsset : ScriptableEventNode
{


    public override IEventNode CreateNodeInstance()
    {
        return new DamageCalculationNode();
    }
}