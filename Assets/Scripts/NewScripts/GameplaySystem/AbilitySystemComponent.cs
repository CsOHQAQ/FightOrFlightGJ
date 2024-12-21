using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySystemComponent : MonoBehaviour
{
    // Attribute management
    private AttributeSet attributeSet;
    // GameplayEffectSpec gameplay effects keyed by their handle ID
    private Dictionary<int, GameplayEffectSpec> effectSpecs = new Dictionary<int, GameplayEffectSpec>();
    private int nextHandleID = 1;

    public event Action<GameplayAttribute, float, float> OnAttributeChanged;


    // IAbilitySystemComponent interface implementations
    public float GetAttributeValue(AttributeReference attributeRef, out bool foundAttribute)
    {
        foundAttribute = false;
        if (!attributeRef.IsValid()) 
        {
            return 0f;
        }
        
        var fieldInfo = attributeRef.GetFieldInfo();
        if (fieldInfo == null)
        {
            
            return 0f;
        } 

        
        var attributeData = fieldInfo.GetValue(attributeSet) as GameplayAttribute;
        if (attributeData == null) 
        {
            
            return 0f;
        }
        foundAttribute = true;
        return attributeData.CurrentValue;
    }


}
