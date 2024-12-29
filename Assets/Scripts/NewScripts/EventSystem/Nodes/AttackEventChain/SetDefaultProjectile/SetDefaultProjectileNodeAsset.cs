using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SetDefaultProjectileNode", menuName = "EventNodes/AttackEventChain/SetDefaultProjectileNode")]
public class SetDefaultProjectileNodeAsset : ScriptableEventNode<EventContext>
{
    [SerializeField]
    GameObject projectilePrefab;

        public override IEventNode<EventContext> CreateNodeInstance()
    {
        //SetCurrentAttackValueNodeAsset node = ;
        if (projectilePrefab == null)
        {
            Debug.LogError("AttackAttributeReference is Not Set");
        }
        //node.AttackAttributeReference = AttackAttributeReference;
        return new SetDefaultProjectileNode(projectilePrefab);
    }
}
