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

public class PlayerCharacter : MonoBehaviour, IPlayerCharacter, IAbilitySystemComponent, IHitReceiver
{
    public event Action<ICharacter> OnCharacterDied;

    [SerializeField] ArtifactSO[] artifactsOnStart;
    private AbilitySystemComponent abilitySystemComponent;
    private WeaponComponent weaponComponent;

    /// <summary>
    /// List of all Enemies currently in the player's trigger overlap.
    /// </summary>
    private List<Enemy> overlappingEnemies = new List<Enemy>();
    public Faction Faction { get { return Faction.PLAYER; } }
    private List<ArtifactItem> artifactItems;

    private InteractComponent interactComponent;
    private PlayerHandsComponent hand;

    // -------------------- 新增或调整的字段 开始 --------------------
    [Header("First-Person Movement Settings (CharacterController)")]
    [SerializeField] private float moveSpeed = 4.0f;    // 玩家移动速度
    [SerializeField] private float rotationSpeed = 180f;// 若要用键盘左右旋转，可用此值

    // 存储 OnMovementPerformed 获取的输入 (x:左右, y:前后)
    private Vector2 moveInput;

    // 使用 CharacterController 而非 Rigidbody
    private CharacterController characterController;

    // -------------------- 新增或调整的字段 结束 --------------------

    [Header("Old Movement/Bump Settings (May keep for reference)")]
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

    private bool canActivate = true;
    public bool CanActivate
    {
        get => canActivate;
        set
        {
            if (canActivate != value)
            {
                canActivate = value;
                OnCanActivateChanged?.Invoke(canActivate);
            }
        }
    }
    public event Action<bool> OnCanActivateChanged;
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

    // 用于 Bump 协程时，标记是否在进行 Bump 动画
    private bool isMoving = false;

    private void Awake()
    {
        playerInputAction = GetComponent<PlayerInput>();
        artifactItems = new List<ArtifactItem>();
        abilitySystemComponent = GetComponent<AbilitySystemComponent>();
        weaponComponent = GetComponentInChildren<WeaponComponent>();
        hand = GetComponentInChildren<PlayerHandsComponent>();
        interactComponent = GetComponentInChildren<InteractComponent>();

        // -------------------- 获取 CharacterController 用于角色自由移动 --------------------
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("No CharacterController found on Player! Please add one in the Inspector.");
        }

        if (bodyTransform == null)
        {
            Debug.LogError("No Transform Set for bodyTransform; defaulting to this.transform.");
            bodyTransform = this.transform;
        }

        if (interactComponent == null)
        {
            Debug.LogError("Could not detect Interact Component on Character");
        }
        if (hand == null)
        {
            Debug.LogError("Could not detect Hand Movement Component on Character");
        }
        if (weaponComponent == null)
        {
            Debug.LogError("Could not detect Weapon Component on Character");
        }

