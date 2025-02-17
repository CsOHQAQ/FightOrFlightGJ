using UnityEngine;
using MoreMountains.Feedbacks; // only if you are using Feel
using System;

public class WeaponComponent : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the actual gun mesh or root transform here.")]
    [SerializeField] private Transform gunMeshRoot;

    [Header("Feel Player (New System)")]
    [SerializeField] private MMF_Player firePlayer; // Not 'MMFeedbacks'

    private RangedWeaponItem weaponData; // We'll store the currently equipped weapon data here.

    // --- Step 1: Hook up the weapon item to this component ---
    private void Start()
    {

    }
    public void Initialize(PlayerCharacter player, RangedWeaponItem rangedWeapon)
    {
        // Unsubscribe if we were previously subscribed to another weapon
        if (weaponData != null)
        {
            weaponData.OnFired -= HandleWeaponFired;
        }

        // Store our new RangedWeaponItem
        weaponData = rangedWeapon;

        // Subscribe to the OnFired event
        if (weaponData != null)
        {
            weaponData.OnFired += HandleWeaponFired;
        }
        
        if (firePlayer == null) { return; }
        

        foreach (MMF_Feedback feedback in firePlayer.FeedbacksList)
        {
            if (feedback is MMF_Rotation rotationFeedback)
            {
                rotationFeedback.AnimateRotationDuration=weaponData.FireCooldown/2f;
                Debug.Log($"AnimateRotationDuration: {rotationFeedback.AnimateRotationDuration}");
            }
        }
        player.OnCanActivateChanged += HandleCanActivateChanged;
    }

    // --- Step 2: Respond to the event ---
    private void HandleWeaponFired()
    {
        // 1. Play Feel feedback if assigned
        
        firePlayer?.PlayFeedbacks();

        // 2. Optionally do things like:
        //    - Animate the gun mesh (e.g., recoil, slide, etc.)
        //    - Play muzzle flash particle effects
        //    - Play a separate audio source if not handled by MMFeedback

        Debug.Log("WeaponComponent: Weapon just fired!");
    }

    // --- Clean up to avoid memory leaks ---
    private void OnDestroy()
    {
        if (weaponData != null)
        {
            weaponData.OnFired -= HandleWeaponFired;
        }
    }

    protected virtual void HandleCanActivateChanged(bool canActivate) {
        gunMeshRoot.gameObject.SetActive(canActivate);
    }
}
