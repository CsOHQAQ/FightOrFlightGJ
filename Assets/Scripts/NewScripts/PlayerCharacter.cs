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

    [SerializeField, Tooltip("Movement script uses the forward of this transform to determine where forward is for the character.")]
    private Transform bodyTransform;

    private Door currentDoor;
    public Door CurrentDoor{get{return currentDoor;}}

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
        if(bodyTransform==null)
        {
            Debug.LogError("No Transform Set for Rotation");
            bodyTransform=gameObject.transform;
        }

        if (interactComponent == null)
        {
            Debug.LogError("Could not detect Interact Component on Character");
        }
        if (hand == null)
        {
            Debug.LogError("Could not detect Hand Movement Component on Character");
        }
        hand.Initialize(this);

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
                hand.ChangeState(HandState.Lowered);
                // Any initialization logic specific to MovementState
                break;

            case PlayerState.DoorOpeningState:
                Debug.Log("Entering Door Opening State");
                hand.ChangeState(HandState.Raised);
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
            InteractInfo interactInfo = interactComponent.PerformInteractionCheck(GetClosestDirection(bodyTransform.forward) * inputDirection.y);
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
            //StartCoroutine(Turn(inputDirection.x));
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

        // Define the cardinal directions on the XZ plane
        Vector3 forward = Vector3.forward;  // Global forward
        Vector3 backward = Vector3.back;   // Global backward
        Vector3 left = Vector3.left;       // Global left
        Vector3 right = Vector3.right;     // Global right

        // Get the forward vector of the transform, projected onto the XZ plane
        Vector3 currentForward = new Vector3(bodyTransform.forward.x, 0, bodyTransform.forward.z).normalized;

        // Compare transform.forward to the cardinal directions and find the closest one
        Vector3 closestDirection = GetClosestDirection(currentForward, forward, backward, left, right);

        // Determine the target direction based on user input (forward or backward)
        Vector3 moveDirection = closestDirection * Mathf.Sign(direction);

        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + moveDirection * moveDistance;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
    
    private Vector3 GetClosestDirection(Vector3 currentForward, params Vector3[] directions)
    {
        float maxDot = float.MinValue;
        Vector3 closestDirection = Vector3.zero;

        foreach (var direction in directions)
        {
            // Compute the dot product between the current forward vector and the candidate direction
            float dot = Vector3.Dot(currentForward, direction);

            if (dot > maxDot)
            {
                maxDot = dot;
                closestDirection = direction;
            }
        }

        return closestDirection;
    }
    //The version that assumes the global directions. 
    private Vector3 GetClosestDirection(Vector3 currentForward)
    {
        // Define the cardinal directions on the XZ plane
        Vector3 forward = Vector3.forward;  // Global forward
        Vector3 backward = Vector3.back;   // Global backward
        Vector3 left = Vector3.left;       // Global left
        Vector3 right = Vector3.right;     // Global right

        // Initialize variables for tracking the closest direction
        float maxDot = float.MinValue;
        Vector3 closestDirection = Vector3.zero;

        // Iterate through the cardinal directions
        Vector3[] directions = { forward, backward, left, right };
        foreach (var direction in directions)
        {
            // Compute the dot product between the current forward vector and the candidate direction
            float dot = Vector3.Dot(currentForward, direction);

            if (dot > maxDot)
            {
                maxDot = dot;
                closestDirection = direction;
            }
        }

        return closestDirection;
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

        Vector3 originalPosition = bodyTransform.position;
        Vector3 bumpPosition = originalPosition + bodyTransform.forward * Mathf.Sign(direction) * bumpDistance;

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
