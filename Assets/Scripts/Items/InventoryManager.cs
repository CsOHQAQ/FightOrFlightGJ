using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.Properties;
//using UnityEditor.UIElements;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    //Array of items
    //0 - head, 1 - chest, 2 - arms, 3 - legs, 4 - weapon
    [SerializeField]
    private Equipment_ScriptableObject[] equippedItems;

    //List of Item SOs that the player currently has but are not equipped
    [SerializeField]
    private List<Item_ScriptableObject> bagItems;

    [SerializeField]
    private UI_ItemPickup pickupUI;

    [Header("DEBUG")]
    [SerializeField]
    private Item_ScriptableObject[] testItems;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Makes the InventoryManager persist between scenes
        }
        else
        {
            Destroy(gameObject); // Destroy any duplicate instances
        }
    }

    public void Start()
    {
        foreach (Item_ScriptableObject item in testItems)
        {
           
            AddItemToBag(item);
        }
    }

    /// <summary>
    /// Function that should be called by external classes. Checks to see if the item is valid before trying to equip it
    /// </summary>
    /// <param name="item"> The item that is attempting to be equipped</param>
    /// <returns></returns>
    public bool TryEquipItem(Equipment_ScriptableObject item)
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
                    EquipItem(item, 2);
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
    /// <param name="slot">The slot that the item is being put into. 0-Head, 1-Chest, 2-Weapon</param>
    private void EquipItem(Equipment_ScriptableObject itemToEquip, int slot)
    {
        bagItems.Add(equippedItems[slot]);
        bagItems.Remove(itemToEquip);
        equippedItems[slot] = itemToEquip;
    }

    /// <summary>
    /// Function called to add an item to the bag. Returns true if it was able to, false otherwise
    /// </summary>
    /// <param name="item">The item being added to the list</param>
    /// <returns></returns>
    public bool AddItemToBag(Item_ScriptableObject item)
    {
        if(item)
        {
            Debug.Log("bagItems.Add");
            bagItems.Add(item);

            Debug.Log("AddItemToStack");
            pickupUI.AddItemToStack(item);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Function that should be called externally when trying to use a consumable item
    /// </summary>
    /// <param name="item"> The item attempting to be used</param>
    /// <returns></returns>
    public bool TryUseItem(ConsumableItem_ScriptableObject item)
    {
        if(bagItems.Contains(item))
        {
            UseItem(item);
        }

        return false;
    }

    /// <summary>
    /// Function called by the UI_Inventory manager when the interact button is pressed.
    /// </summary>
    /// <param name="item">The item being passed through the manager</param>
    /// <returns></returns>
    public bool TryInteractItem(Item_ScriptableObject item)
    {
        switch (item.itemType)
        {
            case ITEM_TYPE.EQUIPMENT:
                TryEquipItem((Equipment_ScriptableObject)(item));
                return true;
            case ITEM_TYPE.CONSUMEABLE:
                TryUseItem((ConsumableItem_ScriptableObject)(item));
                return true;
            default:
                break;
        }
        return false;

    }

    /// <summary>
    /// Function that should ONLY be called from TryUseItem after making sure it's valid.
    /// Removes the item from the BagItems list and then does the functionality
    /// </summary>
    /// <param name="item">The item  to be used and removed from the list</param>
    private void UseItem(ConsumableItem_ScriptableObject item)
    {
        bagItems.Remove(item);

        item.RunBehavior();
    }

    /// <summary>
    /// Function to get a list of the bag items
    /// </summary>
    /// <returns></returns>
    public List<Item_ScriptableObject> GetBagItems() { return bagItems; }

    /// <summary>
    /// Function to get the array of equipped items
    /// </summary>
    /// <returns></returns>
    public Equipment_ScriptableObject[] GetEquipment() { return equippedItems; }

    /// <summary>
    /// Function called externally that returns the lump sum of the player's bonus attack and defense value from equipment
    /// </summary>
    /// <returns>An array, 0 = attack, 1 = defense</returns>
    public int[] GetEquippedItemStats()
    {
        int[] stats = new int[2];

        int attack = 0;
        int defense = 0;

        foreach (Equipment_ScriptableObject equippedItem in equippedItems)
        {
            if (equippedItem ==null) continue;
            attack += equippedItem.attackValue;
            defense += equippedItem.defenceValue;
        }

        stats[0] = attack;
        stats[1] = defense;

        return stats;
    }

    /// <summary>
    /// Function to be called when the player wants to sell an item. Communicates with the DayManager that gives them the value of the item they're selling
    /// </summary>
    /// <param name="item">Item to be sold and removed from the inventory</param>
    /// <returns>True if the process was succesful</returns>
    public bool SellItem(Item_ScriptableObject item)
    {

        if (item == null) {  return false; }

        DayManager.Instance.AddMoney(item.itemValue);

        bagItems.Remove(item);

        return true;

    }

    /// <summary>
    /// Function to be called when the player dies, removes all of the items in their inventory and gives them nothing in return. Equipment stays.
    /// </summary>
    /// <returns>True if all the removals were successful</returns>
    public bool OnDeath()
    {

        bool result = true;

        foreach(Item_ScriptableObject item in bagItems)
        {
            result = bagItems.Remove(item);
        }

        return result;
    }

}
