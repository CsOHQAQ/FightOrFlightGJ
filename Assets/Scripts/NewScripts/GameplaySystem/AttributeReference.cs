using UnityEngine;
using System;
using System.Reflection;

[CreateAssetMenu(menuName = "AbilitySystem/AttributeReference")]
public class AttributeReference : ScriptableObject
{
    #if UNITY_EDITOR
    [SerializeField] private UnityEditor.MonoScript attributeSetScript;
    #endif
    [SerializeField] private string attributeFieldName;

    public bool IsValid()
    {
        #if UNITY_EDITOR
        return attributeSetScript != null && !string.IsNullOrEmpty(attributeFieldName);
        #else
        return !string.IsNullOrEmpty(attributeFieldName);
        #endif
    }

    public Type GetAttributeSetType()
    {
        #if UNITY_EDITOR
        return attributeSetScript != null ? attributeSetScript.GetClass() : null;
        #else
        return null; // Or provide an alternative mechanism in runtime
        #endif
    }

    public FieldInfo GetFieldInfo()
    {
        var type = GetAttributeSetType();
        if (type == null) return null;
        return type.GetField(attributeFieldName, BindingFlags.Public | BindingFlags.Instance);
    }
}
