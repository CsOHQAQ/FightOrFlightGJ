using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class WeaponItem : BaseItem, IEquipable, IActivatable {
    public event Action<IItem, IPlayerCharacter> OnEquipped;
    public event Action<IItem, IPlayerCharacter> OnUnequipped;

    public EquipmentSlot SlotType { get; protected set; }
    public float Damage { get; protected set; }

    protected PlayerCharacter equippedPlayer;

    public WeaponItem(ItemData data, float damage) : base(data) {
        this.SlotType = EquipmentSlot.Weapon;
        this.Damage = damage;
    }

    public virtual void Equip(IPlayerCharacter character) {
        OnEquipped?.Invoke(this, character);
        // Subscribe to the player's activation event if it is a PlayerCharacter.
        if (character is PlayerCharacter player) {
            equippedPlayer = player;
            player.OnCanActivateChanged += HandleCanActivateChanged;
        }
    }

    public virtual void Unequip(IPlayerCharacter character) {
        OnUnequipped?.Invoke(this, character);
        // Unsubscribe from the player's event to avoid memory leaks.
        if (character is PlayerCharacter player) {
            player.OnCanActivateChanged -= HandleCanActivateChanged;
            equippedPlayer = null;
        }
    }

    /// <summary>
    /// Called whenever the player's activation state changes.
    /// </summary>
    protected virtual void HandleCanActivateChanged(bool canActivate) {
        // You can override this in concrete weapon classes to enable/disable functionality.
        Debug.Log($"{Name} received CanActivate change: {canActivate}");
    }

    // IActivatable methods that concrete classes must implement.
    public abstract void BeginUse(IPlayerCharacter user, ActivationTrigger trigger);
    public abstract void HoldUse(IPlayerCharacter user, ActivationTrigger trigger);
    public abstract void EndUse(IPlayerCharacter user, ActivationTrigger trigger);
    public abstract void OnScroll(IPlayerCharacter user, float scrollDelta);
}
