using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeaponItem : WeaponItem {
    public string AmmoType { get; private set; }
    public int MaxMagazineAmmo { get; private set; }
    public int CurrentMagazineAmmo { get; private set; }
    public float ReloadTime { get; private set; }
    public bool IsReloading { get; private set; }

    // Initialize in the constructor
    public RangedWeaponItem(ItemData data, EquipmentSlot slotType, float damage,
                            string ammoType, int maxMagazineAmmo, float reloadTime)
        : base(data, slotType, damage) {
        this.AmmoType = ammoType;
        this.MaxMagazineAmmo = maxMagazineAmmo;
        this.ReloadTime = reloadTime;
        this.CurrentMagazineAmmo = maxMagazineAmmo; // Initially, the magazine is full, or set to 0 if the game requires reloading at the start
    }

    // Override the base class's usage methods
    public override void BeginUse(ICharacter user, ActivationTrigger trigger) {
        if (IsReloading) return; // Cannot perform other actions while reloading

        if (trigger == ActivationTrigger.LeftMouse) {
            // Check the magazine before firing
            if (CurrentMagazineAmmo > 0) {
                // Begin firing logic: charge or fire immediately
                // For simple implementation, actual firing may occur in EndUse
            } else {
                // Magazine is empty, attempt to reload or give a prompt
                // You can directly call Reload(user) here or notify the player that reloading is required
            }
        } else if (trigger == ActivationTrigger.KeyboardKey) {
            // Assume KeyboardKey is used for reloading (should have a clearer trigger in practice)
            StartReload(user);
        }
    }

    public override void HoldUse(ICharacter user, ActivationTrigger trigger) {
        // Implement charge mechanics here if needed for charged shots
    }

    public override void EndUse(ICharacter user, ActivationTrigger trigger) {
        if (IsReloading) return;

        if (trigger == ActivationTrigger.LeftMouse) {
            // Confirm firing action when releasing the left mouse button
            if (CurrentMagazineAmmo > 0) {
                // Execute firing: decrease magazine ammo
                CurrentMagazineAmmo--;
                PerformHitscanOrProjectileShot(user);

                // Consider recoil, accuracy, etc., here
            } else {
                // No ammo to fire
            }
        }
    }

    public override void OnScroll(ICharacter user, float scrollDelta) {
        // Use the scroll wheel to switch firing modes or adjust range, if necessary
    }

    // Method to start the reloading process
    private void StartReload(ICharacter user) {
        // Check conditions: is reloading necessary?
        if (CurrentMagazineAmmo == MaxMagazineAmmo) return; // No need to reload if the magazine is full

        int ammoInInventory = user.GetAmmoCount(AmmoType);
        if (ammoInInventory <= 0) {
            // No reserve ammo available to reload
            return;
        }

        // Start the reload process
        IsReloading = true;

        // In actual gameplay, use a coroutine, timer, or callback to simulate ReloadTime delay
        // Here, it's a demonstration, assuming a DelayedAction method calls a function after a set time
        DelayedAction(() => FinishReload(user), ReloadTime);
    }

    private void FinishReload(ICharacter user) {
        // Calculate how many bullets are needed to fill the magazine
        int needed = MaxMagazineAmmo - CurrentMagazineAmmo;
        int ammoInInventory = user.GetAmmoCount(AmmoType);

        int toLoad = Math.Min(needed, ammoInInventory);

        user.ConsumeAmmo(AmmoType, toLoad);
        CurrentMagazineAmmo += toLoad;

        IsReloading = false;
    }

    private void PerformHitscanOrProjectileShot(ICharacter user) {
        // This is a demonstration of hit detection logic
        // Use Raycast (if in Unity) or other methods to calculate hit targets
        // Call the TakeDamage method on the hit object
    }

    // Assume a simple delayed execution method (needs to be implemented in actual game code)
    private void DelayedAction(Action action, float delayTime) {
        // In Unity, you can use a coroutine: StartCoroutine(ReloadCoroutine(action, delayTime))
        // Or use a timer in your custom engine
    }
}
