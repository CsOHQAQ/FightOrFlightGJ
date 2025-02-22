using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class DoorInteractSubState : BaseState
{
    private PlayerCharacter player;
    private Door door;

    private float doorScrollRate = 0.05f;
    private Coroutine enterRoutine;
    private Coroutine closeRoutine; // For the "stepping in" after door closes

    public DoorInteractSubState(IStateMachineEntity owner, StateMachine stateMachine, Door inDoor)
        : base(owner, stateMachine)
    {
        player = (PlayerCharacter)owner;
        door   = inDoor;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter DoorInteractSubState");

        // Start our "EnterSequence" steps
        enterRoutine = player.StartCoroutine(EnterSequence());
    }

    private IEnumerator EnterSequence()
    {
        // 1) Move the hands to "Raised" or just wait until the door is half open, etc.
        //    For example:
        player.Hand.ChangeState(HandState.Raised);

        // 2) Smoothly shift the player to the door front
        Vector3 standPos = door.GetPlayerStandPosition(0.5f);
        yield return player.StartCoroutine(player.ShiftToPosition(standPos, 0.3f));

        // 3) Reset camera to door
        ResetCameraToDoor();

        // 4) Subscribe door events
        door.OnDoorFullyOpened += HandleDoorOpened;
        door.OnDoorFullyClosed += HandleDoorClosed;
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit DoorInteractSubState");

        // If the coroutines are still running, stop them
        if (enterRoutine != null)
        {
            player.StopCoroutine(enterRoutine);
            enterRoutine = null;
        }
        if (closeRoutine != null)
        {
            player.StopCoroutine(closeRoutine);
            closeRoutine = null;
        }

        // Unsubscribe from door events
        door.OnDoorFullyOpened -= HandleDoorOpened;
        door.OnDoorFullyClosed -= HandleDoorClosed;

        // Optionally revert hands
        player.Hand.ChangeState(HandState.Lowered);
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
    }

    public override void OnInteractInput(float scrollY)
    {
        if (!Mathf.Approximately(scrollY, 0f))
        {
            door.SetTargetOpenness(door.TargetOpenness + scrollY * doorScrollRate);
        }
        
        // Example: Also set camera offset based on door's CurrentOpenness if you want
        // player.FreeLookCameraController.DoorOffset = door.CurrentOpenness;
    }

    private void HandleDoorOpened()
    {
        // Door is fully open => exit sub-state if you want
        closeRoutine = player.StartCoroutine(HandleDoorClosedSequence());
    }

    private void HandleDoorClosed()
    {
        stateMachine.ChangeState(player.MovementParentState);
        
    }

    private IEnumerator HandleDoorClosedSequence()
    {
        
        // For example, we move the player inside the room by -0.5
        Vector3 standPos = door.GetPlayerStandPosition(0.5f,true);
        yield return player.StartCoroutine(player.ShiftToPosition(standPos, 0.4f));
        
        // After we finish stepping inside, we exit the sub-state
        stateMachine.ChangeState(player.MovementParentState);
    }

    private void ResetCameraToDoor()
    {
        // Example direct approach:
        var cameraController = player.FreeLookCameraController;

        Vector3 doorPos = door.DoorCenterPosition;
        Vector3 directionToDoor = (doorPos - player.transform.position).normalized;

        cameraController.StartCoroutine(cameraController.ShiftCamera(directionToDoor, 0.3f));
    }
}
