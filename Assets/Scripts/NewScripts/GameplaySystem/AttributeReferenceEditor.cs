using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(AttributeReference))]
public class AttributeReferenceEditor : Editor
{
    private AttributeReference reference;
    private string[] attributeNames = new string[0];
    private int selectedIndex = -1;

    void OnEnable()
    {
        reference = (AttributeReference)target;
        UpdateAttributeList();
    }

    void UpdateAttributeList()
    {
        var type = reference.GetAttributeSetType();
        if (type == null)
        {
            attributeNames = new string[0];
            selectedIndex = -1;
            return;
        }

        // 查找所有类型为GameplayAttribute的public实例字段
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(GameplayAttribute)).ToList();

        attributeNames = fields.Select(f => f.Name).ToArray();

        selectedIndex = Array.IndexOf(attributeNames, GetCurrentAttributeName());
        if (selectedIndex < 0 && attributeNames.Length > 0)
        {
            selectedIndex = 0;
            SetCurrentAttributeName(attributeNames[0]);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        var scriptProperty = serializedObject.FindProperty("attributeSetScript");
        EditorGUILayout.PropertyField(scriptProperty, new GUIContent("Attribute Set Script"));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            UpdateAttributeList();
        }

        if (attributeNames.Length > 0)
        {
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup("Attribute", selectedIndex, attributeNames);
            if (EditorGUI.EndChangeCheck())
            {
                SetCurrentAttributeName(attributeNames[selectedIndex]);
            }
        }
        else
        {
            EditorGUILayout.LabelField("No GameplayAttribute fields found.");
        }

        serializedObject.ApplyModifiedProperties();
    }

    private string GetCurrentAttributeName()
    {
        var prop = serializedObject.FindProperty("attributeFieldName");
        return prop != null ? prop.stringValue : "";
    }

    private void SetCurrentAttributeName(string name)
    {
        var prop = serializedObject.FindProperty("attributeFieldName");
        if (prop != null)
        {
            prop.stringValue = name;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
