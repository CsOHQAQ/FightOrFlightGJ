using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum OverloadState { BeforeWindow, InWindow, AfterWindow, Failed }
public class AmmoDisplayUI : MonoBehaviour
{
    private PlayerCharacter playerCharacter;

    [SerializeField]
    private Transform ammoImageGroup;

    [SerializeField]
    private Transform reticle;

    private ReticleScript reticleScript;

    [SerializeField]
    private Sprite ammoIconSprite; // Icon for filled ammo

    [SerializeField]
    private Sprite emptyAmmoIconSprite; // Icon for empty ammo

    [SerializeField]
    private GameObject ammoImagePrefab; // Prefab for ammo image UI object

    private ReloadDisplayUI reloadDisplayUI;
    private IEquipable equipment;
    private RangedWeaponItem currentRangedWeapon; // We'll store the currently equipped ranged weapon
    private int lastAmmoCount = -1; // Tracks the previous ammo count

    void Awake()
    {
        // 1) Get PlayerCharacter reference from GameManager
        playerCharacter = GameManager.Instance.PlayerCharacter;

        // 2) Get references from children
        reloadDisplayUI = GetComponentInChildren<ReloadDisplayUI>();
        reticleScript = GetComponentInChildren<ReticleScript>();
        if (reticleScript != null)
        {
            reticleScript.playerCharacter = playerCharacter;
        }

        // 3) Subscribe to player's equip events
        playerCharacter.OnPlayerEquipped += OnPlayerEquipped;
        playerCharacter.OnPlayerUnEquipped += OnPlayerUnEquipped;

        // 4) Validate references
        if (ammoImageGroup == null)
        {
            Debug.LogError("Ammo Image Group Not Set for UI");
        }
        if (ammoIconSprite == null)
        {
            Debug.LogError("Ammo Icon Sprite Not Set");
        }
        if (emptyAmmoIconSprite == null)
        {
            Debug.LogError("Empty Ammo Icon Sprite Not Set");
        }
        if (ammoImagePrefab == null)
        {
            Debug.LogError("Ammo Image Prefab Not Set");
        }
    }

    void Update()
    {
        var item = equipment as IAmmoDisplayEquipment;
        if (item != null)
        {
            // Show/hide crosshair
            reticle.gameObject.SetActive(item.ShowCrosshair);

            // Show/hide ammo UI
            if (item.ShowAmmoInfo)
            {
                ammoImageGroup.gameObject.SetActive(true);
                UpdateAmmoUI(item.MaxMagazineAmmo, item.CurrentMagazineAmmo);
            }
            else
            {
                ammoImageGroup.gameObject.SetActive(false);
            }

            // Show/hide reload UI
            bool reloading = item.IsReloading;
            reloadDisplayUI.gameObject.SetActive(reloading);
            if (reloading) {
            reloadDisplayUI.UpdateReloadProgress(item.CurrentLoadingPercentage);
            reloadDisplayUI.InitializeOverloadWindowUI(item.OverloadWindowRatio);
            reloadDisplayUI.UpdateStateColors(
                item.CurrentOverloadState, 
                item.OverloadFailedThisReload
                );
            }
        }
    }
    private void OnPlayerEquipped(IEquipable equipable)
    {
        // Only handle if it's a weapon
        if (equipable.SlotType != EquipmentSlot.Weapon)
            return;

        // 1) If we already had a ranged weapon, unsubscribe from its OnFired
        if (currentRangedWeapon != null)
        {
            currentRangedWeapon.OnFired -= OnWeaponFiredViaReticle;
        }

        // 2) Assign the new equipment
        equipment = equipable;

        // 3) If it's an IAmmoDisplayEquipment, rebuild ammo icons
        var item = equipable as IAmmoDisplayEquipment;
        ClearImageObjects();
        if (item != null)
        {
            // Populate ammo icons dynamically
            for (int i = 0; i < item.MaxMagazineAmmo; i++)
            {
                GameObject ammoIcon = Instantiate(ammoImagePrefab, ammoImageGroup);
                var imageComponent = ammoIcon.GetComponent<Image>();

                if (imageComponent != null)
                {
                    imageComponent.sprite = i < item.CurrentMagazineAmmo ? ammoIconSprite : emptyAmmoIconSprite;
                }
                else
                {
                    Debug.LogError("Ammo Image Prefab does not have an Image component.");
                }
            }
            lastAmmoCount = item.CurrentMagazineAmmo;
        }

        // 4) If the new item is a RangedWeaponItem, subscribe reticle to its OnFired
        currentRangedWeapon = equipable as RangedWeaponItem;
        if (currentRangedWeapon != null)
        {
            currentRangedWeapon.OnFired += OnWeaponFiredViaReticle;
        }
    }

    private void OnPlayerUnEquipped(IEquipable equipable)
    {
        // If the old equipable is a RangedWeaponItem, unsubscribe
        var oldRanged = equipable as RangedWeaponItem;
        if (oldRanged != null)
        {
            oldRanged.OnFired -= OnWeaponFiredViaReticle;
        }

        equipment = null;
        currentRangedWeapon = null;
        //ClearImageObjects(); // if you want to hide the ammo UI
    }

    public void UpdateAmmoUI(int maxMagazineAmmo, int currentMagazineAmmo)
    {
        // If maxMagazineAmmo has changed, rebuild
        if (ammoImageGroup.childCount != maxMagazineAmmo)
        {
            ClearImageObjects();

            for (int i = 0; i < maxMagazineAmmo; i++)
            {
                GameObject ammoIcon = Instantiate(ammoImagePrefab, ammoImageGroup);
                var imageComponent = ammoIcon.GetComponent<Image>();
                if (imageComponent != null)
                {
                    imageComponent.sprite = i < currentMagazineAmmo ? ammoIconSprite : emptyAmmoIconSprite;
                }
                else
                {
                    Debug.LogError("Ammo Image Prefab does not have an Image component.");
                }
            }
            lastAmmoCount = currentMagazineAmmo;
            return;
        }

        // Only update changed sprites
        if (currentMagazineAmmo != lastAmmoCount)
        {
            for (int i = 0; i < maxMagazineAmmo; i++)
            {
                var ammoIcon = ammoImageGroup.GetChild(i).GetComponent<Image>();
                if (ammoIcon != null)
                {
                    ammoIcon.sprite = i < currentMagazineAmmo ? ammoIconSprite : emptyAmmoIconSprite;
                }
            }
            lastAmmoCount = currentMagazineAmmo;
        }
    }

    void ClearImageObjects()
    {
        foreach (Transform child in ammoImageGroup)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Called when the RangedWeaponItem fires. We then pass it on to the ReticleScript to do a "firing" effect.
    /// </summary>
    private void OnWeaponFiredViaReticle()
    {
        if (reticleScript != null)
        {
            reticleScript.OnWeaponFired();
        }
    }
}
