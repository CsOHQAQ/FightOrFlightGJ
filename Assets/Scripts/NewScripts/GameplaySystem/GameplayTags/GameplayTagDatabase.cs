using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "AbilitySystem/GameplayTagDatabase")]
public class GameplayTagDatabase : ScriptableObject
{
    [SerializeField, Tooltip("A list of all gameplay tags in dot-separated format, e.g. 'Weapon.Ranged.Sniper'")]
    private List<string> allTags = new List<string>();

    // Hierarchical tree root for editor usage or advanced searching
    [System.NonSerialized] private GameplayTagNode rootNode;

    public List<string> AllTags => allTags;

    /// <summary>
    /// (Optional) Build an in-memory tree from allTags for hierarchical display or searching.
    /// You can call this in OnEnable() or in a custom editor.
    /// </summary>
    public void BuildTagTree()
    {
        rootNode = new GameplayTagNode("ROOT");
        foreach (var t in allTags)
        {
            rootNode.AddTagPath(t.Split('.'), 0);
        }
    }

    /// <summary>
    /// Return the root node of the hierarchy (for editor or runtime usage).
    /// </summary>
    public GameplayTagNode GetRootNode()
    {
        if (rootNode == null)
        {
            BuildTagTree();
        }
        return rootNode;
    }

    /// <summary>
    /// True if the tagName is in allTags. (Exact check)
    /// </summary>
    public bool IsValidTag(string tagName)
    {
        return allTags.Contains(tagName);
    }
}

/// <summary>
/// Node for hierarchical representation of a tag. 
/// For example, "Weapon" -> child "Ranged" -> child "Sniper".
/// </summary>
[System.Serializable]
public class GameplayTagNode
{
    public string NodeName;
    public List<GameplayTagNode> Children = new List<GameplayTagNode>();

    public GameplayTagNode(string name)
    {
        NodeName = name;
    }

    /// <summary>
    /// Recursive function that adds a path segment by segment. 
    /// e.g. path = ["Weapon", "Ranged", "Sniper"]
    /// </summary>
    public void AddTagPath(string[] pathSegments, int index)
    {
        if (index >= pathSegments.Length) return;

        string current = pathSegments[index];
        // Find child or create
        var child = Children.FirstOrDefault(x => x.NodeName == current);
        if (child == null)
        {
            child = new GameplayTagNode(current);
            Children.Add(child);
        }
        child.AddTagPath(pathSegments, index + 1);
    }

    /// <summary>
    /// Debug or Editor usage: print hierarchy to console or string builder.
    /// </summary>
    public void PrintHierarchy(string prefix = "")
    {
        Debug.Log(prefix + NodeName);
        foreach (var c in Children)
        {
            c.PrintHierarchy(prefix + "- ");
        }
    }
}
