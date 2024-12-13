using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RangedWeaponItem : WeaponItem {
    public string AmmoType { get; private set; }
    public int MaxMagazineAmmo { get; private set; }
    public int CurrentMagazineAmmo { get; private set; }
    public float ReloadTime { get; private set; }
    public bool IsReloading { get; private set; }

    public float FireCooldown { get; private set; } // The time interval between shots
    private float nextFireTime = 0f;                 // The next time at which the weapon can fire

    public RangedWeaponItem(ItemData data, EquipmentSlot slotType, float damage,
                            string ammoType, int maxMagazineAmmo, float reloadTime, float fireCooldown)
        : base(data, slotType, damage) {
        this.AmmoType = ammoType;
        this.MaxMagazineAmmo = maxMagazineAmmo;
        this.ReloadTime = reloadTime;
        this.FireCooldown = fireCooldown;
        this.CurrentMagazineAmmo = maxMagazineAmmo; 
    }

    public override void BeginUse(ICharacter user, ActivationTrigger trigger) {
        if (IsReloading) return; 
        if (Time.time < nextFireTime) return; // Prevent firing if we haven't reached the cooldown time

        if (trigger == ActivationTrigger.LeftMouse) {
            // Check if we have ammo before starting to fire
            if (CurrentMagazineAmmo > 0) {
                // Begin firing logic; actual firing will occur on EndUse
                // For some weapons (like charge-based), you might charge here.
                // For simple weapons, you could consider firing immediately, but we'll stick to firing on EndUse.
            } else {
                // No ammo in magazine, consider starting a reload or notify the player
            }
        } else if (trigger == ActivationTrigger.KeyboardKey) {
            // Assume this trigger initiates reload
            StartReload(user);
        }
    }

    public override void HoldUse(ICharacter user, ActivationTrigger trigger) {
        // If the weapon requires charge mechanics, they can be handled here
    }

    public override void EndUse(ICharacter user, ActivationTrigger trigger) {
        if (IsReloading) return;

        if (trigger == ActivationTrigger.LeftMouse) {
            // Attempt to fire upon release of the mouse button
            float currentTime = Time.time;
            if (currentTime >= nextFireTime && CurrentMagazineAmmo > 0) {
                // Fire the shot
                CurrentMagazineAmmo--;
                PerformHitscanOrProjectileShot(user);

                // Set the next allowed fire time
                nextFireTime = currentTime + FireCooldown;
            } else {
                // Either no ammo or still in cooldown
            }
        }
    }

    public override void OnScroll(ICharacter user, float scrollDelta) {
        // Scroll could be used to switch firing modes or adjust other parameters if needed
    }

    private void StartReload(ICharacter user) {
        if (CurrentMagazineAmmo == MaxMagazineAmmo) return; // Already full

        int ammoInInventory = user.GetAmmoCount(AmmoType);
        if (ammoInInventory <= 0) {
            // No reserve ammo available
            return;
        }

        IsReloading = true;

        // Use a delayed action to simulate the reload time
        DelayedAction(() => FinishReload(user), ReloadTime);
    }

    private void FinishReload(ICharacter user) {
        int needed = MaxMagazineAmmo - CurrentMagazineAmmo;
        int ammoInInventory = user.GetAmmoCount(AmmoType);

        int toLoad = Math.Min(needed, ammoInInventory);
        user.ConsumeAmmo(AmmoType, toLoad);
        CurrentMagazineAmmo += toLoad;

        IsReloading = false;
    }

    private void PerformHitscanOrProjectileShot(ICharacter user) {
        // Implement hitscan or projectile logic here
        // For hitscan: Raycast forward, find target, apply damage if hit
        // For projectile: Instantiate a projectile and apply physics
    }

    private void DelayedAction(Action action, float delayTime) {
        // In Unity, implement using a coroutine:
        // StartCoroutine(ReloadCoroutine(action, delayTime));
    }
}
