using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DamageCalculationNode", menuName = "EventNodes/HitEventChain/DamageCalculationNode")]
public class DamageCalculationNodeAsset : ScriptableEventNode<EventContext>
{


    public override IEventNode<EventContext> CreateNodeInstance()
    {
        return new DamageCalculationNode();
    }
}