using UnityEngine;
using UnityEngine.InputSystem;

public class FreeLookCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float sensitivity = 1f; // Mouse sensitivity
    [SerializeField] private float maxVerticalAngle = 80f; // Maximum upward/downward angle
    [SerializeField] private float minVerticalAngle = -80f; // Minimum downward/upward angle

    [Header("Control Settings")]
    [SerializeField] private Transform cameraTransform; // Assign the camera Transform explicitly if not on the same GameObject

    private Vector2 lookInput; // Stores the current input for looking
    private float verticalRotation = 0f; // Tracks the vertical rotation of the camera
    private float horizontalRotation = 0f; // Tracks the horizontal rotation of the player object

    private void Start()
    {
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
        RotateCamera();
    }

    private void RotateCamera()
    {
        // Apply sensitivity to the input
        Vector2 scaledInput = lookInput * sensitivity;

        // Horizontal rotation (rotate around Y-axis globally, on the parent object)
        horizontalRotation += scaledInput.x;
        transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);

        // Vertical rotation (rotate around X-axis locally, on the camera)
        verticalRotation -= scaledInput.y;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}
