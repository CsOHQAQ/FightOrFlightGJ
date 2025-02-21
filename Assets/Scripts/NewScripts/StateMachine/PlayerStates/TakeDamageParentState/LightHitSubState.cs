using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightHitSubState : BaseState
{
    private TakeDamageParentState parentState;

    public LightHitSubState(IStateMachineEntity owner, StateMachine stateMachine, TakeDamageParentState parent)
        : base(owner, stateMachine)
    {
        parentState = parent;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter LightHitSubState");
        // 播放轻伤动画
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        // 如果动画播完 or 一定时间后 => 回到Movement？
        // ((PlayerCharacter)owner).BaseStateMachine.ChangeState(((PlayerCharacter)owner).MovementParentState);
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit LightHitSubState");
    }
}
