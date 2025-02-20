using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TakeDamageType
{
    Light,
    Heavy,
    KnockedDown
}

public class TakeDamageParentState : BaseState
{
    private BaseState currentSubState;
    private TakeDamageType damageType;

    // 子状态
    public LightHitSubState LightHitSubState { get; private set; }
    public HeavyHitSubState HeavyHitSubState { get; private set; }
    public KnockedDownSubState KnockedDownSubState { get; private set; }

    public TakeDamageParentState(IStateMachineEntity owner, StateMachine stateMachine)
        : base(owner, stateMachine)
    {
        LightHitSubState = new LightHitSubState(owner, stateMachine, this);
        HeavyHitSubState = new HeavyHitSubState(owner, stateMachine, this);
        KnockedDownSubState = new KnockedDownSubState(owner, stateMachine, this);
    }

    // 由 PlayerCharacter 在进入此状态之前赋值
    public void SetDamageType(TakeDamageType type)
    {
        damageType = type;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter TakeDamageParentState");

        // 根据受击类型，进入对应子状态
        switch(damageType)
        {
            case TakeDamageType.Light:
                SetSubState(LightHitSubState);
                break;
            case TakeDamageType.Heavy:
                SetSubState(HeavyHitSubState);
                break;
            case TakeDamageType.KnockedDown:
                SetSubState(KnockedDownSubState);
                break;
        }
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        // 父状态可能检查某些通用条件，如玩家死亡？或恢复到Movement？

        currentSubState?.UpdateLogic();
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
        currentSubState?.UpdatePhysics();
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit TakeDamageParentState");
        currentSubState?.Exit();
    }

    // 子状态切换方法
    public void SetSubState(BaseState newSub)
    {
        currentSubState?.Exit();
        currentSubState = newSub;
        currentSubState.Enter();
    }
}
