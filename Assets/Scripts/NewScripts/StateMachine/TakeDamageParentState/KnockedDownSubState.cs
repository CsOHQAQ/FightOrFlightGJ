using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 倒地状态：角色被击倒，需要一定时间或条件起身
/// </summary>
public class KnockedDownSubState : BaseState
{
    private TakeDamageParentState parentState;

    private float knockdownTimer = 0f;
    private float knockdownDuration = 3f;
    // 假设倒地三秒后可重新站起

    private bool isTryingToStandUp = false;

    public KnockedDownSubState(IStateMachineEntity owner, StateMachine stateMachine, TakeDamageParentState parent)
        : base(owner, stateMachine)
    {
        parentState = parent;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter KnockedDownSubState");

        // 播放倒地动画；角色失去大部分操作
        knockdownTimer = 0f;
        isTryingToStandUp = false;

        // 也可直接将角色Y轴弄低，或设定相应的姿势
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        var player = (PlayerCharacter)owner;

        // 1) 计时
        knockdownTimer += Time.deltaTime;

        // 2) 等待倒地时间到或玩家按某个键想要起身
        if (knockdownTimer >= knockdownDuration)
        {
            // 可以允许玩家按键或自动起身
            // 这里演示自动起身
            StandUp();
        }
        else
        {
            // 如果想让玩家主动按下跳跃键来提前起身:
            // if (Input.GetKeyDown(KeyCode.Space)) {
            //     StandUp();
            // }
        }
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit KnockedDownSubState");
        // 离开倒地状态，角色回到可操作
    }

    /// <summary>
    /// 角色开始起身过程
    /// </summary>
    private void StandUp()
    {
        isTryingToStandUp = true;
        // 这里你可以播放“起身动画”，然后在动画结束时:
        var player = (PlayerCharacter)owner;
        stateMachine.ChangeState(player.MovementParentState);
        // 或回到IdleParentState，取决于设计
    }
}
