using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseItem : IItem {
    protected ItemData data;

    public BaseItem(ItemData data) {
        this.data = data;
    }

    public string Name => data.Name;
    public string Description => data.Description;
    public string Icon => data.IconPath;
    public float Weight => data.Weight;
    public int Value => data.Value;

    public virtual string GetItemInfo() {
        return $"{Name}: {Description}";
    }
}