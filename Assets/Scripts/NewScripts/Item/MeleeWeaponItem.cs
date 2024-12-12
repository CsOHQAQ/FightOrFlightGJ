using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponItem : WeaponItem {
    public float SwingCooldown { get; private set; }

    public MeleeWeaponItem(ItemData data, EquipmentSlot slotType, float damage, float swingCooldown)
        : base(data, slotType, damage) {
        this.SwingCooldown = swingCooldown;
    }

    public override void BeginUse(ICharacter user, ActivationTrigger trigger) {
        if (trigger == ActivationTrigger.LeftMouse) {
            
        }
    }

    public override void HoldUse(ICharacter user, ActivationTrigger trigger) {
       
    }

    public override void EndUse(ICharacter user, ActivationTrigger trigger) {
        
    }

    public override void OnScroll(ICharacter user, float scrollDelta) {
        
    }
}
