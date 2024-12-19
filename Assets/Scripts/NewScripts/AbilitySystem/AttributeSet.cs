using System.Collections.Generic;
using UnityEngine;

public class AttributeSet
{
    private Dictionary<string, float> attributeValues = new Dictionary<string, float>();
    private Dictionary<string, AttributeDefinition> definitions = new Dictionary<string, AttributeDefinition>();

    public AttributeSet(IEnumerable<AttributeDefinition> attrs)
    {
        foreach (var def in attrs)
        {
            definitions[def.AttributeName] = def;
            attributeValues[def.AttributeName] = def.BaseValue;
        }
    }

    public float GetValue(string attributeName)
    {
        if (attributeValues.TryGetValue(attributeName, out float val))
            return val;
        return 0f;
    }

    public void SetValue(string attributeName, float value)
    {
        /*
        if (definitions.TryGetValue(attributeName, out AttributeDefinition def))
        {
            float clamped = Mathf.Clamp(value, def.MinValue, def.MaxValue);
            attributeValues[attributeName] = clamped;
        }
        */
    }

    public void ModifyValue(string attributeName, float delta)
    {
        float oldValue = GetValue(attributeName);
        SetValue(attributeName, oldValue + delta);
    }
}
