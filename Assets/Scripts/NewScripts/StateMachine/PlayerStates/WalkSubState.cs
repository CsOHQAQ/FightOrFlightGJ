using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkSubState : BaseState
{
    private MovementParentState parentState;
    public WalkSubState(IStateMachineEntity owner, StateMachine stateMachine, MovementParentState parent)
        : base(owner, stateMachine)
    {
        parentState = parent;
    }

    public override void Enter()
    {
        base.Enter();
        var player = owner as PlayerCharacter;
        player.CanActivate = true;
        Debug.Log("Enter WalkSubState");
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        var player = (PlayerCharacter)owner;

        // 如果按住Shift，则切到Sprint
        //if (/* 检测到Shift */ false)
        //{
        //    parentState.SetSubState(parentState.SprintSubState);
        //    return;
        //}

        // 如果想把“向后走速度更慢”也细化成一个子状态，也可以 parentState.SetSubState(backwardState)
        // 否则只需在此子状态中根据移动方向y<0时调低速度
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
        var player = (PlayerCharacter)owner;

        // If pressing backward, maybe reduce speed
        float finalSpeed = player.MoveSpeed;
        if (player.MoveInput.y < 0f)
            finalSpeed *= 0.5f;

        // Use the camera-based yaw forward/right
        Vector3 forward = player.GetCameraYawForward() * player.MoveInput.y;
        Vector3 right   = player.GetCameraYawRight()   * player.MoveInput.x;
        Vector3 movementDir = (forward + right).normalized * finalSpeed;

        if (player.characterController)
        {
            player.characterController.Move(movementDir * Time.fixedDeltaTime);
        }
    }

    public override void Exit()
    {
        base.Exit();
        var player = owner as PlayerCharacter;
        player.CanActivate = false;
        Debug.Log("Exit WalkSubState");
    }
    // ----------------------------------------------------------------------
    // LEFT-CLICK OVERRIDES
    // ----------------------------------------------------------------------
    public override void OnLeftClickStarted()
    {
        base.OnLeftClickStarted();
        
        var player = (PlayerCharacter)owner;

        // Example logic: If the player has a currentActivatable (like a gun), shoot
        IActivatable item = player.GetCurrentActivatable(); 
        if (item != null)
        {
            // Or check player.CanActivate, etc.
            item.BeginUse(player, ActivationTrigger.LeftMouse);
        }
        else
        {
            Debug.Log("No item to fire.");
        }
    }

    public override void OnLeftClickCanceled()
    {
        base.OnLeftClickCanceled();
        
        var player = (PlayerCharacter)owner;
        IActivatable item = player.GetCurrentActivatable();
        if (item != null)
        {
            item.EndUse(player, ActivationTrigger.LeftMouse);
        }
    }
}
