using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class WeaponItem : BaseItem, IEquipable, IActivatable {
    public event Action<IItem, IPlayerCharacter> OnEquipped;
    public event Action<IItem, IPlayerCharacter> OnUnequipped;

    public EquipmentSlot SlotType { get; protected set; }
    public float Damage { get; protected set; }

    

    public WeaponItem(ItemData data,  float damage) : base(data) {
        this.SlotType = EquipmentSlot.Weapon;
        this.Damage = damage;
    }

    public virtual void Equip(IPlayerCharacter character) {
        OnEquipped?.Invoke(this, character);
    }

    public virtual void Unequip(IPlayerCharacter character) {
        OnUnequipped?.Invoke(this, character);
    }

    public abstract void BeginUse(IPlayerCharacter user, ActivationTrigger trigger);
    public abstract void HoldUse(IPlayerCharacter user, ActivationTrigger trigger);
    public abstract void EndUse(IPlayerCharacter user, ActivationTrigger trigger);
    public abstract void OnScroll(IPlayerCharacter user, float scrollDelta);
}