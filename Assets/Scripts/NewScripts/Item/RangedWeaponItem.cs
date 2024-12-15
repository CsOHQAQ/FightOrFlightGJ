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
            // Immediately attempt to fire when the button is pressed
            if (CurrentMagazineAmmo > 0) {
                CurrentMagazineAmmo--;
                PerformHitscanOrProjectileShot(user);

                // Set the next allowed fire time
                nextFireTime = Time.time + FireCooldown;
            } else {
                // No ammo in magazine, try reloading or notify the player
                // If you want to initiate reload automatically here, you can:
                // StartReload(user);
            }
        } else if (trigger == ActivationTrigger.KeyboardKey) {
            // Assume this trigger initiates reload
            StartReload(user);
        }
    }

    public override void HoldUse(ICharacter user, ActivationTrigger trigger) {
        // If the weapon requires charge mechanics or continuous fire while holding, implement it here.
        // For a simple one-shot weapon, this may remain empty.
    }

    public override void EndUse(ICharacter user, ActivationTrigger trigger) {
        // If you previously relied on firing at release, this can now remain empty or be used for cleanup.
        // For continuous fire weapons, you might stop firing logic here when the user releases the button.
    }

    public override void OnScroll(ICharacter user, float scrollDelta) {
        // Scroll can be used to switch firing modes or zoom levels if needed.
    }

    private void StartReload(ICharacter user) {
        if (CurrentMagazineAmmo == MaxMagazineAmmo) return; // Already full

        int ammoInInventory = user.GetAmmoCount(AmmoType);
        if (ammoInInventory <= 0) {
            // No reserve ammo available
            return;
        }

        IsReloading = true;
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
        // Example hitscan logic:
        Vector3 muzzlePos = GetMuzzleLocation();
        Vector3 forwardDir = GetMuzzleForwardDirection();
        // Example: if you have a max range for hitscan:
        float range = 100f; // Adjust as needed

        // Visualize the hitscan ray (from muzzle position in forward direction)
        Debug.DrawRay(muzzlePos, forwardDir * range, Color.red, 1.0f); // Duration = 1 second

        //Prepare EventContext
        EventContext context = new EventContext {
                Attacker = user,
                AttackInfo = new AttackData {
                    BaseDamage = this.Damage,
                    AmmoType = this.AmmoType,
                }
            };
        EventChainManager.Instance.ExecuteAttackChain(context);

        // Perform the actual hitscan raycast
        if (Physics.Raycast(muzzlePos, forwardDir, out RaycastHit hit, range)) {
            IHitReceiver hitReceiver = hit.collider.GetComponent<IHitReceiver>();
            if (hitReceiver != null) {
                HitInfo hitInfo = new HitInfo {
                    HitPoint = hit.point,
                    HitNormal = hit.normal,
                    AdditionalData = null
                };
            context.HitData = new HitData {
                HitInfo = hitInfo,
                FinalDamage = 0f, // start from 0;
                WasCrit = false,
                IsLethalHit = false
            };
            context.Target = hitReceiver;
            EventChainManager.Instance.ExecuteHitChain(context);
                //hitReceiver.OnHit(hitInfo);
            }

            // Optionally, draw a line to the hit point for visualization
            Debug.DrawLine(muzzlePos, hit.point, Color.green, 1.0f); // Green line to the hit point
        } else {
            // Optionally, draw the full range ray when no hit is detected
            Debug.DrawRay(muzzlePos, forwardDir * range, Color.yellow, 1.0f); // Yellow ray if no hit
        }

        Debug.Log("Current Ammo Left: " + CurrentMagazineAmmo);
    }


    /// <summary>
    /// Gets the "muzzle" location from where we start the ray.
    /// In this implementation, it uses the player's main camera.
    /// </summary>
    private Vector3 GetMuzzleLocation() {
        // For a real game, avoid calling Camera.main repeatedly for performance; instead, store a reference.
        return Camera.main.transform.position;
    }

    /// <summary>
    /// Gets the forward direction from the camera, so the raycast aligns with the player's view.
    /// </summary>
    private Vector3 GetMuzzleForwardDirection() {
        return Camera.main.transform.forward;
    }

    private void DelayedAction(Action action, float delayTime) {
        // In Unity, implement using a coroutine:
        // StartCoroutine(ReloadCoroutine(action, delayTime));
    }

}


