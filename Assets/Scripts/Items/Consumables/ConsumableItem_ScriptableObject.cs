using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Item", menuName = "Items/Consumable", order = 3)]
public class ConsumableItem_ScriptableObject : Item_ScriptableObject
{
    //Script that the consumable should call when the behavior is performed
    [SerializeField]
    private GameObject consumableBehaviorHolder;

    /// <summary>
    /// Function that is called by external classes to run the attached behavior
    /// </summary>
    public bool RunBehavior()
    {
        if (consumableBehaviorHolder != null)
        {
            consumableBehaviorHolder.GetComponent<ConsumableItem_Behavior>().ConsumableBehavior();

            return true;
        }

        return false;
    }
}
