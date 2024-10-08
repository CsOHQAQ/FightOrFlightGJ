using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trait_Behavior : MonoBehaviour
{
    /// <summary>
    /// Should be overriden by trait functions
    /// </summary>
    /// <returns>Does some behavior and returns an int based on the attack power</returns>
    public abstract int BehaviorModifier();
}
