using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEM_TYPE
{
    EQUIPMENT,
    CONSUMEABLE,
    BOMB
}


[CreateAssetMenu(fileName = "Item", menuName = "Items/Item", order = 1)]
public class Item_ScriptableObject : ScriptableObject
{
    [Header("Item")]

    [SerializeField]
    public string itemName;

    [SerializeField]
    public int itemId;

    [SerializeField]
    public Sprite itemSprite;

    [SerializeField]
    [TextArea(2,3)]
    public string itemDescription;

    [SerializeField]
    public int itemValue;

    [SerializeField]
    public ITEM_TYPE itemType;
}