        hand.Initialize(this);
    }

    private void Start()
    {
        // Initialize to MovementState
        ChangeState(PlayerState.MovementState);

        // 测试武器/装备示例
        PlayerCharacterAttributeSet playerCharacterAttributeSet = GetComponent<PlayerCharacterAttributeSet>();

        ItemData testData = new ItemData
        {
            ID = "test_gun",
            Name = "Test Gun",
            Description = "A test ranged weapon",
            IconPath = "",
            Weight = 1.0f,
            Value = 100,
            MaxStackCount = 1
        };

        RangedWeaponItem testWeapon = new RangedWeaponItem(
            data: testData,
            damage: playerCharacterAttributeSet.BaseWeaponDamage.CurrentValue,
            ammoType: "9mm",
            maxMagazineAmmo: (int)playerCharacterAttributeSet.MaxAmmo.CurrentValue,
            reloadTime: 1.5f,
            fireCooldown: 1f / playerCharacterAttributeSet.AttackSpeed.CurrentValue,
            BaseSpreadAngle: playerCharacterAttributeSet.BaseSpreadAngle.CurrentValue
        );
        EquipItem(testWeapon);
        weaponComponent?.Initialize(this, testWeapon);

        foreach (ArtifactSO artifact in artifactsOnStart)
        {
            EquipItem(new ArtifactItem(artifact));
        }
    }

    private void Update()
    {
        var confirmAction = playerInputAction.actions["Confirm"];
        if (confirmAction != null && confirmAction.phase == InputActionPhase.Performed)
        {
            if (canActivate)
            {
                HoldUseItem(currentActivatable, ActivationTrigger.LeftMouse);
            }
        }
    }

    private void FixedUpdate()
    {
        // 如果你想把移动放在 Update 里，也可以，但最好固定在一个地方，以免与物理冲突。
        // CharacterController 不完全是物理物体，但为了保持一致，这里放在 FixedUpdate 也行。
        if (currentState == PlayerState.MovementState && !isMoving && characterController)
        {
            // 1) 计算受敌人数量影响的减速因子
            float factor = GetSpeedFactorFromEnemies();

            // 2) 根据 input + bodyTransform.forward/right 计算移动向量 (x,z)
            Vector3 forward = bodyTransform.forward * moveInput.y;
            Vector3 right   = bodyTransform.right   * moveInput.x;
            Vector3 moveDirection = (forward + right).normalized * (moveSpeed * factor);

            // 如果要使用键盘左右旋转替代平移，可以注释掉 right 并在 Update 里做:
            // float turn = moveInput.x * rotationSpeed * Time.deltaTime;
            // bodyTransform.Rotate(0, turn, 0);

            // 3) 用 CharacterController.Move() 来移动，不会穿墙
            // 注意 CC.Move() expects "distance per frame", 所以乘以 Time.fixedDeltaTime
            characterController.Move(moveDirection * Time.fixedDeltaTime);
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
                this.CanActivate = true;
                break;

            case PlayerState.DoorOpeningState:
                Debug.Log("Entering Door Opening State");
                hand.ChangeState(HandState.Raised);
                this.CanActivate = false;
                break;

            case PlayerState.MenuState:
                Debug.Log("Entering Menu State");
                this.CanActivate = false;
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
        if (!canActivate) return;
        if (currentActivatable == null) return;

        if (context.phase == InputActionPhase.Started)
        {
            BeginUseItem(currentActivatable, ActivationTrigger.LeftMouse);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            EndUseItem(currentActivatable, ActivationTrigger.LeftMouse);
        }
    }

    public void OnMovementPerformed(InputAction.CallbackContext context)
    {
        if (currentState != PlayerState.MovementState)
            return;

        // 记录输入 (W/S -> y, A/D -> x)
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnDoorFullyOpened()
    {
        ChangeState(PlayerState.MovementState);
    }

    public void OnDoorFullyClosed()
    {
        ChangeState(PlayerState.MovementState);
    }

    /// <summary>
    /// Bump 协程保留，供未来功能使用
    /// </summary>
    private IEnumerator Bump(float direction)
    {
        // 假如你想在Bump期间禁止普通移动:
        isMoving = true;

        Vector3 originalPosition = bodyTransform.position;
        Vector3 bumpPosition = originalPosition + bodyTransform.forward * Mathf.Sign(direction) * bumpDistance;

        float elapsedTime = 0f;

        while (elapsedTime < bumpDuration)
        {
            float t = elapsedTime / bumpDuration;
            bodyTransform.position = Vector3.Lerp(originalPosition, bumpPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < bumpDuration)
        {
            float t = elapsedTime / bumpDuration;
            bodyTransform.position = Vector3.Lerp(bumpPosition, originalPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bodyTransform.position = originalPosition;
        isMoving = false;
    }

    private float GetSpeedFactorFromEnemies()
    {
        int count = OverlappingEnemyCount();
        if (count == 0)      return 1f;    // Full speed
        else if (count == 1) return 0.66f; // 66% speed
        else if (count == 2) return 0.30f; // 30% speed
        else                 return 0.10f; // 3 or more -> 10% speed
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
        Debug.Log("Player Health is now: " + Health);
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health < 0f) Health = 0f;
        Debug.Log("Player Health is now: " + Health);
    }

    public bool AddItem(IItem item)
    {
        // For now, always return true.
        return true;
    }

    public bool RemoveItem(IItem item)
    {
        // For now, always return true.
        return true;
    }

    public bool EquipItem(IEquipable item)
    {
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
        return 50; // For testing
    }

    public void ConsumeAmmo(string ammoType, int amountToLoad)
    {
        Debug.Log($"Consumed {amountToLoad} rounds of {ammoType}.");
    }

    public AbilitySystemComponent GetAbilitySystemComponent()
    {
        return abilitySystemComponent;
    }

    public void Die()
    {
        // Implement logic for dying.
    }

    public void OnHit(HitData hitData)
    {
        // Debug.Log("Got Hit on " + hitData.HitInfo.HitPoint);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && !overlappingEnemies.Contains(enemy))
            {
                overlappingEnemies.Add(enemy);
                enemy.OnCharacterDied += HandleEnemyDied;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && overlappingEnemies.Contains(enemy))
            {
                enemy.OnCharacterDied -= HandleEnemyDied;
                overlappingEnemies.Remove(enemy);
            }
        }
    }

    private void HandleEnemyDied(ICharacter dyingCharacter)
    {
        Enemy deadEnemy = dyingCharacter as Enemy;
        if (deadEnemy == null) return;

        deadEnemy.OnCharacterDied -= HandleEnemyDied;
        if (overlappingEnemies.Contains(deadEnemy))
        {
            overlappingEnemies.Remove(deadEnemy);
        }
    }

    public int OverlappingEnemyCount()
    {
        return overlappingEnemies.Count;
    }

    public List<Enemy> GetNearbyEnemies()
    {
        return overlappingEnemies;
    }

    public void OnInteractDoor(Door door)
    {
        currentDoor = door;
        ChangeState(PlayerState.DoorOpeningState);
    }
}
