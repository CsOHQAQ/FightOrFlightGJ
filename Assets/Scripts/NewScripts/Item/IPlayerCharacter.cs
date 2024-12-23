using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerCharacter : ICharacter
{
    // Interface for interacting with items
    // The character can have a container to store items (e.g., a backpack or inventory). 
    // Here, we assume simple Add/Remove methods.
    bool AddItem(IItem item);      // Add an item to the character's inventory
    bool RemoveItem(IItem item);   // Remove an item from the inventory

    // Methods for equipping and unequipping items
    bool EquipItem(IEquipable item);
    bool UnequipItem(IEquipable item);

    // Methods for using activatable items
    // When the character presses a certain input (e.g., left mouse button) to start using an item, BeginUse is called.
    void BeginUseItem(IActivatable item, ActivationTrigger trigger);
    void HoldUseItem(IActivatable item, ActivationTrigger trigger);
    void EndUseItem(IActivatable item, ActivationTrigger trigger);
    void ScrollUseItem(IActivatable item, float scrollDelta);

    public int GetAmmoCount(string ammoType)
    {
        return 1;
    }

    public void ConsumeAmmo(string ammoType, int ammountToLoad)
    {
        //deduct the corresponding ammo amount from inventory
    }
    // Additional character attributes and methods can be extended as needed, such as:
    // - Character stats (e.g., strength, agility, intelligence)
    // - Movement methods (e.g., Move, Rotate)
    // - Status effect management (e.g., poisoned, slowed)
    // These depend on the specific design requirements of the game.

   
}
