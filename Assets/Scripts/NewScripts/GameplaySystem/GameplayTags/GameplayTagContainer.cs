using System;
using System.Collections.Generic;

[Serializable]
public class GameplayTagContainer
{
    // Storing as strings for quick membership checks. Alternatively, store as HashSet<GameplayTag>.
    private HashSet<string> tagSet = new HashSet<string>();

    public void AddTag(GameplayTag tag)
    {
        if (tag.IsValid())
        {
            tagSet.Add(tag.TagName);
        }
    }

    public void RemoveTag(GameplayTag tag)
    {
        if (tag.IsValid())
        {
            tagSet.Remove(tag.TagName);
        }
    }

    /// <summary>
    /// Direct check: do we have this exact tag string in the container?
    /// </summary>
    public bool HasExactTag(GameplayTag tag)
    {
        return tag.IsValid() && tagSet.Contains(tag.TagName);
    }

    /// <summary>
    /// Hierarchical check: 
    /// Return true if the container has any tag that matches or is a parent of the given tag.
    /// E.g. If container has "Weapon.Ranged" and we pass in "Weapon.Ranged.Sniper", 
    /// we count that as a match if we want parent->child matching.
    /// </summary>
    public bool HasTagWithParents(GameplayTag tag)
    {
        if (!tag.IsValid()) return false;

        // We need to see if any of our stored tags matches or is parent of the input tag.
        foreach (var storedTag in tagSet)
        {
            var st = new GameplayTag(storedTag);
            if (st.MatchesOrIsParentOf(tag))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if we have ANY of the tags in other container (hierarchical check).
    /// If you want EXACT check, you'd do a different function.
    /// </summary>
    public bool MatchesAny(GameplayTagContainer other)
    {
        foreach (var otherTagName in other.tagSet)
        {
            GameplayTag otherTag = new GameplayTag(otherTagName);
            // If we have something that is a parent or exact match
            if (HasTagWithParents(otherTag))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if we have ALL of the tags in other container (hierarchical check).
    /// </summary>
    public bool MatchesAll(GameplayTagContainer other)
    {
        foreach (var otherTagName in other.tagSet)
        {
            GameplayTag otherTag = new GameplayTag(otherTagName);
            if (!HasTagWithParents(otherTag))
                return false;
        }
        return true;
    }

    public IEnumerable<string> GetAllTags()
    {
        return tagSet;
    }

    public void AddTagsFromContainer(GameplayTagContainer other)
    {
        foreach (var t in other.tagSet)
        {
            tagSet.Add(t);
        }
    }

    public void RemoveTagsFromContainer(GameplayTagContainer other)
    {
        foreach (var t in other.tagSet)
        {
            tagSet.Remove(t);
        }
    }
}
