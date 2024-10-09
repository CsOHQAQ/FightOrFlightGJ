using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerHandsComponent;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 moveForward = Vector3.zero;
    private Vector3 moveBackward = Vector3.zero;
    private Vector3 moveLeft = Vector3.zero;
    private Vector3 moveRight = Vector3.zero;

    [SerializeField] private float gridMovement = 1.0f;
    [SerializeField] private float gridSize = 1.0f;
    private float moveRotationLeft;
    private float moveRotationRight;

    private InputControls controls;
    private Vector2 movementInput;

    private Quaternion originalRotation, targetRotation;
    private Vector3 originalPosition, targetPosition;
    [SerializeField] private float timeToMove = 0.2f;

    private bool isMoving;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float detectionDistance = 1.1f;

    InteractDetect interactDetect;
    PlayerHandsComponent hand;

    private void Awake()
    {
        moveForward = new Vector3(0, 0, 1);
        moveBackward = new Vector3(0, 0, -1);
        moveLeft = new Vector3(-1, 0, 0);
        moveRight = new Vector3(1, 0, 0);

        moveRotationLeft = -90;
        moveRotationRight = 90;

        controls = new InputControls();
        movementInput = Vector2.zero;

        originalPosition = transform.position;
        targetPosition = Vector3.zero;

        isMoving = false;
        interactDetect = GetComponent<InteractDetect>();
        hand = GetComponentInChildren<PlayerHandsComponent>();
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
        Vector2 tempInput = context.ReadValue<Vector2>();
        
        if (tempInput.y != 0)
        {
            if (!interactDetect.TryInteract(context))
            {
                movementInput = tempInput;
                hand.StartCoroutine(hand.EaseToState(HandState.Lowered));
            }
            else
            {
                if(interactDetect.ScrollValue<=0f)
                {
                    hand.StartCoroutine(hand.EaseToState(HandState.Lowered));
                    movementInput = tempInput;
                    return;
                }
                if (hand.lastState != HandState.Raised)
                {
                    hand.StartCoroutine(hand.EaseToState(HandState.Raised));
                }
                else
                {
                    hand.StartCoroutine(hand.EaseToState(HandState.Pushed, interactDetect.ScrollValue));
                }
                
            }
        }
        else
        {
            movementInput = tempInput;
        }

        
    }

    private void MovingOnGrid()
    {
        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y).normalized;


        if (move == moveForward && !isMoving)
        {
            if (IsObstacleInDirection(transform.forward))
            {
                StartCoroutine(CollisionEffect(transform.forward));
            }
            else
            {
                StartCoroutine(MovePlayer(gridMovement));
            }
        }

        if (move == moveBackward && !isMoving)
        {
            if (IsObstacleInDirection(-transform.forward))
            {
                StartCoroutine(CollisionEffect(-transform.forward));
            }
            else
            {
                StartCoroutine(MovePlayer(-gridMovement));
            }
            
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
        // SnapToGrid();
        isMoving = false;
    }

    private bool IsObstacleInDirection(Vector3 direction)
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        return Physics.Raycast(rayOrigin, direction, out RaycastHit hit, detectionDistance, obstacleLayer);
    }

    private IEnumerator CollisionEffect(Vector3 direction)
    {
        isMoving = true;

        float elapsedTime = 0;
        originalPosition = transform.position;
        targetPosition = originalPosition + direction  * 0.3f;

        while (elapsedTime < timeToMove / 2)
        {
            transform.position = Vector3.Lerp(originalPosition, targetPosition, (elapsedTime / (timeToMove / 2)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0;
        Vector3 bounceBackPosition = originalPosition;

        while (elapsedTime < timeToMove / 2)
        {
            transform.position = Vector3.Lerp(targetPosition, bounceBackPosition, (elapsedTime / (timeToMove / 2)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = bounceBackPosition;
        SnapToGrid();
        isMoving = false;
    }

    private void SnapToGrid()
    {
        float snappedX = Mathf.Round(transform.position.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(transform.position.z / gridSize) * gridSize;

        transform.position = new Vector3(snappedX, transform.position.y, snappedZ);
    }


    void Update()
    {
        MovingOnGrid();
    }

    public void Teleport(Vector3 position)
    {
        StopCoroutine("MovePlayer");
        isMoving = false;
        transform.position=position;
    }
}
