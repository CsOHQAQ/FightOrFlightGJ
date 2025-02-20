using UnityEngine;

public class DoorInteractSubState : BaseState
{
    private PlayerCharacter player;
    private Door door;

    public DoorInteractSubState(IStateMachineEntity owner, StateMachine stateMachine, Door door)
        : base(owner, stateMachine)
    {
        this.player = (PlayerCharacter)owner;
        this.door = door;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter DoorInteractSubState");

        // Subscribe to door events:
        door.OnDoorFullyOpened += HandleDoorOpened;
        door.OnDoorFullyClosed += HandleDoorClosed;

        // Possibly lock input, or set some flags on player/camera
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        // For example, if player presses Cancel:
        // stateMachine.ChangeState(player.MovementParentState);
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit DoorInteractSubState");

        door.OnDoorFullyOpened -= HandleDoorOpened;
        door.OnDoorFullyClosed -= HandleDoorClosed;
    }

    private void HandleDoorOpened()
    {
        // Once door is fully open, maybe go back to MovementParentState
        stateMachine.ChangeState(player.MovementParentState);
    }

    private void HandleDoorClosed()
    {
        // If door closes on its own, also revert to movement or do something else
        stateMachine.ChangeState(player.MovementParentState);
    }
}
