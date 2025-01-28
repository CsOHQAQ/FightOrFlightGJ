#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameplayTagDatabase))]
public class GameplayTagDatabaseEditor : Editor
{
    GameplayTagDatabase db;
    bool showTree = false;
    string newTagInput = "";

    void OnEnable()
    {
        db = (GameplayTagDatabase)target;
        db.BuildTagTree(); // Build or rebuild on load
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Gameplay Tag Database (Hierarchical)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Display the flat list for direct editing
        var prop = serializedObject.FindProperty("allTags");
        EditorGUILayout.PropertyField(prop, new GUIContent("All Tags"), true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Add New Tag:");
        newTagInput = EditorGUILayout.TextField(newTagInput);
        if (GUILayout.Button("Add Tag"))
        {
            if (!string.IsNullOrEmpty(newTagInput) && !db.AllTags.Contains(newTagInput))
            {
                db.AllTags.Add(newTagInput);
                newTagInput = "";
                serializedObject.ApplyModifiedProperties();
                db.BuildTagTree();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Tag is empty or already exists.", "OK");
            }
        }

        if (GUILayout.Button("Rebuild Tag Tree"))
        {
            db.BuildTagTree();
        }

        if (GUILayout.Button("Print Tag Tree to Console"))
        {
            db.GetRootNode()?.PrintHierarchy();
        }

        // Show hierarchical tree
        showTree = EditorGUILayout.Foldout(showTree, "Tag Hierarchy");
        if (showTree && db.GetRootNode() != null)
        {
            DrawTagNode(db.GetRootNode(), 0);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawTagNode(GameplayTagNode node, int indent)
    {
        if (node.NodeName == "ROOT") // Skip printing root name, only print children
        {
            foreach (var c in node.Children)
            {
                DrawTagNode(c, indent);
            }
            return;
        }

        EditorGUI.indentLevel = indent;
        EditorGUILayout.LabelField(node.NodeName, EditorStyles.boldLabel);

        foreach (var c in node.Children)
        {
            DrawTagNode(c, indent + 1);
        }

        EditorGUI.indentLevel = 0;
    }
}
#endif