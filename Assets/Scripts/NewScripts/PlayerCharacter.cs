using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacter : MonoBehaviour
{
    private bool isMoving = false;

    private InteractComponent interactComponent;

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

    private void Awake()
    {
        interactComponent = GetComponentInChildren<InteractComponent>();
        if(interactComponent==null)
        {
            Debug.LogError("Could not detect Interact Component on Character");
        }
    }

    // This function is called when movement input is performed
    public void OnMovementPerformed(InputAction.CallbackContext context)
    {
        Vector2 inputDirection = context.ReadValue<Vector2>();

        if (!isMoving)
        {
            if (inputDirection.y != 0) // Forward or backward
            {
                InteractInfo interactInfo = interactComponent.PerformInteractionCheck(gameObject.transform.forward * inputDirection.y);
                if (interactInfo.InteractableObject==null )
                {
                    StartCoroutine(Move(inputDirection.y));
                }else{
                    if(interactInfo.InteractableObject.layer == LayerMask.NameToLayer("Obstacle"))
                    {StartCoroutine(Bump(inputDirection.y));}
                }
            }
            else if (inputDirection.x != 0) // Turning left or right
            {
                StartCoroutine(Turn(inputDirection.x));
            }
        }
    }

    // Coroutine for moving forward or backward
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

    // Coroutine for turning (x = -1 for left, x = 1 for right)
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

    // Coroutine for bump animation (with direction input)
    private IEnumerator Bump(float direction)
    {
        isMoving = true;

        Vector3 originalPosition = transform.position;
        // Calculate bump position based on direction (forward or backward)
        Vector3 bumpPosition = originalPosition + transform.forward * Mathf.Sign(direction) * bumpDistance;

        float elapsedTime = 0f;

        // Move halfway into the direction
        while (elapsedTime < bumpDuration)
        {
            transform.position = Vector3.Lerp(originalPosition, bumpPosition, elapsedTime / bumpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Move back to original position
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

}
