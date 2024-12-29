using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SetCurrentAttackValueNodeNode", menuName = "EventNodes/AttackEventChain/SetCurrentAttackValueNodeNode")]
public class SetCurrentAttackValueNodeAsset : ScriptableEventNode<EventContext>
{
    public AttributeReference AttackAttributeReference;

    public override IEventNode<EventContext> CreateNodeInstance()
    {
        //SetCurrentAttackValueNodeAsset node = ;
        if (AttackAttributeReference == null)
        {
            Debug.LogError("AttackAttributeReference is Not Set");
        }
        //node.AttackAttributeReference = AttackAttributeReference;
        return new SetCurrentAttackValueNode(AttackAttributeReference);
    }
}
