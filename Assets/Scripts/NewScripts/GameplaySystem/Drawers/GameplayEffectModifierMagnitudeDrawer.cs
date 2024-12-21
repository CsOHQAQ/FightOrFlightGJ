using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameplayEffectModifierMagnitude))]
public class GameplayEffectModifierMagnitudeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Start the property
        EditorGUI.BeginProperty(position, label, property);

        // We'll layout a bit manually. Let's indent and handle fields in order.
        // For clarity, let's use EditorGUIUtility.labelWidth or we can use property fields.

        // Step 1: Show MagnitudeCalculationType as a dropdown
        var calcTypeProp = property.FindPropertyRelative("magnitudeCalculationType");
        var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(rect, calcTypeProp, new GUIContent("Calculation Type"));
        
        // Step 2: Get the current enum value
        var calcType = (GameplayEffectMagnitudeCalculation)calcTypeProp.enumValueIndex;

        rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        switch (calcType)
        {
            case GameplayEffectMagnitudeCalculation.ScalableFloat:
            {
                // Show the flatMagnitude field
                var flatMagnitudeProp = property.FindPropertyRelative("flatMagnitude");
                EditorGUI.PropertyField(rect, flatMagnitudeProp, new GUIContent("Flat Magnitude"));
                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                break;
            }
            case GameplayEffectMagnitudeCalculation.AttributeBased:
            {
                // Show the attributeBased field
                var attributeBasedProp = property.FindPropertyRelative("attributeBased");
                EditorGUI.PropertyField(rect, attributeBasedProp, new GUIContent("Attribute Based"));
                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                break;
            }
            // If you have CustomCalculationClass or SetByCaller, you'd handle them similarly
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // We need to calculate how many lines we use
        float totalHeight = EditorGUIUtility.singleLineHeight; // for the calcType
        var calcTypeProp = property.FindPropertyRelative("magnitudeCalculationType");
        var calcType = (GameplayEffectMagnitudeCalculation)calcTypeProp.enumValueIndex;

        switch (calcType)
        {
            case GameplayEffectMagnitudeCalculation.ScalableFloat:
                totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                break;
            case GameplayEffectMagnitudeCalculation.AttributeBased:
                //  We show the entire property for 'attributeBased', which is a class/struct
                //  That typically is one line if it's a single field, but if it's its own custom property drawer, might be more lines. 
                //  For simplicity, let's do single line or do EditorGUI.GetPropertyHeight(...) for the child.
                var attributeBasedProp = property.FindPropertyRelative("attributeBased");
                float childHeight = EditorGUI.GetPropertyHeight(attributeBasedProp, true);
                totalHeight += childHeight + EditorGUIUtility.standardVerticalSpacing;
                break;
        }

        return totalHeight;
    }
}
