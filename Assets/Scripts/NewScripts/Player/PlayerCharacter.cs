using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public enum PlayerState
{
    MovementState,
    DoorOpeningState,
    MenuState
}

public class PlayerCharacter : MonoBehaviour, IPlayerCharacter,IAbilitySystemComponent
{
    public event Action<ICharacter> OnCharacterDied;

    [SerializeField] ArtifactSO[] artifactsOnStart;
    AbilitySystemComponent abilitySystemComponent;
    WeaponComponent weaponComponent;

    private List<ArtifactItem> artifactItems;
    private bool isMoving = false;
    
    private InteractComponent interactComponent;
    private PlayerHandsComponent hand;

    [SerializeField, Tooltip("Duration of the movement forward or backward in seconds.")]
    private float moveDuration = 1.0f;

    [SerializeField, Tooltip("Distance the player moves forward or backward.")]
    private float moveDistance = 1.0f;

    [SerializeField, Tooltip("Duration of the turn in seconds.")]
    private float turnDuration = 0.5f;

    [SerializeField, Tooltip("Distance the player moves for a bump effect.")]
    private float bumpDistance = 0.1f;

    [SerializeField, Tooltip("Duration of the bump animation in seconds.")]
    private float bumpDuration = 0.1f;

    [SerializeField, Tooltip("Movement script uses the forward of this transform to determine where forward is for the character.")]
    private Transform bodyTransform;

    private Door currentDoor;
    public Door CurrentDoor { get { return currentDoor; } }

    private PlayerState currentState;
    public PlayerState CurrentState { get { return currentState; } }

    // Define delegates for entering and exiting states
    public delegate void StateChangeHandler(PlayerState newState);
    public event StateChangeHandler OnStateEnter;
    public event StateChangeHandler OnStateExit;

    public event Action<IEquipable> OnPlayerEquipped;
    public event Action<IEquipable> OnPlayerUnEquipped;

    

    private PlayerInput playerInputAction;

