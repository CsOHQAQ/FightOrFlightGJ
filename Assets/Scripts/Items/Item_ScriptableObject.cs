using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEM_SLOT
{   HEAD,
    CHEST,
    ARMS,
    LEGS,
    WEAPON}

public enum ITEM_CONDITION
{
    MINT,
    USED,
    WORN,
    BROKE
}

[CreateAssetMenu(fileName = "Item")]
public class Item_ScriptableObject : ScriptableObject
{
    [SerializeField]
    public string itemName;

    [SerializeField]
    public ITEM_SLOT itemSlot;

    [SerializeField]
    public int itemDurability;

    [SerializeField]
    public int itemValue;

    [SerializeField]
    public int attackValue;

    [SerializeField]
    public int defenceValue;

    [SerializeField]
    [TextArea(2,4)]
    public string itemDescription;

}
