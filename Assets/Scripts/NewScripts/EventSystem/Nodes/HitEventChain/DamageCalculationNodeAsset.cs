using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DamageCalculationNode", menuName = "EventNodes/HitEventChain/DamageCalculationNode")]
public class DamageCalculationNodeAsset : ScriptableEventNode
{


    public override IEventNode CreateNodeInstance()
    {
        return new DamageCalculationNode();
    }
}