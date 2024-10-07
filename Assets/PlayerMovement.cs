using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 moveForward = Vector3.zero;
    private Vector3 moveBackward = Vector3.zero;
    private Vector3 moveLeft = Vector3.zero;
    private Vector3 moveRight = Vector3.zero;

    private float moveForwardDistance;
    private float moveBackwardDistance;

    private float moveRotationLeft;
    private float moveRotationRight;

    private InputControls controls;
    private Vector2 movementInput;

    private Quaternion originalRotation, targetRotation;
    private Vector3 originalPosition, targetPosition;
    [SerializeField] private float timeToMove = 0.2f;

    private bool isMoving;
    private bool isRotating;


    private void Awake()
    {
        moveForward = new Vector3(0, 0, 1);
        moveBackward = new Vector3(0, 0, -1);
        moveLeft = new Vector3(-1, 0, 0);
        moveRight = new Vector3(1, 0, 0);

        moveForwardDistance = 1.0f;
        moveBackwardDistance = -1.0f;
        moveRotationLeft = -90;
        moveRotationRight = 90;

        controls = new InputControls();
        movementInput = Vector2.zero;

        originalPosition = transform.position;
        targetPosition = Vector3.zero;

        isMoving = false;
    }

    private void OnEnable()
    {
        controls.Enable();

        controls.Player.Movement.performed += OnMove;
        controls.Player.Movement.canceled += OnMove;
    }

    private void OnDisable()
    {
        controls.Player.Movement.performed -= OnMove;
        controls.Player.Movement.canceled -= OnMove;
        controls.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    private void MovingOnGrid()
    {
        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);


        if (move == moveForward && !isMoving)
        {
            StartCoroutine(MovePlayer(moveForwardDistance));
        }

        if (move == moveBackward && !isMoving)
        {
            StartCoroutine(MovePlayer(moveBackwardDistance));
        }

        if (move == moveLeft && !isMoving)
        {
            StartCoroutine(RotatePlayer(moveRotationLeft));
        }

        if (move == moveRight && !isMoving)
        {
            StartCoroutine(RotatePlayer(moveRotationRight));
        }

    }

    private IEnumerator RotatePlayer(float rotation)
    {
        isMoving = true;

        float elapsedTime = 0;
        originalRotation = transform.rotation;
        targetRotation = originalRotation * Quaternion.Euler(0, rotation, 0);

        while (elapsedTime < timeToMove)
        {
            transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, (elapsedTime / timeToMove));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        while (isMoving)
        {
            Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);

            if (move == Vector3.zero)
            {
                isMoving = false;
            }

            yield return null;
        }

        transform.rotation = targetRotation;

    }

    private IEnumerator MovePlayer(float direction)
    {
        isMoving = true;

        float elapsedTime = 0;

        originalPosition = transform.position;
        targetPosition = originalPosition + transform.forward * direction;

        while (elapsedTime < timeToMove)
        {
            transform.position = Vector3.Lerp(originalPosition, targetPosition, (elapsedTime / timeToMove));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        isMoving = false;
    }


    void Update()
    {
        MovingOnGrid();
    }
}
