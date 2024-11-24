using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float sensitivity = 1f; // Mouse sensitivity
    [SerializeField] private float maxVerticalAngle = 80f; // Max up/down angle
    [SerializeField] private float maxHorizontalAngle = 80f; // Max left/right angle

    private Vector2 lookInput; // Stores the current input for looking
    private float verticalRotation = 0f; // Tracks the vertical rotation of the camera
    private float horizontalRotation = 0f; // Tracks the horizontal rotation of the camera

    private void Start()
    {
        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        // Horizontal rotation (local rotation on Y-axis)
        horizontalRotation += scaledInput.x;
        horizontalRotation = Mathf.Clamp(horizontalRotation, -maxHorizontalAngle, maxHorizontalAngle);

        // Vertical rotation (local rotation on X-axis)
        verticalRotation -= scaledInput.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);

        // Apply the horizontal and vertical rotations as local rotations
        Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }
}
