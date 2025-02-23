using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushSubState : BaseState
{
    private PlayerCharacter player;
    private Animator handAnimator;

    // The name of the push animation in your Animator Controller.
    // Adjust to match your actual animation state name.
    private const string PUSH_ANIMATION_STATE = "Pushing";

    // The name of the bool parameter in your Animator Controller.
    // If you prefer a Trigger, see the notes below.
    private const string PUSH_BOOL_PARAM = "IsPushing";

    public PushSubState(IStateMachineEntity owner, StateMachine stateMachine)
        : base(owner, stateMachine)
    {
        // Typically just store references in constructor if needed
        player = (PlayerCharacter)owner;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter Push State");
        // 1) Grab the PlayerCharacter reference
        

        // 2) Retrieve the Animator on the player’s hand (or the relevant object)
        //    This might be `player.Hand.Animator`, or `player.Hand.GetComponent<Animator>()`, etc.
        //    Adjust to match your actual code:
        if (player.Hand != null)
        {
            // Example: if you have an Animator field in PlayerHandsComponent
            // handAnimator = player.Hand.Animator;
            // or if not, do:
            handAnimator = player.Hand.GetComponent<Animator>();
        }

        // 3) Trigger the push animation
        // Option A: Using a bool
        if (handAnimator != null)
        {
            handAnimator.SetBool(PUSH_BOOL_PARAM, true);
        }
        // Option B: Using a trigger instead:
        // handAnimator.SetTrigger("PushTrigger");
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        // If we have an Animator and want to detect the animation is done:
        if (handAnimator != null)
        {
            AnimatorStateInfo stateInfo = handAnimator.GetCurrentAnimatorStateInfo(0);
            // Check if we are in the push animation AND have passed 1.0 normalized time
            if (stateInfo.IsName(PUSH_ANIMATION_STATE) && stateInfo.normalizedTime >= 1f)
            {
                // Once the animation is complete, go back to walk or parent movement state
                // e.g. if you have a reference to the WalkSubState or MovementParentState:
                // stateMachine.ChangeState(player.MovementParentState);
                
                // If you have a parent 'InteractState' with sub-states, 
                // you might revert to the parent state's default sub-state:
                // parentState.SetSubState(parentState.DefaultSubState);

                // For simplicity, let's assume you want to go back to MovementParentState:
                stateMachine.ChangeState(player.MovementParentState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();

        // Reset the bool if we used one
        if (handAnimator != null)
        {
            handAnimator.SetBool(PUSH_BOOL_PARAM, false);
        }

        Debug.Log("Exiting PushSubState.");
    }
}
