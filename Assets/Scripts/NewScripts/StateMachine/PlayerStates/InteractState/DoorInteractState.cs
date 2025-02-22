using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class DoorInteractSubState : BaseState
{
    private PlayerCharacter player;
    private Door door;

    // Example rate for how fast the door changes per scroll step
    private float doorScrollRate = 0.05f;
    private Coroutine enterRoutine;
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

        enterRoutine = player.StartCoroutine(EnterSequence());
    }

    // This coroutine does the step-by-step approach:
    private IEnumerator EnterSequence()
    {
        // 1) Move the hands to "Raised"
        

        // 2) Smoothly shift the player to the door front
        Vector3 standPos = door.GetPlayerStandPosition(0.5f);
        yield return player.StartCoroutine(player.ShiftToPosition(standPos, 0.3f));

        // 3) Reset camera to door
        ResetCameraToDoor();
        player.Hand.ChangeState(HandState.Raised);
        // 4) Subscribe door events
        door.OnDoorFullyOpened += HandleDoorOpened;
        door.OnDoorFullyClosed += HandleDoorClosed;
    }
    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit DoorInteractSubState");

        // If the coroutine is still running, stop it
        if (enterRoutine != null)
        {
            player.StopCoroutine(enterRoutine);
            enterRoutine = null;
        }

        door.OnDoorFullyOpened -= HandleDoorOpened;
        door.OnDoorFullyClosed -= HandleDoorClosed;

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
        player.FreeLookCameraController.DoorOffset = door.CurrentOpenness;
        if (doorIsNearlyClosed && scrollY<0f)
        {
            door.SetTargetOpenness(0f);
            stateMachine.ChangeState(player.MovementParentState);
        }
    }


    private void ResetCameraToDoor()
    {
        // Option A) Directly snap the camera to face the door
        //     i.e. camera local rotation = (0,0,0) after adjusting the player
        // Option B) Use your existing "ShiftCamera" coroutine

        // Let’s do a direct approach:
        Vector3 doorPos = door.DoorCenterPosition;
        Vector3 toDoor = (doorPos - player.transform.position).normalized;

        // 1) Flatten or not
        toDoor.y = 0f;
        /*
        // 2) compute desired Y rotation for the player camera
        float desiredYaw = Mathf.Atan2(toDoor.x, toDoor.z) * Mathf.Rad2Deg;
        
        // We'll forcibly set the camera's horizontalRotation & verticalRotation:
        
        if (cameraController != null)
        {
            cameraController.SetRotation(desiredYaw, 0f); 
            // a hypothetical method "SetRotation(float yaw, float pitch)"
        }
        */
        var cameraController = player.FreeLookCameraController;
        
        Vector3 directionToDoor = (doorPos - player.transform.position).normalized;
        cameraController.StartCoroutine(cameraController.ShiftCamera(directionToDoor, 0.3f));
    }
}
