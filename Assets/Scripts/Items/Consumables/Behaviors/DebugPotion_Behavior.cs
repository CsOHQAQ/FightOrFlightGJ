using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPotion_Behavior : ConsumableItem_Behavior
{
    public override void ConsumableBehavior()
    {
        Debug.Log("The debug potion is consumed");
    }
}
