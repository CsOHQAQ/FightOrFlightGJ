using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteractSubState : BaseState
{
    private PlayerCharacter player;
    private Door door;

    // Example rate for how fast the door changes per scroll step
    private float doorScrollRate = 0.05f;

    public DoorInteractSubState(IStateMachineEntity owner, StateMachine stateMachine, Door inDoor)
        : base(owner, stateMachine)
    {
        player = (PlayerCharacter)owner;
        door = inDoor;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter DoorInteractSubState");

        player.Hand.ChangeState(HandState.Raised);
        
        door.OnDoorFullyOpened += HandleDoorOpened;
        door.OnDoorFullyClosed += HandleDoorClosed;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        // Read the "Interact" scroll input each frame
        Vector2 scroll = player.GetComponent<PlayerInput>()
                            .actions["Interact"]
                            .ReadValue<Vector2>();
        float scrollY = scroll.y;

        // If we get scroll input, update door’s TargetOpenness 
        if (!Mathf.Approximately(scrollY, 0f))
        {
            door.SetTargetOpenness(door.TargetOpenness + (scrollY * doorScrollRate));
        }

        // Check if door is near closed, AND user is scrolling down ~ -1
        // (We pick -0.9 as a threshold since scroll might not be exactly -1.)
        //bool doorIsNearlyClosed = (door.CurrentOpenness <= 0.02f);
        //bool userIsPullingFully = (scrollY <= -0.9f);

        //if (doorIsNearlyClosed && userIsPullingFully)
        //{
        //    Debug.Log("Door is nearly closed, user scrolled down => Exit DoorInteractSubState");
        //    stateMachine.ChangeState(player.MovementParentState);
        //}
    }


    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit DoorInteractSubState");

        door.OnDoorFullyOpened -= HandleDoorOpened;
        door.OnDoorFullyClosed -= HandleDoorClosed;

        // Optionally revert hands
        player.Hand.ChangeState(HandState.Lowered);
    }

    private void HandleDoorOpened()
    {
        stateMachine.ChangeState(player.MovementParentState);
    }

    private void HandleDoorClosed()
    {
        stateMachine.ChangeState(player.MovementParentState);
    }

    public override void OnInteractInput(float scrollY)
    {
        if (!Mathf.Approximately(scrollY, 0f))
        {
            // push => TargetOpenness += rate
            door.SetTargetOpenness(door.TargetOpenness + scrollY * doorScrollRate);
        }
        bool doorIsNearlyClosed = (door.CurrentOpenness <= 0.02f);
        // If door nearly closed or fully open => exit
        if (doorIsNearlyClosed && scrollY<0f)
        {
            door.SetTargetOpenness(0f);
            stateMachine.ChangeState(player.MovementParentState);
        }
    }
}
