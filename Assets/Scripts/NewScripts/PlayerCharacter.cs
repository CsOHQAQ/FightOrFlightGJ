using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacter : MonoBehaviour
{
    private bool isMoving = false;
    private InteractComponent interactComponent;
    private PlayerHandsComponent hand;
    
    [SerializeField, Tooltip("Duration of the movement forward or backward in seconds.")]
    private float moveDuration = 1.0f;

    [SerializeField, Tooltip("Distance the player moves forward or backward.")]
    private float moveDistance = 1.0f;

    [SerializeField, Tooltip("Duration of the turn in seconds.")]
    private float turnDuration = 0.5f;

    [SerializeField, Tooltip("Distance the player moves for a bump effect.")]
    private float bumpDistance = 0.1f;

    [SerializeField, Tooltip("Duration of the bump animation in seconds.")]
    private float bumpDuration = 0.1f;

    private Door currentDoor;

    public enum PlayerState
    {
        MovementState,
        DoorOpeningState,
        MenuState
    }

    private PlayerState currentState;

    private void Awake()
    {
        hand = GetComponentInChildren<PlayerHandsComponent>();
        interactComponent = GetComponentInChildren<InteractComponent>();
        if (interactComponent == null)
        {
            Debug.LogError("Could not detect Interact Component on Character");
        }

        // Initialize to MovementState
        ChangeState(PlayerState.MovementState);
    }

    public void ChangeState(PlayerState newState)
    {
        // Call OnStateExit for the current state
        OnStateExit(currentState);

        // Change to the new state
        currentState = newState;

        // Call OnStateEnter for the new state
        OnStateEnter(currentState);
    }

    private void OnStateEnter(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.MovementState:
                Debug.Log("Entering Movement State");
                // Any initialization logic specific to MovementState
                break;

            case PlayerState.DoorOpeningState:
                Debug.Log("Entering Door Opening State");
                
                break;

            case PlayerState.MenuState:
                Debug.Log("Entering Menu State");
                // Disable player movement or display a menu
                break;
        }
    }

    private void OnStateExit(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.MovementState:
                Debug.Log("Exiting Movement State");
                // Clean up or finalize logic for MovementState if needed
                break;

            case PlayerState.DoorOpeningState:
                Debug.Log("Exiting Door Opening State");
                // Logic to finalize or reset after DoorOpeningState
                break;

            case PlayerState.MenuState:
                Debug.Log("Exiting Menu State");
                // Logic to hide menu or re-enable movement
                break;
        }
    }

    public void OnMovementPerformed(InputAction.CallbackContext context)
    {
        if (currentState != PlayerState.MovementState || isMoving)
            return; // Only allow movement in MovementState and when not moving

        Vector2 inputDirection = context.ReadValue<Vector2>();
        HandleMovement(inputDirection);

    }
    private void HandleMovement(Vector2 inputDirection)
    {
        if (inputDirection.y != 0) // Forward or backward
        {
            InteractInfo interactInfo = interactComponent.PerformInteractionCheck(gameObject.transform.forward * inputDirection.y);
            if (interactInfo.InteractableObject == null)
            {
                StartCoroutine(Move(inputDirection.y));
            }
            else
            {
                if (interactInfo.InteractableObject.layer == LayerMask.NameToLayer("Obstacle"))
                {
                    StartCoroutine(Bump(inputDirection.y));
                }
            }
        }
        else if (inputDirection.x != 0) // Turning left or right
        {
            StartCoroutine(Turn(inputDirection.x));
        }
    }


    public void OnDoorFullyOpened()
    {
        //Need to transit to Combat not movement state in the future. 
        ChangeState(PlayerState.MovementState);
    }

    public void OnDoorFullyClosed()
    {
        ChangeState(PlayerState.MovementState);
    }

    private IEnumerator Move(float direction)
    {
        isMoving = true;

        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + transform.forward * Mathf.Sign(direction) * moveDistance;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }

    private IEnumerator Turn(float direction)
    {
        isMoving = true;

        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + (direction * 90), 0);

        while (elapsedTime < turnDuration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / turnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        isMoving = false;
    }

    private IEnumerator Bump(float direction)
    {
        isMoving = true;

        Vector3 originalPosition = transform.position;
        Vector3 bumpPosition = originalPosition + transform.forward * Mathf.Sign(direction) * bumpDistance;

        float elapsedTime = 0f;

        while (elapsedTime < bumpDuration)
        {
            transform.position = Vector3.Lerp(originalPosition, bumpPosition, elapsedTime / bumpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < bumpDuration)
        {
            transform.position = Vector3.Lerp(bumpPosition, originalPosition, elapsedTime / bumpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        isMoving = false;
    }
    public void OnInteractDoor(Door door)
    {
        currentDoor = door;
        ChangeState(PlayerState.DoorOpeningState);
    }

}
