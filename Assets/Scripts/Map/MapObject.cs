using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (spriteList.Count > 1) // Only switch sprites if there's more than one
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
            // Get the position of the camera and calculate the direction to face it
            Vector3 directionToFace = mainCamera.transform.position - transform.position;

            // Keep only the Y-axis rotation
            directionToFace.y = 0;

            // Rotate towards the camera (only on the Y-axis)
            Quaternion targetRotation = Quaternion.LookRotation(directionToFace);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Smooth rotation
        }
    }
}
