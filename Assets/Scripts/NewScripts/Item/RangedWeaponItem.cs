using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RangedWeaponItem : WeaponItem, IAmmoDisplayEquipment
{
    public string AmmoType { get; private set; }
    public int MaxMagazineAmmo { get; private set; }
    public int CurrentMagazineAmmo { get; private set; }
    public float ReloadTime { get; private set; }
    public bool IsReloading { get; private set; }

    public float FireCooldown { get; private set; } 
    private float nextFireTime = 0f;                 
    private float reloadTimer = 0f;

    public bool ShowAmmoInfo { get { return !IsReloading; } }
    public bool ShowCrosshair { get { return !IsReloading; } }

    public float CurrentLoadingPercentage {
        get {
            if (!IsReloading) return 0f;
            return Mathf.Clamp01(reloadTimer / ReloadTime);
        }
    }

    public RangedWeaponItem(ItemData data, EquipmentSlot slotType, float damage,
                            string ammoType, int maxMagazineAmmo, float reloadTime, float fireCooldown)
        : base(data, slotType, damage) {
        this.AmmoType = ammoType;
        this.MaxMagazineAmmo = maxMagazineAmmo;
        this.ReloadTime = reloadTime;
        this.FireCooldown = fireCooldown;
        this.CurrentMagazineAmmo = maxMagazineAmmo; 
    }

    public override void BeginUse(IPlayerCharacter user, ActivationTrigger trigger) {
        if (IsReloading) return; 
        if (Time.time < nextFireTime) return; 

        if (trigger == ActivationTrigger.LeftMouse) {
            // Attempt to fire immediately
            FireShot(user);
        } 
    }

    public override void HoldUse(IPlayerCharacter user, ActivationTrigger trigger) {
        if (IsReloading) return; 
        if (Time.time < nextFireTime) return;
        
        if (trigger == ActivationTrigger.LeftMouse) {
            // Continuous firing if desired
            FireShot(user);
        }
    }
    
    public override void EndUse(IPlayerCharacter user, ActivationTrigger trigger) {
        // No-op here unless needed for future logic
    }

    public override void OnScroll(IPlayerCharacter user, float scrollDelta) {
        // If needed for changing fire mode, etc.
    }

    private void FireShot(IPlayerCharacter user) {
        if (CurrentMagazineAmmo > 0) {
            CurrentMagazineAmmo--;
            PerformHitscanOrProjectileShot(user);

            nextFireTime = Time.time + FireCooldown;

            // After firing, check if mag is empty
            if (CurrentMagazineAmmo == 0) {
                StartReload(); // Automatically start reload if empty
            }

        } else {
            // If somehow tried to fire with no ammo
            if (!IsReloading) {
                StartReload();
            }
        }
    }

    private void StartReload() {
        if (IsReloading) return;
        IsReloading = true;
        reloadTimer = 0f;

        //Need some monobehavior to run the coroutine for it. 
        GameManager.Instance.StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine() {
        
        while (reloadTimer < ReloadTime) {
            reloadTimer += Time.deltaTime;
            yield return null;
        }

        FinishReload();
    }

    private void FinishReload() {
        CurrentMagazineAmmo = MaxMagazineAmmo;
        IsReloading = false;
        reloadTimer = 0f;
    }

    private void PerformHitscanOrProjectileShot(IPlayerCharacter user) {
        Vector3 muzzlePos = GetMuzzleLocation();
        Vector3 forwardDir = GetMuzzleForwardDirection();
        float range = 100f; 

        Debug.DrawRay(muzzlePos, forwardDir * range, Color.red, 1.0f);

        EventContext context = new EventContext {
            Attacker = user,
            AttackInfo = new AttackData {
                BaseDamage = this.Damage,
                AmmoType = this.AmmoType,
            }
        };
        EventChainManager.Instance.ExecuteAttackChain(context);

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
                    FinalDamage = 0f,
                    WasCrit = false,
                    IsLethalHit = false
                };
                context.Target = hitReceiver;
                EventChainManager.Instance.ExecuteHitChain(context);
            }

            Debug.DrawLine(muzzlePos, hit.point, Color.green, 1.0f);
        } else {
            Debug.DrawRay(muzzlePos, forwardDir * range, Color.yellow, 1.0f);
        }

        Debug.Log("Current Ammo Left: " + CurrentMagazineAmmo);
    }

    private Vector3 GetMuzzleLocation() {
        return Camera.main.transform.position;
    }

    private Vector3 GetMuzzleForwardDirection() {
        return Camera.main.transform.forward;
    }

    private void DelayedAction(Action action, float delayTime) {
        // Not used now since we switched to coroutine-based reload
    }
}
