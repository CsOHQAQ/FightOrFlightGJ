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

    //List of Item SOs that the player currently has but are not equipped
    [SerializeField]
    private List<Item_ScriptableObject> bagItems;

    public void Start()
    {

    }

    /// <summary>
    /// Function that should be called by external classes. Checks to see if the item is valid before trying to equip it
    /// </summary>
    /// <param name="item"> The item that is attempting to be equipped</param>
    /// <returns></returns>
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

    /// <summary>
    /// Function that should ONLY be called by TryEquipItem.
    /// Removes the previous item from the equipped array and adds it back to the bag
    /// Then puts the new item in the array in the correct slot
    /// </summary>
    /// <param name="itemToEquip">The item that should be taking the new slot</param>
    /// <param name="slot">The slot that the item is being put into. 0-Head, 1-Chest, 2-Arms, 3-Legs, 4-Weapon</param>
    private void EquipItem(Item_ScriptableObject itemToEquip, int slot)
    {
        bagItems.Add(equippedItems[slot]);
        bagItems.Remove(itemToEquip);
        equippedItems[slot] = itemToEquip;
    }

}
