using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Item", menuName = "Items/Item", order = 1)]
public class Item_ScriptableObject : ScriptableObject
{
    [Header("Item")]

    [SerializeField]
    public string itemName;

    [SerializeField]
    public Sprite itemSprite;

    [SerializeField]
    [TextArea(2,3)]
    public string itemDescription;

    [SerializeField]
    public int itemValue;
}
