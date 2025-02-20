using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprintSubState : BaseState
{
    private MovementParentState parentState;
    public SprintSubState(IStateMachineEntity owner, StateMachine stateMachine, MovementParentState parent)
        : base(owner, stateMachine)
    {
        parentState = parent;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter SprintSubState");
        // 播放冲刺开始动画等等
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        var player = (PlayerCharacter)owner;

        // 如果松开Shift，则回到Walk
        if (/*Shift松开*/ false)
        {
            parentState.SetSubState(parentState.WalkSubState);
            return;
        }
        // 或检测体力不足 => 退出冲刺
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
        // 在此进行冲刺速度移动
        // float sprintSpeed = 6.0f or from player
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit SprintSubState");
    }
}
