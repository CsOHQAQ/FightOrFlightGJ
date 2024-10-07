using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    //Array of items
    //0 - head, 1 - chest, 2 - arms, 3 - legs, 4 - weapon
    [SerializeField]
    private Item_ScriptableObject[] equippedItems;

    [SerializeField]
    private List<Item_ScriptableObject> bagItems;

    public void Start()
    {
        bagItems = new List<Item_ScriptableObject>();
    }

    public bool TryEquipItem(Item_ScriptableObject item)
    {
        if (bagItems.Contains(item))
        {
            switch (item.itemSlot)
            {
                case ITEM_SLOT.HEAD:
                    EquipItem(item, 0);
                    break;
                case ITEM_SLOT.CHEST:
                    EquipItem(item, 1);
                    break;
                case ITEM_SLOT.ARMS:
                    EquipItem(item, 2);
                    break;
                case ITEM_SLOT.LEGS:
                    EquipItem(item, 3);
                    break;
                case ITEM_SLOT.WEAPON:
                    EquipItem(item, 4);
                    break;
                default:
                    return false;
            }

            return true;
        }

        return false;
    }

    private void EquipItem(Item_ScriptableObject itemToEquip, int slot)
    {
        bagItems.Add(equippedItems[slot]);
        bagItems.Remove(itemToEquip);
        equippedItems[slot] = itemToEquip;
    }


}
