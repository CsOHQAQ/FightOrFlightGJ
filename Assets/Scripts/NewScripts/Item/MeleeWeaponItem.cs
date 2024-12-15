using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponItem : WeaponItem {
    public float SwingCooldown { get; private set; }

    public MeleeWeaponItem(ItemData data, EquipmentSlot slotType, float damage, float swingCooldown)
        : base(data, slotType, damage) {
        this.SwingCooldown = swingCooldown;
    }

    public override void BeginUse(IPlayerCharacter user, ActivationTrigger trigger) {
        if (trigger == ActivationTrigger.LeftMouse) {
            
        }
    }

    public override void HoldUse(IPlayerCharacter user, ActivationTrigger trigger) {
       
    }

    public override void EndUse(IPlayerCharacter user, ActivationTrigger trigger) {
        
    }

    public override void OnScroll(IPlayerCharacter user, float scrollDelta) {
        
    }
}
