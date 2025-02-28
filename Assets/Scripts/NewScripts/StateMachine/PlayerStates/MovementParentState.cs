using UnityEngine;
using UnityEngine.InputSystem;

public class MovementParentState : BaseState
{
    private PlayerCharacter player;
    private BaseState currentSubState;

    // Sub-states
    public WalkSubState   WalkSubState   { get; private set; }
    public SprintSubState SprintSubState { get; private set; }
    public PushSubState   PushSubState   { get; private set; }  // <-- Add this

    public MovementParentState(IStateMachineEntity owner, StateMachine stateMachine)
        : base(owner, stateMachine)
    {
        player = (PlayerCharacter)owner;
        // Initialize sub-states
        WalkSubState   = new WalkSubState(owner, stateMachine, this);
        SprintSubState = new SprintSubState(owner, stateMachine, this);
        //PushSubState   = new PushSubState(owner, stateMachine, this); // <-- Initialize
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter MovementParentState");
        // Default to walk
        SetSubState(WalkSubState);
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
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

    public void SetSubState(BaseState newSub)
    {
        currentSubState?.Exit();
        currentSubState = newSub;
        currentSubState.Enter();
    }

    public override void OnInteractInput(float scrollY)
    {
        // If the user isn't actually scrolling, do nothing
        if (Mathf.Approximately(scrollY, 0f)) return;

        // Attempt a raycast for an interactable
        Vector3 direction = player.MainCameraTransform.forward;

        InteractInfo info = player.InteractComponent
            .PerformInteractionCheck(player.gameObject, direction, scrollY);

        if(scrollY > 0f)
        {
            if (info.Interactable is Door door)
            {
                if (door.CanInteract)
                {
                    // e.g. Door sub-state or a direct state
                    var doorSub = new DoorInteractSubState(player, stateMachine, door);
                    player.CurrentDoor = door;
                    door.Interact(info);
                    stateMachine.ChangeState(doorSub);
                }
                else
                {
                    Debug.Log("Door is not interactable right now.");
                }
            }
            else if (info.Interactable != null)
            {
                // Some other interactable (button, item, etc.)
                info.Interactable.Interact(info);
            }
            else
            {
                
                // No interactable found, but user scrolled forward => go to push sub-state
                Debug.Log("No interactable found. Entering PushSubState for a quick push action.");
                var pushSub = new PushSubState(player, stateMachine);
                stateMachine.ChangeState(pushSub);
            }
        }

    }

    public override void OnLookInput(Vector2 lookInput)
    {
        // Pass the look input to your camera controller
        player.FreeLookCameraController.LookInput = lookInput;
    }

    public override void OnLeftClickStarted()
    {
        base.OnLeftClickStarted();
        currentSubState?.OnLeftClickStarted();
    }

    public override void OnLeftClickCanceled()
    {
        base.OnLeftClickCanceled();
        currentSubState?.OnLeftClickCanceled();
    }
}
