using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractState : BaseState
{
    private DoorInteractSubState doorInteractSubState;
    private BaseState currentSubState;

    public InteractState(IStateMachineEntity owner, StateMachine stateMachine)
        : base(owner, stateMachine)
    {
        // We can create sub-states here or create them dynamically 
        // once we know which door is being opened
    }

    public void BeginDoorInteraction(Door door)
    {
        doorInteractSubState = new DoorInteractSubState(owner, stateMachine, door);
        SetSubState(doorInteractSubState);
    }
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter InteractState");
        // 执行交互动作，比如推门
        // Player不能移动
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        // 等推门完毕 => stateMachine.ChangeState(((PlayerCharacter)owner).MovementParentState);
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit InteractState");
        // 允许移动
    }

    public void SetSubState(BaseState newSub)
    {
        currentSubState?.Exit();
        currentSubState = newSub;
        currentSubState.Enter();
    }
}