using System.Collections;
using UnityEngine;
using System;

public class RangedWeaponItem : WeaponItem, IAmmoDisplayEquipment {
    public string AmmoType { get; private set; }
    public int MaxMagazineAmmo { get; private set; }
    public int CurrentMagazineAmmo { get; private set; }
    public float ReloadTime { get; private set; }
    public bool IsReloading { get; private set; }

    // Base angle, e.g. 5 degrees at accuracy=0
    private float BaseSpreadAngle = 5f; 

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
                            string ammoType, int maxMagazineAmmo, float reloadTime, float fireCooldown,float BaseSpreadAngle,
                            float overloadWindowRatio = 0.3f)
        : base(data, slotType, damage) {
        this.AmmoType = ammoType;
        this.MaxMagazineAmmo = maxMagazineAmmo;
        this.ReloadTime = reloadTime;
        this.FireCooldown = fireCooldown;
        this.CurrentMagazineAmmo = maxMagazineAmmo;
        this.BaseSpreadAngle = BaseSpreadAngle;
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

            AttributeSet attributeSet = user.GetAbilitySystemComponent().AttributeSet;
            WeaponAttributeSet weaponAttributeSet = attributeSet as WeaponAttributeSet;
            

            if (weaponAttributeSet != null) {
                // Fire multiple shots
                int bulletsToFire = Mathf.Max(Mathf.FloorToInt(weaponAttributeSet.BulletPerShot.CurrentValue),1);
                for (int i = 0; i < bulletsToFire; i++)
                {
                    PerformHitscanOrProjectileShot(user);
                }
            }else{
                PerformHitscanOrProjectileShot(user);
                Debug.LogError("No WeaponAttributeSet found");
            }
            
            
            
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
    // 1) Access the IPlayerCharacter
    var user = context.Source as IPlayerCharacter;
    if (user == null)
    {
        Debug.LogWarning("No valid user found for hitscan shot.");
        return;
    }

        // 2) Attempt to retrieve the WeaponAttributeSet from the character's AbilitySystem
        var asc = user.GetAbilitySystemComponent();
        if (asc != null)
        {
            AttributeSet attributeSet = asc.AttributeSet;
            WeaponAttributeSet weaponAttrSet = attributeSet as WeaponAttributeSet;
            if (weaponAttrSet != null)
            {
                // 3) Retrieve the accuracy (could be stored in weaponAttrSet.Accuracy.CurrentValue)
                float accuracyValue = weaponAttrSet.Accuracy.CurrentValue; 
                // e.g. 0 means big spread, 1 or higher means less spread
                Debug.Log($"Current Accuracy: {accuracyValue}");
                
                // 4) Compute random spread
                Vector3 muzzlePos = GetMuzzleLocation();
                Vector3 forwardDir = GetMuzzleForwardDirection();

                Vector3 finalShotDirection = ApplyAccuracySpread(forwardDir, accuracyValue);

                // 5) Raycast with the 'finalShotDirection' to get the actual hitscan result
                float range = 100f;
                if (Physics.Raycast(muzzlePos, finalShotDirection, out RaycastHit hit, range))
                {
                    IHitReceiver hitReceiver = hit.collider.GetComponent<IHitReceiver>();
                    if (hitReceiver != null)
                    {
                        // Fill out hit data
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

                        // Then run HitEventChain
                        EventChainManager.Instance.ExecuteHitChain(ref context);
                    }
                }

                Debug.Log("Hitscan shot fired with spread-based accuracy.");
                return;
            }
        }

        // fallback: if no attributes found, do normal hitscan with no spread
        Debug.LogWarning("No WeaponAttributeSet found, defaulting to no spread.");
        //BaseHitscanNoSpread(context);
    }


    private void SpawnProjectileShot(EventContext context)
    {
        var user = context.Source as IPlayerCharacter;
        if (user == null) return;

        // Retrieve accuracy from user attribute set...
        float accuracyValue = 0f; // default
        var asc = user.GetAbilitySystemComponent();
        if (asc != null)
        {
            AttributeSet attributeSet = asc.AttributeSet;
            WeaponAttributeSet weaponAttrSet = attributeSet as WeaponAttributeSet;
            if (weaponAttrSet != null)
            {
                accuracyValue = weaponAttrSet.Accuracy.CurrentValue;
            }
        }

        Vector3 muzzlePos = GetMuzzleLocation();
        Vector3 forwardDir = GetMuzzleForwardDirection();
        // Apply random spread
        Vector3 finalProjectileDir = ApplyAccuracySpread(forwardDir, accuracyValue);

        // Instantiate projectile
        var projObj = GameObject.Instantiate(context.AttackInfo.ProjectilePrefab, muzzlePos, Quaternion.LookRotation(forwardDir));
        //Debug.Log($"Spawned projectile rotation = {projObj.transform.eulerAngles}");
        Projectile projectile = projObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Setup(context, finalProjectileDir);
        }
    }

    /// <summary>
    /// Adjusts 'forwardDir' by a random spread depending on 'accuracyValue'.
    /// Higher accuracy => smaller spread range.
    /// Example: finalSpreadAngle = baseAngle / (1 + accuracyValue).
    /// </summary>
    private Vector3 ApplyAccuracySpread(Vector3 forwardDir, float accuracyValue)
    {
        

        // Example formula: finalSpreadAngle = baseAngle / (1 + accuracyValue)
        // So if accuracy=1, finalSpread=2.5°, if accuracy=4, finalSpread=1°.
        float finalSpreadAngle = BaseSpreadAngle / (1f + accuracyValue);

        // We randomize yaw & pitch in [-finalSpreadAngle/2, +finalSpreadAngle/2]
        float yaw   = UnityEngine.Random.Range(-finalSpreadAngle * 0.5f, finalSpreadAngle * 0.5f);
        float pitch = UnityEngine.Random.Range(-finalSpreadAngle * 0.5f, finalSpreadAngle * 0.5f);

        // Construct a rotation from these angles
        Quaternion spreadRotation = Quaternion.Euler(pitch, yaw, 0f);
        Debug.Log("Pitch: "+pitch+", Yaw: "+ yaw);
        // Apply it to the forward direction
        Vector3 spreadDirection = spreadRotation * forwardDir.normalized;
        Debug.Log($"Spawned projectile spreadDirection = {spreadDirection}");

        // 1) Normalize forward vector
        Vector3 fwd = forwardDir.normalized;

        // 2) Choose a random angle up to 'finalSpreadAngle' and a random rotation around that axis
        float spreadRad = Mathf.Deg2Rad * finalSpreadAngle; // finalSpreadAngle in degrees
        float u = UnityEngine.Random.value;  // for radius
        float r = Mathf.Sin(spreadRad) * Mathf.Sqrt(u);  
        float theta = UnityEngine.Random.Range(0f, 2f * Mathf.PI);

        // Build an orthonormal basis around fwd
        Vector3 up = (Mathf.Abs(Vector3.Dot(fwd, Vector3.up)) > 0.9999f) ? Vector3.forward : Vector3.up;
        Vector3 right = Vector3.Cross(fwd, up).normalized;
        up = Vector3.Cross(fwd, right).normalized;

        // Offsets in plane
        Vector3 offset = (Mathf.Cos(theta) * right + Mathf.Sin(theta) * up) * r;
        // Combine with forward
        Vector3 finalDir = (fwd * Mathf.Sqrt(1f - r*r)) + offset;  // For uniform distribution
        
        return finalDir.normalized;
    }


    private Vector3 GetMuzzleLocation() {
        return Camera.main.transform.position;
    }

    private Vector3 GetMuzzleForwardDirection()
    {
        // 1) Get camera’s local pitch from its localEulerAngles.x
        float pitchAngle = Camera.main.transform.eulerAngles.x;

        // 2) Get parent’s yaw from eulerAngles.y
        float yawAngle = Camera.main.transform.parent.eulerAngles.y;

        // 3) Construct a combined rotation using (pitch, yaw, 0).
        Quaternion combinedRot = Quaternion.Euler(pitchAngle, yawAngle, 0f);

        // 4) Multiply by Vector3.forward to get the forward direction in world space
        Vector3 finalForward = combinedRot * Vector3.forward;
        //Debug.Log("------------------"+ finalForward + "");
        // Optionally normalize:
        return finalForward.normalized;
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