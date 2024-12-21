using UnityEditor;
using UnityEngine;
using System.Linq;

public class MyTagSelectorComponent : MonoBehaviour
{
    public GameplayTagDatabase TagDatabase;
    public GameplayTag SelectedTag;
}

[CustomEditor(typeof(MyTagSelectorComponent))]
public class MyTagSelectorComponentEditor : Editor
{
    MyTagSelectorComponent comp;

    void OnEnable()
    {
        comp = (MyTagSelectorComponent)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("TagDatabase"));
        // If we have a database, show a dropdown of tags
        if (comp.TagDatabase != null)
        {
            // Ensure the database is built
            comp.TagDatabase.BuildTagTree();

            var allTags = comp.TagDatabase.AllTags;
            int currentIndex = allTags.IndexOf(comp.SelectedTag.TagName);
            if (currentIndex < 0) currentIndex = 0;

            int newIndex = EditorGUILayout.Popup("Selected Tag", currentIndex, allTags.ToArray());
            if (newIndex >= 0 && newIndex < allTags.Count)
            {
                comp.SelectedTag = new GameplayTag(allTags[newIndex]);
            }
        }
        else
        {
            EditorGUILayout.LabelField("No TagDatabase assigned.");
        }

        serializedObject.ApplyModifiedProperties();
    }
}
