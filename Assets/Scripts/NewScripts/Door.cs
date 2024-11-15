using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

public class Door : MonoBehaviour, IInteractable
{
    private PlayerInput playerInput;
    PlayerCharacter character;
    private BoxCollider doorCollider;

    public GameObject LeftPart, RightPart;
    public GameObject DoorOpener;
    bool isClosed = true;
    public bool IsClosed{get{return isClosed;}}
    private bool compareX;

    private float targetOpenness;
    
    [SerializeField, Tooltip("Defines the amount by which the door opens per input."), Range(0f, 1f)]
    private float doorOpenessPerInput;

    [SerializeField, Tooltip("Defines the angle at which the door is considered to be fully opened."), Range(80f, 179f)]
    private float MaxDoorAngle = 120;

    public float TargetDoorAngle => (compareX ? Mathf.Sign(LeftPart.transform.position.z - RightPart.transform.position.z) : Mathf.Sign(LeftPart.transform.position.x - RightPart.transform.position.x) * -1f) * relativePos * targetOpenness * MaxDoorAngle;
    
    [SerializeField]
    private float doorOpenSpeed = 2f;



    public float CurrentDoorAngle
    {
        get
        {
            if (LeftPart != null)
            {
                float tempAngle = LeftPart.transform.localEulerAngles.y;
                return tempAngle > 180f ? tempAngle - 360f : tempAngle;
            }
            Debug.LogWarning("LeftPart GameObject is not assigned.");
            return 0f;
        }
    }

    public float CurrentOpenness => Mathf.Abs(this.CurrentDoorAngle) / MaxDoorAngle;

    private float relativePos;

    [Header("Door Events")]
    public Action OnDoorFullyOpened;
    public Action OnDoorFullyClosed;

    private void Awake()
    {
        doorCollider = GetComponent<BoxCollider>();
    }

    void Start()
    {
        InitializeDoor();
        SetRelativePosition();
    }

    private void SetRelativePosition()
    {
        if (DoorOpener != null && LeftPart != null)
        {
            relativePos = compareX
                ? (DoorOpener.transform.position.x > LeftPart.transform.position.x ? 1f : -1f)
                : (DoorOpener.transform.position.z > LeftPart.transform.position.z ? 1f : -1f);
        }
        else
        {
            Debug.LogWarning("DoorOpener or LeftPart GameObject is not assigned.");
        }
    }

    void FixedUpdate()
    {
        DoorUpdate();
    }

    public void DoorUpdate()
    {
        if (LeftPart != null && RightPart != null)
        {
            float currentAngle = this.CurrentDoorAngle;
            float targetAngle = this.TargetDoorAngle;
            float tempOpenness = this.CurrentOpenness;
            if (isClosed&&tempOpenness>0f)
            {
                isClosed = false;
            }

            if (Mathf.Abs(Mathf.Abs(currentAngle)-Mathf.Abs(targetAngle))>0.2f)
            {
                float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.fixedDeltaTime * doorOpenSpeed);
                Vector3 leftLocalEulerAngles = LeftPart.transform.localEulerAngles;
                leftLocalEulerAngles.y = newAngle;
                LeftPart.transform.localEulerAngles = leftLocalEulerAngles;

                Vector3 rightLocalEulerAngles = RightPart.transform.localEulerAngles;
                rightLocalEulerAngles.y = -newAngle;
                RightPart.transform.localEulerAngles = rightLocalEulerAngles;

                //Debug.Log("current angle approaching target angle");
            }
            else
            {
                //Debug.Log("current angle reach target angle");
                //Debug.Log(tempOpenness);
                // Trigger events if door is fully opened or fully closed
                //if (tempOpenness>= 1f- 0.02f)
                if (targetOpenness>= 1f- 0.02f)
                {
                    playerInput.actions["Movement"].performed -= OnMovementPerformed;
                    doorCollider.enabled = false;
                    OnDoorFullyOpened?.Invoke();
                    OnDoorFullyOpened -= character.OnDoorFullyOpened;
                    OnDoorFullyClosed -= character.OnDoorFullyClosed;
                }
                else if (!isClosed && tempOpenness<=0.02f)
                {
                    isClosed = true;

                    Debug.Log("Door Fully Closed");
                }
            }
        }
        else
        {
            Debug.LogWarning("LeftPart or RightPart GameObject is not assigned.");
        }
    }

    public void Interact(object args = null)
    {
        GameObject playerObject = args as GameObject;
        playerInput = playerObject.GetComponentInChildren<PlayerInput>();
        playerInput.actions["Movement"].performed += OnMovementPerformed;
        character = playerObject.GetComponentInChildren<PlayerCharacter>();
        character.OnInteractDoor(this);
        OnDoorFullyOpened += character.OnDoorFullyOpened;
        OnDoorFullyClosed += character.OnDoorFullyClosed;
    }

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        if (moveInput.y != 0f)
        {
            targetOpenness += moveInput.y * doorOpenessPerInput;
            targetOpenness = Mathf.Clamp(targetOpenness, 0f, 1f);

            // Unsubscribe from movement if door is fully closed and input is closing
            if (Mathf.Approximately(targetOpenness,0f) && moveInput.y < 0)
            {
                playerInput.actions["Movement"].performed -= OnMovementPerformed;
                OnDoorFullyClosed?.Invoke();
                OnDoorFullyOpened -= character.OnDoorFullyOpened;
                OnDoorFullyClosed -= character.OnDoorFullyClosed;
            }

            //Debug.Log($"Door Input: {moveInput}, Target Openess: {targetOpenness}, Target Door Angle: {TargetDoorAngle}");
        }
    }

    private void InitializeDoor()
    {
        if (LeftPart != null && RightPart != null)
        {
            Vector3 leftPartPosition = LeftPart.transform.position;
            Vector3 rightPartPosition = RightPart.transform.position;

            bool isXDifferent = !Mathf.Approximately(leftPartPosition.x, rightPartPosition.x);
            bool isZDifferent = !Mathf.Approximately(leftPartPosition.z, rightPartPosition.z);

            switch ((isXDifferent, isZDifferent))
            {
                case (true, false):
                    compareX = false;
                    break;
                case (false, true):
                    compareX = true;
                    break;
                case (true, true):
                    Debug.LogError("Error: Both x and z positions are different.");
                    break;
                case (false, false):
                    Debug.LogError("Error: Both x and z positions are the same.");
                    break;
            }
        }
        else
        {
            Debug.LogWarning("LeftPart or RightPart GameObject is not assigned in the inspector.");
        }
    }
}
