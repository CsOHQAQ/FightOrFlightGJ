using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WeaponItem : BaseItem, IEquipable, IActivatable {
    public event Action<IItem, ICharacter> OnEquipped;
    public event Action<IItem, ICharacter> OnUnequipped;

    public EquipmentSlot SlotType { get; private set; }
    public string AmmoType { get; private set; }

    // Additional properties like AttackPower and ReloadTime can be added
    private bool isCharging = false;

    public WeaponItem(ItemData data, EquipmentSlot slotType, string ammoType) : base(data) {
        this.SlotType = slotType;
        this.AmmoType = ammoType;
    }

    public void Equip(ICharacter character) {
        OnEquipped?.Invoke(this, character);
    }

    public void Unequip(ICharacter character) {
        OnUnequipped?.Invoke(this, character);
    }

    // Implementation of IActivatable: Perform attack or mode switching based on input
    public void BeginUse(ICharacter user, ActivationTrigger trigger) {
        if (trigger == ActivationTrigger.LeftMouse) {
            // Pressing the left mouse button starts charging or prepares to shoot
            isCharging = true;
        } else if (trigger == ActivationTrigger.RightMouse) {
            // The right mouse button might be for aiming or switching firing modes
        }
    }

    public void HoldUse(ICharacter user, ActivationTrigger trigger) {
        if (trigger == ActivationTrigger.LeftMouse && isCharging) {
            // While charging (if it's a charge-based weapon), increase the charge value
        }
    }

    public void EndUse(ICharacter user, ActivationTrigger trigger) {
        if (trigger == ActivationTrigger.LeftMouse && isCharging) {
            // Charging is complete, release the left mouse button to fire a bullet
            isCharging = false;
            // Check if there is ammunition of AmmoType in the character's inventory
            // Consume one bullet and calculate damage to the target or in the direction of the crosshair
        } 
        // If ending the right mouse button action, disable aiming
    }

    public void OnScroll(ICharacter user, float scrollDelta) {
        // The scroll wheel operation can be used to adjust the firing angle, switch weapon modes, or adjust charge, etc.
        // For example, modify an internal parameter of the weapon based on scrollDelta
    }
}
