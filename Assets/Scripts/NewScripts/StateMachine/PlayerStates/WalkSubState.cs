using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WalkSubState : BaseState
{
    
    PlayerCharacter player;
    private MovementParentState parentState;

    private GameplayEffectSpecHandle walkDebuffHandle;
    public WalkSubState(IStateMachineEntity owner, StateMachine stateMachine, MovementParentState parent)
        : base(owner, stateMachine)
    {
        parentState = parent;
        player = owner as PlayerCharacter;
    }

    public override void Enter()
    {
        base.Enter();
        
        player.CanActivate = true;
        Debug.Log("Enter WalkSubState");
        //RemoveEffectSpec(GameplayEffectSpecHandle handle)
        //walkDebuffHandle = new GameplayEffectSpecHandle();
        
        
        
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        // 1) Check if left click input is "Performed" 
        //    (meaning it's actively held down)
        var leftClickAction = player.PlayerInputAction.actions["Confirm"]; 
        // We'll define a helper below to get the action by name

        if (leftClickAction != null && leftClickAction.phase == InputActionPhase.Performed)
        {
            // 2) If we have an activatable item, call HoldUse each frame
            IActivatable item = player.GetCurrentActivatable();
            if (item != null && player.CanActivate)
            {
                item.HoldUse(player, ActivationTrigger.LeftMouse);
            }
        }

        // [Optionally] do other logic for sub‐state...
        // e.g. handle shift -> sprint, etc.
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();

        // The usual movement logic
        float finalSpeed = player.MoveSpeed;
        if (player.MoveInput.y < 0f)
            finalSpeed *= 0.5f;

        Vector3 forward = player.GetCameraYawForward() * player.MoveInput.y;
        Vector3 right   = player.GetCameraYawRight()   * player.MoveInput.x;
        Vector3 movementDir = (forward + right).normalized * finalSpeed;

        if (player.characterController && movementDir.magnitude > 0f)
        {
            player.characterController.Move(movementDir * Time.fixedDeltaTime);

            // Possibly apply your walkDebuff if moving...
            if (walkDebuffHandle.HandleID == 0)
            {
                walkDebuffHandle = player.AbilitySystemComponent
                    .ApplyEffectToSelf(StateConfig.Instance.WalkDebuffEffect, 1f);
            }
        }
        else
        {
            // If not moving, remove the effect
            if (walkDebuffHandle.HandleID != 0)
            {
                player.AbilitySystemComponent.RemoveEffectSpec(walkDebuffHandle);
                walkDebuffHandle = new GameplayEffectSpecHandle();
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.CanActivate = false;
        Debug.Log("Exit WalkSubState");

        // If we want to remove the walkDebuff on exit:
        if (walkDebuffHandle.HandleID != 0)
        {
            player.AbilitySystemComponent.RemoveEffectSpec(walkDebuffHandle);
            walkDebuffHandle = new GameplayEffectSpecHandle();
        }
    }

    // ----------------------------------------------------------------------
    // LEFT-CLICK OVERRIDES
    // ----------------------------------------------------------------------
    public override void OnLeftClickStarted()
    {
        base.OnLeftClickStarted();

        IActivatable item = player.GetCurrentActivatable(); 
        if (item != null && player.CanActivate)
        {
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

        IActivatable item = player.GetCurrentActivatable();
        if (item != null)
        {
            item.EndUse(player, ActivationTrigger.LeftMouse);
        }
    }
}
