using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;


public class PlayerCharacter : MonoBehaviour, 
                              IPlayerCharacter, 
                              IAbilitySystemComponent, 
                              IHitReceiver,
                              IStateMachineEntity
{
    public event Action<ICharacter> OnCharacterDied;

    [SerializeField] ArtifactSO[] artifactsOnStart;
    private AbilitySystemComponent abilitySystemComponent;
    public AbilitySystemComponent AbilitySystemComponent{get{return abilitySystemComponent;}}
    private WeaponComponent weaponComponent;

    /// <summary>
    /// List of all Enemies currently in the player's trigger overlap.
    /// </summary>
    private List<Enemy> overlappingEnemies = new List<Enemy>();
    public Faction Faction { get { return Faction.PLAYER; } }
    private List<ArtifactItem> artifactItems;

    private InteractComponent interactComponent;
    public InteractComponent InteractComponent { get { return interactComponent;}}
    private FreeLookCameraController freeLookCameraController;
    public FreeLookCameraController FreeLookCameraController { get { return freeLookCameraController;}}

    private PlayerHandsComponent hand;
    public PlayerHandsComponent Hand { get { return hand; } }
    [Header("First-Person Movement Settings (CharacterController)")]
    [SerializeField] private float moveSpeed = 4.0f;    // 玩家移动速度
    public float MoveSpeed { get { return moveSpeed; } }

    //[SerializeField] private float rotationSpeed = 180f; 
    [SerializeField]
    private Transform mainCameraTransform;
    public Transform MainCameraTransform{get {return mainCameraTransform;}}
    // 存储 OnMovementPerformed 获取的输入 (x:左右, y:前后)
    private Vector2 moveInput;
    public Vector2 MoveInput { get { return moveInput; } }

    // 使用 CharacterController 而非 Rigidbody
    public CharacterController characterController;

    [Header("Old Movement/Bump Settings (May keep for reference)")]
    [SerializeField] private float bumpDistance = 0.1f;
    [SerializeField] private float bumpDuration = 0.1f;

    /// <summary>
    /// Event fired whenever the player has equipped an item (e.g., a weapon).
    /// </summary>
    public event Action<IEquipable> OnPlayerEquipped;

    /// <summary>
    /// Event fired whenever the player has unequipped an item (e.g., a weapon).
    /// </summary>
    public event Action<IEquipable> OnPlayerUnEquipped;

    [SerializeField, Tooltip("Movement script uses the forward of this transform to determine where forward is for the character.")]
    private Transform bodyTransform;

    private Door currentDoor;
    public Door CurrentDoor { get { return currentDoor; } set{currentDoor = value;} }

    [Header("Push Settings")]
    // Expose these in your class for easy tuning
    [SerializeField] private float pushCapsuleDistance = 1.0f;   // Distance forward from player
    [SerializeField] private float pushCapsuleHeight   = 2.0f;   // Height of the capsule
    [SerializeField] private float pushCapsuleRadius   = 0.5f;   // Radius of the capsule

    // ----------------- 不再用 PlayerState 来驱动核心逻辑 -----------------
    // private PlayerState currentState;  // 移除或不用

    // HFSM: 顶层状态机
    public StateMachine BaseStateMachine { get; private set; }

    // 几个大状态
    public MovementParentState   MovementParentState   { get; private set; }
    public TakeDamageParentState TakeDamageParentState { get; private set; }
    public MenuState             MenuState             { get; private set; }
    public InteractState         InteractState         { get; private set; }

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

    #region Unity Lifecycle
    private void Awake()
    {
        // 常规初始化
        playerInputAction = GetComponent<PlayerInput>();
        artifactItems = new List<ArtifactItem>();
        abilitySystemComponent = GetComponent<AbilitySystemComponent>();
        weaponComponent = GetComponentInChildren<WeaponComponent>();
        hand = GetComponentInChildren<PlayerHandsComponent>();
        interactComponent = GetComponentInChildren<InteractComponent>();
        freeLookCameraController = GetComponentInChildren<FreeLookCameraController>();

        // 获取 CharacterController
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("No CharacterController found on Player!");
        }

        if (bodyTransform == null)
        {
            Debug.LogWarning("No Transform Set for bodyTransform; defaulting to this.transform.");
            bodyTransform = this.transform;
        }

        // 检测组件
        if (interactComponent == null)
            Debug.LogWarning("No InteractComponent found.");
        if (hand == null)
            Debug.LogWarning("No Hand Movement Component found.");
        if (weaponComponent == null)
            Debug.LogWarning("No Weapon Component found.");

        // 初始化手部
        hand?.Initialize(this);

        // 初始化 HFSM
        BaseStateMachine = new StateMachine();

        // 创建大状态对象
        MovementParentState   = new MovementParentState(this, BaseStateMachine);
        TakeDamageParentState = new TakeDamageParentState(this, BaseStateMachine);
        MenuState             = new MenuState(this, BaseStateMachine);
        InteractState         = new InteractState(this, BaseStateMachine);
    }

    private void Start()
    {
        // 测试武器/装备示例
        var playerCharacterAttributeSet = GetComponent<PlayerCharacterAttributeSet>();

        // 简单示例
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
        // 装备
        EquipItem(testWeapon);
        weaponComponent?.Initialize(this, testWeapon);
        playerCharacterAttributeSet.BaseSpreadAngle.OnValueChanged+=testWeapon.OnSpreadAngleChanged;
        // 装备一些artifact
        foreach (ArtifactSO artifact in artifactsOnStart)
        {
            EquipItem(new ArtifactItem(artifact));
        }

        // 初始化状态机, 默认进入移动大状态
        BaseStateMachine.Initialize(MovementParentState);
    }

    private void Update()
    {
        // 如果玩家按下Confirm并能交互
        var confirmAction = playerInputAction.actions["Confirm"];
        if (confirmAction != null && confirmAction.phase == InputActionPhase.Performed && canActivate)
        {
            HoldUseItem(currentActivatable, ActivationTrigger.LeftMouse);
        }

        // 让状态机自身Update
        BaseStateMachine.Update();
    }

    private void FixedUpdate()
    {
        // 不再在此直接处理移动, 而由 MovementParentState 或其子状态管理
        BaseStateMachine.FixedUpdate();
    }
    #endregion

    #region Input Callback

    // Movement 输入回调
    public void OnMovementPerformed(InputAction.CallbackContext context)
    {
        // 不再判断“if currentState == PlayerState.MovementState”
        // 任何时候都记录 input, 由 MovementParentState 决定是否采用
        moveInput = context.ReadValue<Vector2>();
    }

    // 左键交互
    public void OnLeftClickPerformed(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            // Let the current HFSM state handle the left-click "start"
            BaseStateMachine.CurrentState.OnLeftClickStarted();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            // Let the current HFSM state handle the left-click "canceled"
            BaseStateMachine.CurrentState.OnLeftClickCanceled();
        }
    }
    #endregion

    #region HFSM Entry Points

    /// <summary> 进入受击状态 </summary>
    public void EnterTakeDamage(TakeDamageType damageType)
    {
        TakeDamageParentState.SetDamageType(damageType);
        BaseStateMachine.ChangeState(TakeDamageParentState);
    }

    /// <summary> 进入菜单 </summary>
    public void EnterMenu()
    {
        BaseStateMachine.ChangeState(MenuState);
    }

    /// <summary> 进入互动状态(如推门) </summary>
    public void EnterInteract()
    {
        BaseStateMachine.ChangeState(InteractState);
    }

    #endregion

    #region ICharacter / Inventory / Other Logic

    // Bump 协程可保留, 仅供特定状态下调用
    private IEnumerator Bump(float direction)
    {
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
        if (count == 0)      return 1f;   
        else if (count == 1) return 0.66f; 
        else if (count == 2) return 0.30f; 
        else                 return 0.10f; 
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

    public bool AddItem(IItem item) { return true; }
    public bool RemoveItem(IItem item) { return true; }

    public bool EquipItem(IEquipable item)
    {
        var activatableItem = item as IActivatable;
        if (activatableItem != null) currentActivatable = activatableItem;

        var artifactItem = item as ArtifactItem;
        if (artifactItem != null) artifactItems.Add(artifactItem);

        item.Equip(this);
        OnPlayerEquipped?.Invoke(item);
        return true;
    }

    public bool UnequipItem(IEquipable item)
    {
        item.Unequip(this);
        OnPlayerUnEquipped?.Invoke(item);
        var artifactItem = item as ArtifactItem;
        if (artifactItem != null) artifactItems.Remove(artifactItem);
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

    public int GetAmmoCount(string ammoType) { return 50; }
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

    public void OnHit(HitData hitData) { }

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

    public int OverlappingEnemyCount() { return overlappingEnemies.Count; }
    public List<Enemy> GetNearbyEnemies() { return overlappingEnemies; }

    public void OnInteractDoor(Door door)
    {
        currentDoor = door;
        // 过去可能是ChangeState(PlayerState.DoorOpeningState)，
        // 现在改为 HFSM: InteractState
        EnterInteract();
    }
    #endregion

    // ICharacter Implementation
    public void AddHealth(float amount)
    {
        Health += amount;
    }

    /// <summary>
    /// Returns the "forward" direction aligned with the camera's yaw.
    /// This effectively ignores the camera's pitch/roll, so movement
    /// is purely horizontal in the direction the camera is facing.
    /// </summary>
    public Vector3 GetCameraYawForward()
    {
        if (mainCameraTransform == null) 
        {
            // fallback
            return transform.forward;
        }

        // Extract camera's yaw
        float yaw = mainCameraTransform.eulerAngles.y;

        // Build a flat rotation
        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);

        return rotation * Vector3.forward; 
    }

    /// <summary>
    /// Returns the "right" direction aligned with the camera's yaw.
    /// </summary>
    public Vector3 GetCameraYawRight()
    {
        if (mainCameraTransform == null)
        {
            return transform.right;
        }

        float yaw = mainCameraTransform.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);

        return rotation * Vector3.right;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Vector2 scroll = context.ReadValue<Vector2>();
        float scrollY = scroll.y;
        
        // Forward it to the HFSM
        BaseStateMachine.CurrentState.OnInteractInput(scrollY);
    }
    
    /// <summary>
    /// Smoothly moves the player's transform from current position 
    /// to the given targetPosition over 'duration' seconds.
    /// </summary>
    public IEnumerator ShiftToPosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, targetPosition, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final position is exact
        transform.position = targetPosition;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        //FreeLookCameraController = context.ReadValue<Vector2>();
        BaseStateMachine.CurrentState.OnLookInput(context.ReadValue<Vector2>());
    }

    public IActivatable GetCurrentActivatable()
    {
        return currentActivatable;
    }

    /// <summary>
    /// Called by PlayerHandsComponent to actually do the push sphere check
    /// and handle the closest IPushable found.
    /// </summary>
    public void PerformPushCapsuleCheck(int pushType)
{
    // 1) The bottom of the capsule is at the player's current position (player center).
    Vector3 bottomPoint = transform.position;

    // 2) The top is forward in the direction of the camera by pushCapsuleDistance.
    Vector3 forwardDir = MainCameraTransform.forward;  // from your player’s camera
    Vector3 topPoint   = bottomPoint + (forwardDir * pushCapsuleDistance);

    // 3) Filter out the "Player" layer from the overlap
    int layerMask = ~(1 << LayerMask.NameToLayer("Player"));

    // 4) OverlapCapsule to find colliders
    Collider[] hits = Physics.OverlapCapsule(bottomPoint, topPoint, pushCapsuleRadius, layerMask);


    float closestDistSqr = float.MaxValue;
    Collider closestCollider = null;

    // 5) Find the closest IHitReceiver
    foreach (Collider col in hits)
    {
        IHitReceiver hitReceiver = col.GetComponent<IHitReceiver>();
        if (hitReceiver != null)
        {
            // We'll measure distance from the midpoint of the capsule
            // A quick approach: just pick the average of bottomPoint & topPoint
            Vector3 capsuleCenter = (bottomPoint + topPoint) * 0.5f;
            float distSqr = (col.transform.position - capsuleCenter).sqrMagnitude;

            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closestCollider = col;
            }
        }
    }

    // 6) If we found something, build an EventContext to represent the push
    if (closestCollider != null)
    {
        IHitReceiver closestReceiver = closestCollider.GetComponent<IHitReceiver>();

        // Calculate push direction: from the capsule center to the collider
        Vector3 capsuleCenter = (bottomPoint + topPoint) * 0.5f;
        Vector3 pushDir       = (closestCollider.transform.position - capsuleCenter).normalized;
        // If you only want horizontal push, do:
        // pushDir.y = 0f; pushDir.Normalize();

        // Find a contact point on the collider
        Vector3 contactPoint = closestCollider.ClosestPoint(capsuleCenter);

        // Normal is from the contact point back to capsuleCenter
        Vector3 normal = (capsuleCenter - contactPoint).normalized;

        // Build the event context
        EventContext context = new EventContext
        {
            Source = this,  // The player pushing
            Target = closestReceiver,
            AttackInfo = new AttackData
            {
                PushType      = pushType,
                BaseDamage    = 0f,   // No damage
                PushStagger   = 5f,   // Some push "strength"
                PushDirection = pushDir
            },
            HitData = new HitData
            {
                FinalDamage = 0f,  // No damage
                HitInfo = new HitInfo
                {
                    HitPoint  = contactPoint,
                    HitNormal = normal
                }
            }
        };

        // Optionally invoke your chain:
        // EventChainManager.Instance.ExecuteAttackChain(ref context);

        Debug.Log($"Pushed object: {closestReceiver} with pushType={pushType}, " +
                  $"pushDir={pushDir}, contactPoint={contactPoint}");
    }
    else
    {
        Debug.Log("No pushable object found in capsule.");
    }
}


}
