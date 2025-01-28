#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameplayEffectModifierMagnitude))]
public class GameplayEffectModifierMagnitudeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Start the property
        EditorGUI.BeginProperty(position, label, property);

        // Calculate base properties
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // Draw Calculation Type
        var calcTypeProp = property.FindPropertyRelative("magnitudeCalculationType");
        var rect = new Rect(position.x, position.y, position.width, lineHeight);
        EditorGUI.PropertyField(rect, calcTypeProp, new GUIContent("Calculation Type"));

        rect.y += lineHeight + spacing;

        // Handle specific calculation types
        var calcType = (GameplayEffectMagnitudeCalculation)calcTypeProp.enumValueIndex;

        switch (calcType)
        {
            case GameplayEffectMagnitudeCalculation.ScalableFloat:
                var flatMagnitudeProp = property.FindPropertyRelative("flatMagnitude");
                EditorGUI.PropertyField(rect, flatMagnitudeProp, new GUIContent("Flat Magnitude"));
                rect.y += lineHeight + spacing;
                break;

            case GameplayEffectMagnitudeCalculation.AttributeBased:
                // Draw all attribute-based properties
                var attributeBasedProp = property.FindPropertyRelative("attributeBased");
                EditorGUI.PropertyField(rect, attributeBasedProp, new GUIContent("Attribute Based"), true);
                rect.y += EditorGUI.GetPropertyHeight(attributeBasedProp, true) + spacing;

                // Draw Capture Definition (nested field)
                var captureDefProp = attributeBasedProp.FindPropertyRelative("backingAttribute");
                if (captureDefProp != null)
                {
                    EditorGUI.PropertyField(rect, captureDefProp, new GUIContent("Capture Definition"), true);
                    rect.y += EditorGUI.GetPropertyHeight(captureDefProp, true) + spacing;
                }
                break;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Calculate height dynamically based on expanded states
        float height = EditorGUIUtility.singleLineHeight; // For Calculation Type
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        var calcTypeProp = property.FindPropertyRelative("magnitudeCalculationType");
        var calcType = (GameplayEffectMagnitudeCalculation)calcTypeProp.enumValueIndex;

        switch (calcType)
        {
            case GameplayEffectMagnitudeCalculation.ScalableFloat:
                height += EditorGUIUtility.singleLineHeight + spacing; // For Flat Magnitude
                break;

            case GameplayEffectMagnitudeCalculation.AttributeBased:
                // Account for attribute-based fields
                var attributeBasedProp = property.FindPropertyRelative("attributeBased");
                height += EditorGUI.GetPropertyHeight(attributeBasedProp, true) + spacing;

                // Add height for Capture Definition if it exists
                var captureDefProp = attributeBasedProp.FindPropertyRelative("backingAttribute");
                if (captureDefProp != null)
                {
                    height += EditorGUI.GetPropertyHeight(captureDefProp, true) + spacing;
                }
                break;
        }

        return height;
    }
}
#endif