    // ICharacter properties and fields
    private float health = 100f; // Default health
    public float Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0f, MaxHealth);
        }
    }

    public float MaxHealth => 100f;

    private IActivatable currentActivatable;

    private void Awake()
    {
        playerInputAction = GetComponent<PlayerInput>();
        artifactItems = new List<ArtifactItem>();
        abilitySystemComponent = GetComponent<AbilitySystemComponent>();
        weaponComponent = GetComponentInChildren<WeaponComponent>();
        hand = GetComponentInChildren<PlayerHandsComponent>();
        interactComponent = GetComponentInChildren<InteractComponent>();
        if (bodyTransform == null)
        {
            Debug.LogError("No Transform Set for Rotation");
            bodyTransform = gameObject.transform;
        }

        if (interactComponent == null)
        {
            Debug.LogError("Could not detect Interact Component on Character");
        }
        if (hand == null)
        {
            Debug.LogError("Could not detect Hand Movement Component on Character");
        }
        if(weaponComponent==null)
        {
            Debug.LogError("Could not detect Weapon Component on Character");
        }
        
        hand.Initialize(this);

        // Initialize to MovementState
        ChangeState(PlayerState.MovementState);
    }

    private void Start()
    {
        PlayerCharacterAttributeSet playerCharacterAttributeSet = GetComponent<PlayerCharacterAttributeSet>();
        
        
        ItemData testData = new ItemData {
            ID = "test_gun",
            Name = "Test Gun",
            Description = "A test ranged weapon",
            IconPath = "",
            Weight = 1.0f,
            Value = 100,
            MaxStackCount = 1
        };

        // Create a ranged weapon with basic parameters
        RangedWeaponItem testWeapon = new RangedWeaponItem(
            data: testData,
            //slotType: EquipmentSlot.Hands,
            damage: playerCharacterAttributeSet.BaseWeaponDamage.CurrentValue,
            ammoType: "9mm",
            maxMagazineAmmo: (int)playerCharacterAttributeSet.MaxAmmo.CurrentValue,
            reloadTime: 1.5f,
            fireCooldown: 0.4f,
            BaseSpreadAngle : playerCharacterAttributeSet.BaseSpreadAngle.CurrentValue
        );
        // Equip the weapon
        EquipItem(testWeapon);
        weaponComponent?.Initialize(testWeapon);

        foreach(ArtifactSO artifact in artifactsOnStart)
        {
            EquipItem(new ArtifactItem(artifact));
            
        }


        UnequipItem(artifactItems[0]);
        
    }

    private void Update()
    {
        var confirmAction = playerInputAction.actions["Confirm"]; 
        if (confirmAction != null && confirmAction.phase == InputActionPhase.Performed)
        {
            //Debug.Log("Key is being held down.");
            HoldUseItem(currentActivatable, ActivationTrigger.LeftMouse);
        }
    }

    public void ChangeState(PlayerState newState)
    {
        OnStateExitInternal(currentState);
        currentState = newState;
        OnStateEnterInternal(currentState);
    }

    private void OnStateEnterInternal(PlayerState state)
    {
        OnStateEnter?.Invoke(state);
        switch (state)
        {
            case PlayerState.MovementState:
                Debug.Log("Entering Movement State");
                hand.ChangeState(HandState.Lowered);
                break;

            case PlayerState.DoorOpeningState:
                Debug.Log("Entering Door Opening State");
                hand.ChangeState(HandState.Raised);
                break;

            case PlayerState.MenuState:
                Debug.Log("Entering Menu State");
                break;
        }
    }

    private void OnStateExitInternal(PlayerState state)
    {
        OnStateExit?.Invoke(state);
        switch (state)
        {
            case PlayerState.MovementState:
                Debug.Log("Exiting Movement State");
                break;

            case PlayerState.DoorOpeningState:
                Debug.Log("Exiting Door Opening State");
                break;

            case PlayerState.MenuState:
                Debug.Log("Exiting Menu State");
                break;
        }
    }

    public void OnLeftClickPerformed(InputAction.CallbackContext context)
    {
        if (currentState != PlayerState.MovementState || isMoving)
            return;
        if(currentActivatable == null)
            return;
        if (context.phase == InputActionPhase.Started)
        {
            
            BeginUseItem(currentActivatable, ActivationTrigger.LeftMouse);

        }
        else if (context.phase == InputActionPhase.Performed)
        {
            //HoldUseItem(currentActivatable, ActivationTrigger.LeftMouse);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            EndUseItem(currentActivatable, ActivationTrigger.LeftMouse);
        }
    }


    public void OnMovementPerformed(InputAction.CallbackContext context)
    {
        if (currentState != PlayerState.MovementState || isMoving)
            return;

        Vector2 inputDirection = context.ReadValue<Vector2>();
        HandleMovement(inputDirection);
    }

    private void HandleMovement(Vector2 inputDirection)
    {
        if (inputDirection.y != 0)
        {
            InteractInfo interactInfo = interactComponent.PerformInteractionCheck(GetClosestDirection(bodyTransform.forward) * inputDirection.y);
            if (interactInfo.InteractableObject == null)
            {
                StartCoroutine(Move(inputDirection.y));
            }
            else
            {
                if (interactInfo.InteractableObject.layer == LayerMask.NameToLayer("Obstacle")||!interactInfo.Interactable.CanInteract)
                {
                    StartCoroutine(Bump(inputDirection.y));
                }
                else if (interactInfo.InteractableObject.layer == LayerMask.NameToLayer("Interactable Obj"))
                {
                    if(inputDirection.y >0)
                    {
                        interactInfo.Interactable.Interact(this.gameObject);

                    }else{
                        StartCoroutine(Bump(inputDirection.y));
                    }
                }
            }
        }
        else if (inputDirection.x != 0)
        {
            // For turning, uncomment and use as needed:
            // StartCoroutine(Turn(inputDirection.x));
        }
    }

    public void OnDoorFullyOpened()
    {
        //
        HandleMovement(new Vector2(0, 1f));
        ChangeState(PlayerState.MovementState);
        
        
    }

    public void OnDoorFullyClosed()
    {
        ChangeState(PlayerState.MovementState);
    }

    private IEnumerator Move(float direction)
    {
        isMoving = true;
        Vector3 forward = Vector3.forward;
        Vector3 backward = Vector3.back;
        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;

        Vector3 currentForward = new Vector3(bodyTransform.forward.x, 0, bodyTransform.forward.z).normalized;
        Vector3 closestDirection = GetClosestDirection(currentForward, forward, backward, left, right);
        Vector3 moveDirection = closestDirection * Mathf.Sign(direction);

        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + moveDirection * moveDistance;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }

    public Vector3 GetClosestDirection(Vector3 currentForward, params Vector3[] directions)
    {
        float maxDot = float.MinValue;
        Vector3 closestDirection = Vector3.zero;

        foreach (var direction in directions)
        {
            float dot = Vector3.Dot(currentForward, direction);
            if (dot > maxDot)
            {
                maxDot = dot;
                closestDirection = direction;
            }
        }

        return closestDirection;
    }

    private Vector3 GetClosestDirection(Vector3 currentForward)
    {
        Vector3 forward = Vector3.forward;
        Vector3 backward = Vector3.back;
        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;

        float maxDot = float.MinValue;
        Vector3 closestDirection = Vector3.zero;
        Vector3[] directions = { forward, backward, left, right };

        foreach (var direction in directions)
        {
            float dot = Vector3.Dot(currentForward, direction);
            if (dot > maxDot)
            {
                maxDot = dot;
                closestDirection = direction;
            }
        }

        return closestDirection;
    }

    private IEnumerator Turn(float direction)
    {
        isMoving = true;

        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + (direction * 90), 0);

        while (elapsedTime < turnDuration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / turnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        isMoving = false;
    }

    private IEnumerator Bump(float direction)
    {
        isMoving = true;

        Vector3 originalPosition = bodyTransform.position;
        Vector3 bumpPosition = originalPosition + bodyTransform.forward * Mathf.Sign(direction) * bumpDistance;

        float elapsedTime = 0f;

        while (elapsedTime < bumpDuration)
        {
            transform.position = Vector3.Lerp(originalPosition, bumpPosition, elapsedTime / bumpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < bumpDuration)
        {
            transform.position = Vector3.Lerp(bumpPosition, originalPosition, elapsedTime / bumpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        isMoving = false;
    }

    public void OnInteractDoor(Door door)
    {
        currentDoor = door;
        ChangeState(PlayerState.DoorOpeningState);
    }

    // ICharacter Implementation
    public void AddHealth(float amount)
    {
        Health += amount;
    }

    public void TakeDamage(EventContext context)
    {
        Health -= context.HitData.FinalDamage;
        if (Health < 0f) Health = 0f;
    }

    public bool AddItem(IItem item)
    {
        // For now, always return true.
        // In a real game, you would add item to inventory.
        return true;
    }

    public bool RemoveItem(IItem item)
    {
        // For now, always return true.
        // In a real game, you would remove the item from inventory.
        return true;
    }

    public bool EquipItem(IEquipable item)
    {
        // Call Equip on the item. 
        // Optionally store a reference to the equipped item if you need to track it.
        var activatableItem = item as IActivatable;
        if (activatableItem != null)
        {
            currentActivatable = activatableItem;
        }
        ArtifactItem artifactItem = item as ArtifactItem;
        if (artifactItem != null)
        {
            artifactItems.Add(artifactItem);
        }
        item.Equip(this);
        OnPlayerEquipped?.Invoke(item);
        return true;
    }

    public bool UnequipItem(IEquipable item)
    {
        // Call Unequip on the item.
        item.Unequip(this);
        OnPlayerUnEquipped?.Invoke(item);

        var artifactItem = item as ArtifactItem;
        if (artifactItem != null)
        {
            artifactItems.Remove(artifactItem);
        }
        return true;
    }

    public void BeginUseItem(IActivatable item, ActivationTrigger trigger)
    {
        item.BeginUse(this, trigger);
    }

    public void HoldUseItem(IActivatable item, ActivationTrigger trigger)
    {
        item.HoldUse(this, trigger);
    }

    public void EndUseItem(IActivatable item, ActivationTrigger trigger)
    {
        item.EndUse(this, trigger);
    }

    public void ScrollUseItem(IActivatable item, float scrollDelta)
    {
        item.OnScroll(this, scrollDelta);
    }

    public int GetAmmoCount(string ammoType)
    {
        // For testing, return a large number
        return 50;
    }

    public void ConsumeAmmo(string ammoType, int amountToLoad)
    {
        // For testing, just log ammo consumption.
        Debug.Log($"Consumed {amountToLoad} rounds of {ammoType}.");
    }

    public AbilitySystemComponent GetAbilitySystemComponent()
    {
        return abilitySystemComponent;
    }
}
