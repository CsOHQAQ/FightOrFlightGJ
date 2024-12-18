using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoDisplayUI : MonoBehaviour
{
    private PlayerCharacter playerCharacter;

    [SerializeField]
    private Transform ammoImageGroup;

    [SerializeField]
    private Sprite ammoIconSprite; // Icon for filled ammo

    [SerializeField]
    private Sprite emptyAmmoIconSprite; // Icon for empty ammo

    [SerializeField]
    private GameObject ammoImagePrefab; // Prefab for ammo image UI object

    private IEquipable equipment;
    private int lastAmmoCount = -1; // Tracks the previous ammo count
    void Awake()
    {
        // Get PlayerCharacter reference from GameManager
        playerCharacter = GameManager.Instance.PlayerCharacter;

        // Subscribe to player events
        playerCharacter.OnPlayerEquipped += OnPlayerEquipped;
        playerCharacter.OnPlayerUnEquipped += OnPlayerUnEquipped;

        // Check if required variables are set
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
            UpdateAmmoUI(item.MaxMagazineAmmo, item.CurrentMagazineAmmo);
        }
    }

    void OnPlayerEquipped(IEquipable equipable)
    {
        ClearImageObjects();

        var item = equipable as IAmmoDisplayEquipment;
        equipment = equipable;
        if (item != null)
        {
            // Populate ammo icons dynamically using the prefab
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
        }
    }

    void ClearImageObjects()
    {
        // Destroy all child objects in ammoImageGroup
        foreach (Transform child in ammoImageGroup)
        {
            Destroy(child.gameObject);
        }
    }

    void OnPlayerUnEquipped(IEquipable equipable)
    {
        //ClearImageObjects();
    }

    public void UpdateAmmoUI(int maxMagazineAmmo, int currentMagazineAmmo)
    {
        // If maxMagazineAmmo has changed, rebuild the UI
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

        // Efficiently update only the ammo sprites
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
}
