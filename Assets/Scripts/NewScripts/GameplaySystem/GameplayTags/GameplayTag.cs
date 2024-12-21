using System;

[Serializable]
public struct GameplayTag
{
    public string TagName;

    public GameplayTag(string tagName)
    {
        TagName = tagName;
    }

    public bool IsValid() => !string.IsNullOrEmpty(TagName);

    /// <summary>
    /// Split the tag by '.' to get hierarchical segments.
    /// e.g. "Weapon.Ranged.Sniper" => ["Weapon", "Ranged", "Sniper"]
    /// </summary>
    public string[] GetTagSegments()
    {
        if (!IsValid()) return Array.Empty<string>();
        return TagName.Split('.');
    }

    /// <summary>
    /// Returns true if this tag is the same or a parent of the other tag in the hierarchy.
    /// For example, "Weapon.Ranged" is a parent of "Weapon.Ranged.Sniper".
    /// Likewise, "Weapon" is a parent of "Weapon.Ranged.Sniper".
    /// A tag is also considered a parent/child of itself if you want exact match to count.
    /// </summary>
    public bool MatchesOrIsParentOf(GameplayTag other)
    {
        if (!IsValid() || !other.IsValid()) return false;

        // If they are identical, we can decide to call that a match. 
        // If you only want strict parent (excluding exact match), adjust logic.
        if (TagName == other.TagName)
            return true;

        // Check if other starts with this tag plus a '.'
        // e.g. "Weapon.Ranged" => "Weapon.Ranged.Sniper"
        // If "Weapon.Ranged" is prefix of "Weapon.Ranged.Sniper", we consider it a parent.
        if (other.TagName.StartsWith(TagName + "."))
            return true;

        return false;
    }

    public override string ToString() => TagName;

    public override bool Equals(object obj)
    {
        if (obj is GameplayTag tag)
        {
            return TagName == tag.TagName;
        }
        return false;
    }

    public override int GetHashCode() => TagName != null ? TagName.GetHashCode() : 0;

    public static bool operator ==(GameplayTag a, GameplayTag b) => a.TagName == b.TagName;
    public static bool operator !=(GameplayTag a, GameplayTag b) => a.TagName != b.TagName;
}
