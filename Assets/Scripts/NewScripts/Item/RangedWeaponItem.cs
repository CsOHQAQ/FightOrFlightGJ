using System.Collections;
using UnityEngine;
using System;

public class RangedWeaponItem : WeaponItem, IAmmoDisplayEquipment {
    public string AmmoType { get; private set; }
    public int MaxMagazineAmmo { get; private set; }
    public int CurrentMagazineAmmo { get; private set; }
    public float ReloadTime { get; private set; }
    public bool IsReloading { get; private set; }

    public float FireCooldown { get; private set; } 
    private float nextFireTime = 0f;                 
    private float reloadTimer = 0f;
    private float lastShotTime = 0f;
    private float autoReloadThreshold = 2f; // for example, wait 3 seconds of inactivity before auto-recover starts
    private Coroutine autoRecoverCoroutine;

    // Overload window: for example, if ReloadTime = 2s,
    // OverloadWindowStart = 0.8f and OverloadWindowEnd = 1.0f
    // means between 0.8s and 1.0s into reload, pressing fire grants instant reload.
    private float OverloadWindowStart;
    private float OverloadWindowEnd;
    public float OverloadWindowRatio{ get {return overloadWindowRatio;} }// Set this via constructor or data
    private bool canOverload=false;
    private float overloadWindowRatio;
    public bool ShowAmmoInfo { get { return !IsReloading; } }
    public bool ShowCrosshair { get { return !IsReloading; } }

    public float CurrentLoadingPercentage {
        get {
            if (!IsReloading) return 0f;
            return Mathf.Clamp01(reloadTimer / ReloadTime);
        }
    }

    public RangedWeaponItem(ItemData data, EquipmentSlot slotType, float damage,
                            string ammoType, int maxMagazineAmmo, float reloadTime, float fireCooldown,
                            float overloadWindowRatio = 0.3f)
        : base(data, slotType, damage) {
        this.AmmoType = ammoType;
        this.MaxMagazineAmmo = maxMagazineAmmo;
        this.ReloadTime = reloadTime;
        this.FireCooldown = fireCooldown;
        this.CurrentMagazineAmmo = maxMagazineAmmo;
        this.overloadWindowRatio = overloadWindowRatio;
         // Calculate window based on ratio
        // Overload window is centered around ReloadTime/2
        // Window length = ReloadTime * OverloadWindowRatio
        // Half window length = (ReloadTime * OverloadWindowRatio) / 2
        float halfWindow = (ReloadTime * overloadWindowRatio) / 2f;
        float midpoint = ReloadTime / 2f;

        OverloadWindowStart = midpoint - halfWindow;
        OverloadWindowEnd = midpoint + halfWindow;
    }

    public override void BeginUse(IPlayerCharacter user, ActivationTrigger trigger) {
        if (IsReloading) {
            if (canOverload) TryOverload();
            return;
        } 
        if (Time.time < nextFireTime) return;

        if (trigger == ActivationTrigger.LeftMouse) {
            FireShot(user);
        }
    }

    public override void HoldUse(IPlayerCharacter user, ActivationTrigger trigger) {
        if (IsReloading) return;
        if (Time.time < nextFireTime) return;

        if (trigger == ActivationTrigger.LeftMouse) {
            FireShot(user);
        }
    }

    public override void OnScroll(IPlayerCharacter user, float delta) {}


    public override void EndUse(IPlayerCharacter user, ActivationTrigger trigger)
    {

    }

    private void FireShot(IPlayerCharacter user) {
        // Cancel auto-recover if running
        StopAutoRecoverIfActive();

        if (CurrentMagazineAmmo > 0) {
            CurrentMagazineAmmo--;
            lastShotTime = Time.time; // Update last shot time
            PerformHitscanOrProjectileShot(user);
            nextFireTime = Time.time + FireCooldown;

            if (CurrentMagazineAmmo == 0) {
                StartReload();
            } else {
                // If not empty, schedule auto-recover after inactivity
                ScheduleAutoRecover();
            }
        } else {
            if (!IsReloading) {
                StartReload();
            }
        }
    }

    private void StartReload() {
        if (IsReloading) return;
        IsReloading = true;
        reloadTimer = 0f;
        canOverload = true;
        StopAutoRecoverIfActive();
        GameManager.Instance.StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine() {
        while (reloadTimer < ReloadTime) {
            reloadTimer += Time.deltaTime;
            if (reloadTimer > OverloadWindowEnd) {
                canOverload = false;
            }
            yield return null;
        }
        FinishReload(normalReload: true);
    }

    private void FinishReload(bool normalReload) {
        CurrentMagazineAmmo = MaxMagazineAmmo;
        IsReloading = false;
        reloadTimer = 0f;

        if (!normalReload) {
            Debug.Log("Overload Successful! Instant Reload.");
        } else {
            Debug.Log("Normal Reload Completed.");
        }

        // Once reloaded, can start auto-recover after inactivity if player doesn't shoot
        ScheduleAutoRecover();
    }

    private void TryOverload() {
        if (!IsReloading) return;
        if (reloadTimer >= OverloadWindowStart && reloadTimer <= OverloadWindowEnd) {
            GameManager.Instance.StopAllCoroutines();
            FinishReload(normalReload: false);
        } else {
            canOverload = false;
            Debug.Log("Overload Failed: Pressed outside the window.");
        }
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

    private void StopAutoRecoverIfActive() {
        if (autoRecoverCoroutine != null) {
            GameManager.Instance.StopCoroutine(autoRecoverCoroutine);
            autoRecoverCoroutine = null;
        }
    }

    private void ScheduleAutoRecover() {
        // Schedule after inactivity threshold only if not reloading and not empty
        if (!IsReloading && CurrentMagazineAmmo < MaxMagazineAmmo) {
            // Start a coroutine that waits for inactivity then recovers ammo
            StopAutoRecoverIfActive(); // ensure only one instance
            autoRecoverCoroutine = GameManager.Instance.StartCoroutine(AutoRecoverCoroutine());
        }
    }

    private IEnumerator AutoRecoverCoroutine() {
        // Wait until player is inactive for autoReloadThreshold
        float startWaitTime = Time.time;
        while (Time.time - lastShotTime < autoReloadThreshold) {
            // If at any point we start reloading or overload, break
            if (IsReloading) yield break;
            yield return null;
        }

        // Now start recovering ammo over time
        float timePerAmmo = ReloadTime / MaxMagazineAmmo; 
        while (CurrentMagazineAmmo < MaxMagazineAmmo) {
            if (IsReloading) yield break; // If start reloading, stop
            // Add one ammo after timePerAmmo seconds
            yield return new WaitForSeconds(timePerAmmo);
            CurrentMagazineAmmo++;

            Debug.Log($"Auto recovered 1 ammo. Current Ammo: {CurrentMagazineAmmo}");

            // If player shoots again, break
            if (Time.time - lastShotTime < autoReloadThreshold) {
                yield break;
            }
        }

        autoRecoverCoroutine = null;
    }
}