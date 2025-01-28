using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapObject : MonoBehaviour
{
    // Serialized list of sprites
    [SerializeField]
    private List<Sprite> spriteList = new List<Sprite>();

    // Time between sprite changes
    [SerializeField]
    private float switchInterval = 0.5f;

    // Bool to control if the object rotates to face the player
    [SerializeField]
    private bool rotateToFacePlayer = false;

    private SpriteRenderer spriteRenderer;
    private int currentSpriteIndex = 0;
    private float timer = 0f;
    private Camera mainCamera;
    public bool DoesAutoPlay = true;
    void Start()
    {
        
        // Get the SpriteRenderer component attached to this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ensure there is a SpriteRenderer and the list is not empty
        if (spriteRenderer != null && spriteList.Count > 0)
        {
            // Assign the first sprite in the list to the SpriteRenderer
            spriteRenderer.sprite = spriteList[0];
        }
        else
        {
            ItemPickUp pickUp = GetComponent<ItemPickUp>();
            if (pickUp != null)
            {
                spriteRenderer.sprite = pickUp.item.itemSprite;
            }
        }

        // Try to get the main camera
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Handle sprite switching
        if (DoesAutoPlay && spriteList.Count > 1 ) // Only switch sprites if there's more than one
        {
            timer += Time.deltaTime;

            // Switch to the next sprite after the interval has passed
            if (timer >= switchInterval)
            {
                timer = 0f;
                currentSpriteIndex = (currentSpriteIndex + 1) % spriteList.Count; // Loop back to the first sprite
                spriteRenderer.sprite = spriteList[currentSpriteIndex];
            }
        }

        // Handle rotation to face the player
    if (rotateToFacePlayer && mainCamera != null)
    {
        // 1. Compute the direction to the camera (optionally zero out Y if you only want a horizontal direction)
        Vector3 directionToFace = mainCamera.transform.position - transform.position;
        directionToFace.y = 0f; // Only turn on Y-axis
        
        // 2. Figure out what the Y angle should be
        float targetY = Quaternion.LookRotation(directionToFace).eulerAngles.y;
        
        // 3. Get your current rotation's Euler angles
        Vector3 currentEuler = transform.eulerAngles;
        
        // 4. Create a "target" set of Euler angles that only changes the Y component
        Vector3 targetEuler = new Vector3(currentEuler.x, targetY, currentEuler.z);
        
        // 5. Convert those Euler angles back to a Quaternion
        Quaternion targetRotation = Quaternion.Euler(targetEuler);
        
        // 6. Slerp from the current rotation to this new Y-only rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    }

    public void UpdateSpriteAutoPlay(bool doesAutoPlay, bool snapToSprite = false, int snapToIndex = 0)
    {
        DoesAutoPlay = doesAutoPlay;
        if (snapToSprite)
        {
            // Clamping snapToIndex to be within the valid range of spriteList indices
            
            if(spriteList.Count>0)
            {
                int clampedIndex = Math.Clamp(snapToIndex, 0, spriteList.Count - 1);
                spriteRenderer.sprite = spriteList[clampedIndex];
            }
        }
    }
}
