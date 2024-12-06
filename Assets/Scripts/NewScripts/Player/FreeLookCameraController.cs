using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class FreeLookCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float verticalSensitivity = 1f; // Vertical mouse sensitivity
    [SerializeField] private float horizontalSensitivity = 1f; // Horizontal mouse sensitivity
    [SerializeField] private float maxVerticalAngle = 80f; // Maximum upward/downward angle
    [SerializeField] private float minVerticalAngle = -80f; // Minimum downward/upward angle

    [Header("Horizontal Rotation Settings")]
    [SerializeField] private bool limitHorizontalRotation = false; // Whether or not to limit horizontal rotation
    [SerializeField] private float horizontalLimit = 90f; // Total horizontal rotation limit (same value for both directions)

    [Header("Control Settings")]
    [SerializeField] private Transform cameraTransform; // Assign the camera Transform explicitly if not on the same GameObject

    [Header("Door Adjustment Settings")]
    [SerializeField] private float maxCameraMovement = 1.3f; // Max distance camera moves forward when door is opening
    [SerializeField] private float maxOpenness = 0.8f; // Max openness value that affects camera movement

    [Header("Events")]
    [SerializeField] private UnityEvent onLookedToBottom; // Event to notify when the camera looks downward beyond the threshold
    [SerializeField] private UnityEvent onExitLookedToBottom; // Event to notify when the camera exits looking downward

    private Vector2 lookInput; // Stores the current input for looking
    private float verticalRotation = 0f; // Tracks the vertical rotation of the camera
    private float horizontalRotation = 0f; // Tracks the horizontal rotation of the player object

    private PlayerCharacter player;
    private bool hasLookedToBottom = false; // To prevent multiple triggers for the same downward movement
    private bool canRotateCamera = true;


    private void Start()
    {
        player = gameObject.transform.parent.GetComponent<PlayerCharacter>();
        if (player == null)
        {
            Debug.LogError("NO PLAYERCHARACTER FOUND on CameraController");
        }
        player.OnStateEnter += HandleStateEnter;
        player.OnStateExit += HandleStateExit;

        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // If no cameraTransform is assigned, use the camera on this GameObject
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        // Process the camera movement
        if (canRotateCamera)
        {
            switch (player.CurrentState)
            {
                case PlayerState.MovementState:
                    RotateCamera();
                    break;

                case PlayerState.DoorOpeningState:
                    
                    // Handle camera adjustment based on the door's openness
                    if (player.CurrentDoor != null)
                    {
                        RotateCameraWithCameraRotation();
                        AdjustCameraPositionBasedOnDoor();
                    }
                    break;
            }
        }


    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void RotateCamera()
    {
        // Apply sensitivity to the input
        Vector2 scaledInput = new Vector2(lookInput.x * horizontalSensitivity, lookInput.y * verticalSensitivity);

        // Horizontal rotation (rotate around Y-axis globally, on the parent object)
        horizontalRotation += scaledInput.x;

        // Clamp the horizontal rotation if the limit is enabled
        if (limitHorizontalRotation)
        {
            horizontalRotation = Mathf.Clamp(horizontalRotation, -horizontalLimit, horizontalLimit);
        }

        transform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);

        // Vertical rotation (rotate around X-axis locally, on the camera)
        verticalRotation -= scaledInput.y;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        CheckLookedToBottom();
    }

    private void RotateCameraWithCameraRotation()
    {
        if(player.CurrentDoor.CurrentOpenness<=0.5)
        {
            StartCoroutine(ShiftCamera(player.GetClosestDirection(transform.forward), 0.3f));
            return;
        }

        // Apply sensitivity to the input
        Vector2 scaledInput = new Vector2(lookInput.x * horizontalSensitivity, lookInput.y * verticalSensitivity);

        // Horizontal rotation (rotate the camera around the Y-axis locally)
        horizontalRotation += scaledInput.x;

        // Clamp the horizontal rotation if the limit is enabled
        if (limitHorizontalRotation)
        {
            horizontalRotation = Mathf.Clamp(horizontalRotation, -horizontalLimit, horizontalLimit);
        }
        
        cameraTransform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);

        verticalRotation -= scaledInput.y;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, cameraTransform.localRotation.eulerAngles.y, 0f);

        CheckLookedToBottom();
    }

    private void AdjustCameraPositionBasedOnDoor()
    {
        // Get the current openness of the door (range from 0 to maxOpenness)
        float doorOpenness = Mathf.Min(player.CurrentDoor.CurrentOpenness, maxOpenness);

        // Calculate movement amount based on the door's openness
        float movementAmount = doorOpenness * maxCameraMovement;

        // Determine forward direction based on the player's bodyTransform
        Vector3 forwardDirection = player.transform.forward.normalized;

        // Calculate the target camera position
        Vector3 targetPosition = player.transform.position + forwardDirection * movementAmount;

        // Smoothly interpolate to the target position
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, Time.deltaTime * 4f);
    }

    public void ResetHorizontalCameraAngle()
    {
        horizontalRotation = 0f;
        cameraTransform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f) * cameraTransform.localRotation;
    }

    private void CheckLookedToBottom()
    {
        float downwardThreshold = 70f;

        if (verticalRotation >= downwardThreshold && !hasLookedToBottom)
        {
            hasLookedToBottom = true;
            onLookedToBottom?.Invoke();
        }
        else if (verticalRotation < downwardThreshold && hasLookedToBottom)
        {
            hasLookedToBottom = false;
            onExitLookedToBottom?.Invoke();
        }
    }

    public void HandleStateEnter(PlayerState state)
    {
        if (state == PlayerState.DoorOpeningState)
        {
            canRotateCamera = false;
            StartCoroutine(ShiftCamera(player.GetClosestDirection(transform.forward), 0.3f));
            limitHorizontalRotation = true;
        }
    }

    public void HandleStateExit(PlayerState state)
    {
        if (state == PlayerState.DoorOpeningState)
        {
            canRotateCamera = false;
            StartCoroutine(ShiftCameraLocalPosition(Vector3.zero, 0.3f));
            limitHorizontalRotation = false;
            
        }
    }

    public IEnumerator ShiftCamera(Vector3 targetDirection, float duration)
    {
        Quaternion startCameraRotation = cameraTransform.localRotation;
        Quaternion startPlayerRotation = transform.localRotation;

        float targetHorizontalRotation = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float horizontalRotation = Mathf.LerpAngle(startPlayerRotation.eulerAngles.y, targetHorizontalRotation, elapsedTime / duration);
            transform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);

            float verticalRotation = Mathf.LerpAngle(startCameraRotation.eulerAngles.x, 0f, elapsedTime / duration);
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = Quaternion.Euler(0f, targetHorizontalRotation, 0f);
        cameraTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        canRotateCamera = true;
    }

    public IEnumerator ShiftCameraLocalPosition(Vector3 targetLocalPosition, float duration)
    {
        // Cache the initial local position of the camera
        Vector3 startLocalPosition = cameraTransform.localPosition;

        float elapsedTime = 0f;

        // Smoothly interpolate the camera's local position to the target
        while (elapsedTime < duration)
        {
            // Interpolate between the start and target local positions
            cameraTransform.localPosition = Vector3.Lerp(startLocalPosition, targetLocalPosition, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final local position is set to the target
        cameraTransform.localPosition = targetLocalPosition;
        canRotateCamera=true;
    }


    private void OnDestroy()
    {
        player.OnStateEnter -= HandleStateEnter;
        player.OnStateExit -= HandleStateExit;
    }
}
