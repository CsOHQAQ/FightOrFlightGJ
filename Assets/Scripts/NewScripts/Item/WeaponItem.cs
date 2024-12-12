using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class WeaponItem : BaseItem, IEquipable, IActivatable {
    public event Action<IItem, ICharacter> OnEquipped;
    public event Action<IItem, ICharacter> OnUnequipped;

    public EquipmentSlot SlotType { get; protected set; }
    public float Damage { get; protected set; }

    public WeaponItem(ItemData data, EquipmentSlot slotType, float damage) : base(data) {
        this.SlotType = slotType;
        this.Damage = damage;
    }

    public virtual void Equip(ICharacter character) {
        OnEquipped?.Invoke(this, character);
    }

    public virtual void Unequip(ICharacter character) {
        OnUnequipped?.Invoke(this, character);
    }

    public abstract void BeginUse(ICharacter user, ActivationTrigger trigger);
    public abstract void HoldUse(ICharacter user, ActivationTrigger trigger);
    public abstract void EndUse(ICharacter user, ActivationTrigger trigger);
    public abstract void OnScroll(ICharacter user, float scrollDelta);
}