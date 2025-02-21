using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 代表角色受到重击，但尚未倒地的状态；可有较长硬直或特殊动画
/// </summary>
public class HeavyHitSubState : BaseState
{
    private TakeDamageParentState parentState;

    // 记录该状态持续时间或动画时长
    private float hitTimer = 0f;
    private float heavyHitDuration = 1.5f; 
    // 假设重击动画约 1.5s

    public HeavyHitSubState(IStateMachineEntity owner, StateMachine stateMachine, TakeDamageParentState parent)
        : base(owner, stateMachine)
    {
        parentState = parent;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter HeavyHitSubState");

        // 播放“重击”动画，禁用移动/操作等
        // 也可对玩家的移动速度设0，或与CharacterController交互

        hitTimer = 0f;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        var player = (PlayerCharacter)owner;

        // 计时：当动画时间超过 heavyHitDuration，则退出受击，回到移动或别的状态
        hitTimer += Time.deltaTime;
        if (hitTimer >= heavyHitDuration)
        {
            // 若角色没有死亡，则回到Movement或Idle
            // 例如:
            stateMachine.ChangeState(player.MovementParentState);
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit HeavyHitSubState");
        // 重击结束后，恢复可操作，或根据需求做别的处理
    }
}
