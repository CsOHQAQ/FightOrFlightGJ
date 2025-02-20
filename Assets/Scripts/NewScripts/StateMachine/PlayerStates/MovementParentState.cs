using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementParentState : BaseState
{
    // 引用当前子状态
    private BaseState currentSubState;

    // 可能的子状态
    public WalkSubState WalkSubState   { get; private set; }
    public SprintSubState SprintSubState { get; private set; }

    // 可扩展: 如果你想将“后退走路”拆成一个子状态，也行
    // public BackwardWalkSubState BackwardWalkSubState { get; private set; }

    public MovementParentState(IStateMachineEntity owner, StateMachine stateMachine)
        : base(owner, stateMachine)
    {
        // 初始化子状态
        WalkSubState   = new WalkSubState(owner, stateMachine, this);
        SprintSubState = new SprintSubState(owner, stateMachine, this);
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter MovementParentState");
        // 初始默认进入 Walk
        SetSubState(WalkSubState);
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        // 父状态统一处理对外部输入的检测，比如是否要切换到 受击、菜单、交互 等

        // 访问 player 强转
        var player = (PlayerCharacter)owner;

        // 若检测到打开菜单
        // if (某个键按下) => stateMachine.ChangeState(player.MenuState);

        // 若检测到受击(示例)
        // if (被打) => player.EnterTakeDamage(TakeDamageType.Light);

        // 让子状态执行其逻辑
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
        Debug.Log("Exit MovementParentState");
        currentSubState?.Exit();
    }

    // 用于内部子状态切换
    public void SetSubState(BaseState newSub)
    {
        currentSubState?.Exit();
        currentSubState = newSub;
        currentSubState.Enter();
    }
}
