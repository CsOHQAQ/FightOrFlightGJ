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

    [HideInInspector] public bool CanMove = true;
    private bool isMoving;
    private bool isRotating;

    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask interactableLayer;
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
        isRotating = false;
        interactDetect = GetComponent<InteractDetect>();
        hand = GetComponent<PlayerHandsComponent>();
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
                if (hand.lastState == HandState.Lowered)
                {
                    hand.StartCoroutine(hand.EaseToState(HandState.Raised));
                }
                else
                {
                    if (interactDetect.interactable is DoubleDoor)
                    {
                        DoubleDoor door = interactDetect.interactable as DoubleDoor;
                        Debug.Log(door.DoorOpeness);
                        hand.StartCoroutine(hand.EaseToState(HandState.Pushed, door.DoorOpeness));
                    }
                }

                if(interactDetect.ScrollValue<=0f)
                {
                    movementInput = tempInput;
                    return;
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


        if (!isMoving && !isRotating)
        {
            if (move == moveForward)
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
            else if (move == moveBackward)
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
            else if (move == moveLeft)
            {
                StartCoroutine(RotatePlayer(moveRotationLeft));
            }
            else if (move == moveRight)
            {
                StartCoroutine(RotatePlayer(moveRotationRight));
            }
        }    

    }

    private IEnumerator RotatePlayer(float rotation)
    {
        isRotating = true;

        float elapsedTime = 0;
        originalRotation = transform.rotation;
        targetRotation = originalRotation * Quaternion.Euler(0, rotation, 0);

        while (elapsedTime < timeToMove)
        {
            transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, (elapsedTime / timeToMove));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;

        isRotating = false;

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

    private bool IsObstacleInDirection(Vector3 direction)
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, detectionDistance, obstacleLayer))
        {
            /*
            if (hit.transform.TryGetComponent<OpenChest>(out OpenChest openChest))
            {
                openChest.Interact();
            }
            */

            return true;
        }

        return false;
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
        if (CanMove)
        {
            MovingOnGrid();
        }
    }

    public void Teleport(Vector3 position)
    {
        StopCoroutine("MovePlayer");
        isMoving = false;
        float y = transform.position.y;//A ugly fix to keep the player on the ground
        transform.position = position;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}
