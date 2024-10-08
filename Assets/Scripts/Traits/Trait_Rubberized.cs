using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trait_Rubberized : Trait_Behavior
{
    public override int BehaviorModifier()
    {
        Debug.Log("Trait: Rubberized");

        return 0;
    }
}
