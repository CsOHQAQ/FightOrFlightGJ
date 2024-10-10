using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotion_Behavior : ConsumableItem_Behavior
{
    [SerializeField]
    private float healthRestored;

    public override void ConsumableBehavior()
    {
        //throw new System.NotImplementedException();
        FindObjectOfType<PlayerStats>().TakeDamage((int)-healthRestored);
    }
}
