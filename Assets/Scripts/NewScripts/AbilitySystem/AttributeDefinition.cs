using UnityEngine;

[CreateAssetMenu(fileName = "AttributeDefinition", menuName = "AbilitySystem/AttributeDefinition")]
public class AttributeDefinition : ScriptableObject
{
    public string AttributeName;
    public float BaseValue = 0f;
    //public float MinValue = float.NegativeInfinity;
    //public float MaxValue = float.PositiveInfinity;
}
