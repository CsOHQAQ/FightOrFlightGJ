using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Enemy : MonoBehaviour, IEnemyMoveable, ITriggerCheckable,ICharacter,IHitReceiver,IRoomObject
{
    // For Navigation Implementation
    NavMeshAgent agent;
    public NavMeshAgent Agent{get{return agent;}}
    public event Action<ICharacter> OnCharacterDied;
    [field: SerializeField] public float MaxHealth { get; set; }
    public float Health { get; set; }
    [SerializeField]
    private float speed = 1f;
    public float Speed{get{return speed;}}
    public Rigidbody RB { get; set; }

    public Faction Faction{ get{return Faction.ENEMY;} set{Faction = value;} }
    // Not Being Used
    [field: SerializeField] public float RotationSpeed { get; set; } = 5.0f;

    public bool IsAggroed { get; set; }
    public bool IsWithinStrikingDistance { get; set; }

    #region State Machine Variables

    public EnemyStateMachine StateMachine { get; set; }

    public EnemyIdleState IdleState { get; set; }

    public EnemyChaseState ChaseState { get; set; }

    public EnemyAttackState AttackState { get; set; }

    #endregion

    #region ScriptableObject Variables

    [SerializeField] private EnemyIdleSOBase EnemyIdleBase;
    [SerializeField] private EnemyChaseSOBase EnemyChaseBase;
    [SerializeField] private EnemyAttackSOBase EnemyAttackBase;

    public EnemyIdleSOBase EnemyIdleBaseInstance { get; set; }
    public EnemyChaseSOBase EnemyChaseBaseInstance { get; set; }
    public EnemyAttackSOBase EnemyAttackBaseInstance { get; set; }

    #endregion

    private void Awake()
    {
        EnemyIdleBaseInstance = Instantiate(EnemyIdleBase);
        EnemyChaseBaseInstance = Instantiate(EnemyChaseBase);
        EnemyAttackBaseInstance = Instantiate(EnemyAttackBase);


        StateMachine = new EnemyStateMachine();

        IdleState = new EnemyIdleState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        AttackState = new EnemyAttackState(this, StateMachine);
    }

    private void Start()
    {
        Health = MaxHealth;

        agent = GetComponentInChildren<NavMeshAgent>();
        RB = GetComponent<Rigidbody>();

        EnemyIdleBaseInstance.Initialize(gameObject, this);
        EnemyChaseBaseInstance.Initialize(gameObject, this);
        EnemyAttackBaseInstance.Initialize(gameObject, this);

        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.CurrentEnemyState.FrameUpdate();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentEnemyState.PhysicsUpdate();
    }

    #region Health/ / Die Functions

    public void AddHealth(float amount)
    {
        Health += amount;
    }

    public void TakeDamage(EventContext context)
    {
        Health -= context.HitData.FinalDamage;
        Debug.Log("Monster current health: " + Health);
        if (Health <= 0f) 
        {
            Health = 0f;
            CharacterDiedEventContext eventContext = new CharacterDiedEventContext(this,context.Source);
            Die();
            EventChainManager.Instance.ExecuteCharacterDiedChain(ref eventContext);
        }
        
    }


    public void Die()
    {
        Debug.Log( "Enemy "+ this.gameObject.name+" DIED");
        //TODO: Trigger Event Chain for death
        OnCharacterDied?.Invoke(this);
        
        //TODO: Play Death Animation and show corpse
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }
    #endregion


    #region Movement Functions

    public void MoveEnemy(Vector3 velocity)
    {
        RB.velocity = velocity;
    }

    #endregion

    #region Distance Checks

    public void SetAggroStatus(bool isAggroed)
    {
        IsAggroed = isAggroed;
    }

    public void SetStrikingDistanceBool(bool isWithinStrikingDistance)
    {
        IsWithinStrikingDistance = isWithinStrikingDistance;
    }

    #endregion

    #region Animation Triggers

    private void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {
        StateMachine.CurrentEnemyState.AnimationTriggerEvent(triggerType);
    }

    public enum AnimationTriggerType
    {
        EnemyDamaged,
        PlayFootstepSound
    }

    #endregion


    public AbilitySystemComponent GetAbilitySystemComponent()
    {
        return null;
    }

    public void OnCombatStartedInRoom(Room room)
    {
        //Get Activated and can start attacking plyer
        SetAggroStatus(true);
    }
    public void OnCombatEndedInRoom(Room room)
    {

    }
    public void OnHit(HitData hitData)
    {
        //Debug.Log("Got Hit on " + hitData.HitInfo.HitPoint);
    }
}
