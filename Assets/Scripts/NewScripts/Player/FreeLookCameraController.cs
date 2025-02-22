using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;

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

    // -- NEW: For smoothing the camera offset while in DoorInteract
    [Header("Door Offset Settings")]
    [SerializeField] private float doorOffsetMaxDistance = 0.5f; // how many meters forward
    [SerializeField] private float doorOffsetSmoothTime  = 0.1f; // how quickly to ease
    private float targetDoorOffset = 0f;   // sub-state sets in [0..1]
    private float currentDoorOffset = 0f;  // what we have now
    private float doorOffsetVelocity = 0f; // for SmoothDamp

    [Header("Events")]
    [SerializeField] private UnityEvent onLookedToBottom;
    [SerializeField] private UnityEvent onExitLookedToBottom;

    private Vector2 lookInput;
    public Vector2 LookInput { get { return lookInput; } set{ lookInput = value; } }
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;

    private PlayerCharacter player;
    private bool hasLookedToBottom = false;
    private bool canRotateCamera = true;

    // We'll store the initial local position so we can offset from it
    private Vector3 baseLocalPosition;

    private void Awake()
    {
        // We store the camera's initial local pos to come back to
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        baseLocalPosition = cameraTransform.localPosition;
    }

    private void Start()
    {
        player = gameObject.transform.parent.GetComponent<PlayerCharacter>();
        if (player == null)
        {
            Debug.LogError("NO PLAYERCHARACTER FOUND on CameraController");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // A property so sub-states can set the desired offset (0..1).
    public float DoorOffset
    {
        get => targetDoorOffset;
        set => targetDoorOffset = Mathf.Clamp01(value);
    }

    private void Update()
    {
        if (!canRotateCamera) return;

        // Query the HFSM's current state
        var current = player.BaseStateMachine.CurrentState;

        // If in MovementParentState => standard free look
        if (current is MovementParentState)
        {
            RotateCamera();
        }
        else if (current is InteractState)
        {
            // e.g. door interaction logic
            //RotateCameraWithCameraRotation();

            //if (player.CurrentDoor != null)
            //{
            //    AdjustCameraPositionBasedOnDoor();
            //}
        }
    }

    // We'll do the smoothing for door offset in FixedUpdate
    private void FixedUpdate()
    {
        if (!canRotateCamera) return;

        // Smoothly approach the desired door offset
        float desiredOffset = targetDoorOffset * doorOffsetMaxDistance;
        currentDoorOffset = Mathf.SmoothDamp(currentDoorOffset, desiredOffset, ref doorOffsetVelocity, doorOffsetSmoothTime);

        // Apply offset in local space, relative to the baseLocalPosition
        cameraTransform.position = player.transform.position + player.GetCameraYawForward() * currentDoorOffset;
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
        if (player.CurrentDoor != null && player.CurrentDoor.CurrentOpenness <= 0.5f)
        {
            Vector3 directionToDoor = (player.CurrentDoor.transform.position - player.transform.position).normalized;
            StartCoroutine(ShiftCamera(directionToDoor, centerCameraDuration));
            return;
        }

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

        // This was your existing approach for adjusting camera
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
        horizontalRotation = newYaw;
        verticalRotation   = newPitch;

        if (limitHorizontalRotation)
        {
            horizontalRotation = Mathf.Clamp(horizontalRotation, -horizontalLimit, horizontalLimit);
        }

        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        transform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}
