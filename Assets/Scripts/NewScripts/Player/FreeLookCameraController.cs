using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class FreeLookCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float sensitivity = 1f; // Mouse sensitivity
    [SerializeField] private float maxVerticalAngle = 80f; // Maximum upward/downward angle
    [SerializeField] private float minVerticalAngle = -80f; // Minimum downward/upward angle

    [Header("Control Settings")]
    [SerializeField] private Transform cameraTransform; // Assign the camera Transform explicitly if not on the same GameObject

    [Header("Events")]
    [SerializeField] private UnityEvent onLookedToBottom; // Event to notify when the camera looks downward beyond the threshold
    [SerializeField] private UnityEvent onExitLookedToBottom; // Event to notify when the camera exits looking downward

    private Vector2 lookInput; // Stores the current input for looking
    private float verticalRotation = 0f; // Tracks the vertical rotation of the camera
    private float horizontalRotation = 0f; // Tracks the horizontal rotation of the player object

    private PlayerCharacter player;
    private bool hasLookedToBottom = false; // To prevent multiple triggers for the same downward movement

    private void Start()
    {
        player = gameObject.transform.parent.GetComponent<PlayerCharacter>();
        if (player == null)
        {
            Debug.LogError("NO PLAYERCHARACTER FOUND on CameraController");
        }
        player.OnStateEnter += HandleStateEnter;

        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // If no cameraTransform is assigned, use the camera on this GameObject
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    // This method will be called by the Input System when "Look" is performed
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        // Process the camera movement
        if (player.CurrentState != PlayerState.DoorOpeningState)
        {
            RotateCamera();
        }
    }

    private void RotateCamera()
    {
        // Apply sensitivity to the input
        Vector2 scaledInput = lookInput * sensitivity;

        // Horizontal rotation (rotate around Y-axis globally, on the parent object)
        horizontalRotation += scaledInput.x;
        transform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);

        // Vertical rotation (rotate around X-axis locally, on the camera)
        verticalRotation -= scaledInput.y;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // Check if the camera is looking down and invoke the event if needed
        CheckLookedToBottom();
    }

    private void CheckLookedToBottom()
    {
        float downwardThreshold = 70f; // Positive value for downward threshold

        // Check if the camera is now looking downward beyond the threshold
        if (verticalRotation >= downwardThreshold && !hasLookedToBottom)
        {
            hasLookedToBottom = true;
            Debug.Log("LOOKING AT BOTTOM");
            onLookedToBottom?.Invoke(); // Invoke the event when the camera looks down beyond the threshold
        }
        // Check if the camera moved up beyond the threshold, exiting the downward look
        else if (verticalRotation < downwardThreshold && hasLookedToBottom)
        {
            hasLookedToBottom = false;
            Debug.Log("EXIT LOOKING AT BOTTOM");
            onExitLookedToBottom?.Invoke(); // Invoke the event when the camera exits looking down beyond the threshold
        }
    }

    public void HandleStateEnter(PlayerState state)
    {
        if (state == PlayerState.DoorOpeningState)
        {
            StartCoroutine(ShiftCamera(player.GetClosestDirection(transform.forward), 0.3f));
        }
    }

    public IEnumerator ShiftCamera(Vector3 targetDirection, float duration)
    {
        // Cache the initial rotation of the camera and the player
        Quaternion startCameraRotation = cameraTransform.localRotation;
        Quaternion startPlayerRotation = transform.localRotation;

        // Calculate the target horizontal rotation based on the target direction
        float targetHorizontalRotation = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Interpolate the horizontal rotation for the player object (Y-axis)
            float horizontalRotation = Mathf.LerpAngle(startPlayerRotation.eulerAngles.y, targetHorizontalRotation, elapsedTime / duration);
            transform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);

            // Interpolate the vertical rotation for the camera (X-axis)
            float verticalRotation = Mathf.LerpAngle(startCameraRotation.eulerAngles.x, 0f, elapsedTime / duration); // Keeping it upright
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final rotation is precisely set
        transform.localRotation = Quaternion.Euler(0f, targetHorizontalRotation, 0f);
        cameraTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void OnDestroy()
    {
        player.OnStateEnter -= HandleStateEnter;
    }
}
