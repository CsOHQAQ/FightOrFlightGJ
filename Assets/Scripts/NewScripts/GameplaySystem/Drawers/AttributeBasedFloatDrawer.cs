using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AttributeBasedFloat))]
public class AttributeBasedFloatDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel++;

        var coefficientProp = property.FindPropertyRelative("coefficient");
        var preAddProp = property.FindPropertyRelative("preMultiplyAdditiveValue");
        var postAddProp = property.FindPropertyRelative("postMultiplyAdditiveValue");
        var curveProp = property.FindPropertyRelative("attributeCurve");
        var calcTypeProp = property.FindPropertyRelative("attributeCalculationType");
        var backingAttribProp = property.FindPropertyRelative("backingAttribute");

        float y = position.y;
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // coefficient
        var rect = new Rect(position.x, y, position.width, lineHeight);
        EditorGUI.PropertyField(rect, coefficientProp);
        y += lineHeight + spacing;

        rect.y = y;
        EditorGUI.PropertyField(rect, preAddProp);
        y += lineHeight + spacing;

        rect.y = y;
        EditorGUI.PropertyField(rect, postAddProp);
        y += lineHeight + spacing;

        rect.y = y;
        EditorGUI.PropertyField(rect, curveProp);
        y += lineHeight + spacing;

        rect.y = y;
        EditorGUI.PropertyField(rect, calcTypeProp, new GUIContent("Attribute Calc Type"));
        y += lineHeight + spacing;

        rect.y = y;
        EditorGUI.PropertyField(rect, backingAttribProp, new GUIContent("Capture Definition"));
        y += lineHeight + spacing;

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 6 lines in this example
        return 6 * EditorGUIUtility.singleLineHeight 
            + 5 * EditorGUIUtility.standardVerticalSpacing;
    }
}
