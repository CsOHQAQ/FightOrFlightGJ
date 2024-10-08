using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ITEM_SLOT
{   
    HEAD,
    CHEST,
    ARMS,
    LEGS,
    WEAPON
}

public enum ITEM_CONDITION
{
    MINT,
    USED,
    WORN,
    BROKE
}

[CreateAssetMenu(fileName = "Item", menuName = "Items/Equipment", order = 2)]
public class Equipment_ScriptableObject : Item_ScriptableObject
{
    [Header("Equipment")]

    [SerializeField]
    public ITEM_SLOT itemSlot;

    [SerializeField]
    public int itemDurability;

    [SerializeField]
    public int attackValue;

    [SerializeField]
    public int defenceValue;

}
