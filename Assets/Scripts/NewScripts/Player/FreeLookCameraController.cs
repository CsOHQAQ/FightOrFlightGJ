using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class FreeLookCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float verticalSensitivity = 1f;
    [SerializeField] private float horizontalSensitivity = 1f;
    [SerializeField] private float maxVerticalAngle = 80f;
    [SerializeField] private float minVerticalAngle = -80f;

    [SerializeField] private float centerCameraDuration = 0.6f;

    [Header("Horizontal Rotation Settings")]
    [SerializeField] private bool limitHorizontalRotation = false;
    [SerializeField] private float horizontalLimit = 90f;

    [Header("Control Settings")]
    [SerializeField] private Transform cameraTransform;

    [Header("Door Adjustment Settings")]
    [SerializeField] private float maxCameraMovement = 1.3f;
    [SerializeField] private float maxOpenness = 0.8f;

    [Header("Events")]
    [SerializeField] private UnityEvent onLookedToBottom;
    [SerializeField] private UnityEvent onExitLookedToBottom;

    private Vector2 lookInput;
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;

    private PlayerCharacter player;
    private bool hasLookedToBottom = false;
    private bool canRotateCamera = true;

    private void Start()
    {
        player = gameObject.transform.parent.GetComponent<PlayerCharacter>();
        if (player == null)
        {
            Debug.LogError("NO PLAYERCHARACTER FOUND on CameraController");
        }

        // Remove references to player.OnStateEnter / OnStateExit 
        // because we no longer use the old PlayerState-based events.

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (!canRotateCamera) return;

        // Query the HFSM's current state
        var current = player.BaseStateMachine.CurrentState;

        // Example usage: if in MovementParentState => standard free look
        if (current is MovementParentState)
        {
            RotateCamera();
        }
        // If in some InteractState (like DoorInteractState), do something else
        else if (current is InteractState)
        {
            RotateCameraWithCameraRotation();

            // If relevant, adjust camera position with door logic 
            // (assuming 'player.CurrentDoor' is valid if Interacting with a door)
            if (player.CurrentDoor != null)
            {
                AdjustCameraPositionBasedOnDoor();
            }
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void RotateCamera()
    {
        Vector2 scaledInput = new Vector2(lookInput.x * horizontalSensitivity, lookInput.y * verticalSensitivity);

        // Horizontal rotation
        horizontalRotation += scaledInput.x;
        if (limitHorizontalRotation)
        {
            horizontalRotation = Mathf.Clamp(horizontalRotation, -horizontalLimit, horizontalLimit);
        }
        transform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);

        // Vertical rotation
        verticalRotation -= scaledInput.y;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        CheckLookedToBottom();
    }

    private void RotateCameraWithCameraRotation()
    {
        // Example logic used when interacting with a door
        // Possibly center camera if door hasn't opened enough
        if (player.CurrentDoor != null && player.CurrentDoor.CurrentOpenness <= 0.5f)
        {
            Vector3 directionToDoor = (player.CurrentDoor.transform.position - player.transform.position).normalized;
            StartCoroutine(ShiftCamera(directionToDoor, centerCameraDuration));
            return;
        }

        // Otherwise, let player aim, but maybe a different transform approach
        Vector2 scaledInput = new Vector2(lookInput.x * horizontalSensitivity, lookInput.y * verticalSensitivity);

        // Horizontal
        horizontalRotation += scaledInput.x;
        if (limitHorizontalRotation)
        {
            horizontalRotation = Mathf.Clamp(horizontalRotation, -horizontalLimit, horizontalLimit);
        }
        cameraTransform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);

        // Vertical
        verticalRotation -= scaledInput.y;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        cameraTransform.localRotation =
            Quaternion.Euler(verticalRotation, cameraTransform.localRotation.eulerAngles.y, 0f);

        CheckLookedToBottom();
    }

    private void AdjustCameraPositionBasedOnDoor()
    {
        float doorOpenness = Mathf.Min(player.CurrentDoor.CurrentOpenness, maxOpenness);
        float movementAmount = doorOpenness * maxCameraMovement;

        Vector3 forwardDirection = transform.forward.normalized;
        Vector3 targetPosition = player.transform.position + forwardDirection * movementAmount;

        cameraTransform.position = Vector3.Lerp(
            cameraTransform.position,
            targetPosition,
            Time.deltaTime * 4f
        );
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

    public IEnumerator ShiftCamera(Vector3 targetDirection, float duration)
    {
        Quaternion startCameraRotation = cameraTransform.localRotation;
        Quaternion startPlayerRotation = transform.localRotation;

        Vector3 horizontalDir = new Vector3(targetDirection.x, 0f, targetDirection.z);
        Quaternion targetLookRotation = Quaternion.LookRotation(horizontalDir, Vector3.up);
        float targetHorizontalRotation = targetLookRotation.eulerAngles.y - 90f;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float newHorizontalRotation = Mathf.LerpAngle(
                startPlayerRotation.eulerAngles.y,
                targetHorizontalRotation,
                elapsedTime / duration
            );
            transform.localRotation = Quaternion.Euler(0f, newHorizontalRotation, 0f);

            float newVerticalRotation = Mathf.LerpAngle(
                startCameraRotation.eulerAngles.x,
                0f,
                elapsedTime / duration
            );
            cameraTransform.localRotation = Quaternion.Euler(newVerticalRotation, 0f, 0f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = Quaternion.Euler(0f, targetHorizontalRotation, 0f);
        cameraTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        horizontalRotation = transform.localEulerAngles.y;
        verticalRotation = cameraTransform.localEulerAngles.x;
        canRotateCamera = true;
    }

    public IEnumerator ShiftCameraLocalPosition(Vector3 targetLocalPosition, float duration)
    {
        Vector3 startLocalPosition = cameraTransform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            cameraTransform.localPosition = Vector3.Lerp(
                startLocalPosition,
                targetLocalPosition,
                elapsedTime / duration
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cameraTransform.localPosition = targetLocalPosition;

        horizontalRotation = transform.localEulerAngles.y;
        verticalRotation = cameraTransform.localEulerAngles.x;
        canRotateCamera = true;
    }

    /// <summary>
    /// Sets the camera's yaw and pitch rotation immediately, 
    /// respecting any optional horizontal/vertical limits.
    /// </summary>
    public void SetRotation(float newYaw, float newPitch)
    {
        // 1) Store the intended angles in your local fields
        horizontalRotation = newYaw;
        verticalRotation   = newPitch;

        // 2) If you want to clamp horizontal rotation:
        if (limitHorizontalRotation)
        {
            horizontalRotation = Mathf.Clamp(horizontalRotation, -horizontalLimit, horizontalLimit);
        }

        // 3) Clamp vertical rotation so it doesn't exceed the min/max
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        // 4) Apply these to the transforms
        // The "parent" object does horizontal, the cameraTransform does vertical
        transform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}
