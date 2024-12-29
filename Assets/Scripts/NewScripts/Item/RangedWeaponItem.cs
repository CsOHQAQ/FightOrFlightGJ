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
    public event Action OnFired;


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
        OnFired?.Invoke(); 
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
        EventContext context = new EventContext {
            Source = user,
            AttackInfo = new AttackData {
                BaseDamage = this.Damage,
                AmmoType = this.AmmoType,
            }
        };
        
        EventChainManager.Instance.ExecuteAttackChain(ref context);

        if (context.AttackInfo.ProjectilePrefab == null) {
            PerformHitscanShot(context);
        }
        else {
            SpawnProjectileShot(context); 
            //Debug.Log("-----------------------------------");
        }

    }

    private void PerformHitscanShot(EventContext context)
    {
        float range = 100f;
        Vector3 muzzlePos = GetMuzzleLocation();
        Vector3 forwardDir = GetMuzzleForwardDirection();

        if (Physics.Raycast(muzzlePos, forwardDir, out RaycastHit hit, range))
        {
            IHitReceiver hitReceiver = hit.collider.GetComponent<IHitReceiver>();
            if (hitReceiver != null)
            {
                // 更新 context.HitData
                HitInfo hitInfo = new HitInfo {
                    HitPoint = hit.point,
                    HitNormal = hit.normal
                };
                context.HitData = new HitData {
                    HitInfo = hitInfo,
                    FinalDamage = 0f,
                    WasCrit = false,
                    IsLethalHit = false
                };
                context.Target = hitReceiver;

                // 再次执行HitEventChain
                EventChainManager.Instance.ExecuteHitChain(ref context);
            }
        }
        Debug.Log("Hitscan shot fired.");
    }

    private void SpawnProjectileShot(EventContext context)
    {
        
        var user = context.Source as IPlayerCharacter;
        if (context.AttackInfo.ProjectilePrefab == null)
        {
            Debug.LogWarning("No projectilePrefab specified. Falling back to hitscan or do nothing.");
            return;
        }

        // 2) 计算枪口位置 & 方向
        Vector3 muzzlePos = GetMuzzleLocation();
        Vector3 forwardDir = GetMuzzleForwardDirection();

        // 3) 实例化Projectile
        GameObject projObj = GameObject.Instantiate(context.AttackInfo.ProjectilePrefab, muzzlePos, Quaternion.LookRotation(forwardDir));
        Projectile projectile = projObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            // 传递伤害/攻击者信息等
            projectile.Setup(context); // 你可以定义 Setup(...) 让projectile获取伤害、射速、攻击者ID等
        }
        
        Debug.Log("Spawned a projectile shot.");
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