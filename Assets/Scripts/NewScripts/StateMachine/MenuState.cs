using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuState : BaseState
{
    public MenuState(IStateMachineEntity owner, StateMachine stateMachine) 
        : base(owner, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter MenuState");
        // 暂停游戏？显示菜单UI？
        // ((PlayerCharacter)owner).ShowMenuUI();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        // 如果玩家按下退出菜单按键 => 回到Movement
        // if (Input.GetKeyDown(KeyCode.Escape)) {
        //   stateMachine.ChangeState(((PlayerCharacter)owner).MovementParentState);
        // }
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit MenuState");
        // 关闭菜单UI
    }
}

