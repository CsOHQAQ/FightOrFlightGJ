using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TRAIT_TARGET
{
    NORMAL,
    FLYING,
    GROUND
}


[CreateAssetMenu(fileName = "Type")]
public class Trait_Type : SerializedScriptableObject
{
    [OdinSerialize]
    public Dictionary<TRAIT_TARGET, float> TypeBehavior;
}
