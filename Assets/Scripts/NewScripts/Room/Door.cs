using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System;

public class Door : MonoBehaviour, IInteractable, IRoomObject
{
    private Room room;
    public Room Room { get { return room; } set { room = value; } }

    private BoxCollider doorCollider;
    private bool canInteract=true;
    public bool CanInteract{get{return canInteract;}}
    public GameObject LeftPart, RightPart;
    [HideInInspector]
    public GameObject DoorOpener;

    private bool isClosed = true;
    public bool IsClosed { get { return isClosed; } }

    private bool isFullyOpen = false;

    // The 0..1 measure of how open the door is (set from HFSM or sub-state)
    [Range(0f, 1f)]
    [SerializeField] private float targetOpenness = 0f;

    [SerializeField, Tooltip("Defines the angle at which the door is considered fully opened."), Range(80f, 179f)]
    private float maxDoorAngle = 120f;

    [SerializeField]
    private float doorOpenSpeed = 2f;

    // If the user wants to do partial increments, the HFSM might do that. 
    // But we keep the ability to clamp targetOpenness in [0..1].
    // doorOpenessPerInput can remain if we want to do partial increments 
    // in a quick script or debugging.
    [SerializeField, Range(0f, 1f)]
    private float doorOpenessPerInput = 0.1f;

    // Starting euler angles for each part, to restore when closed
    private Vector3 leftStartingLocalEulerAngles;
    private Vector3 rightStartingLocalEulerAngles;

    private bool compareX;
    private float relativePos;

    [Header("Door Events")]
    public Action OnDoorFullyOpened;
    public Action OnDoorFullyClosed;

    // For the angle interpolation
    public float CurrentDoorAngle
    {
        get
        {
            if (LeftPart != null)
            {
                float tempAngle = LeftPart.transform.localEulerAngles.y;
                // If angles exceed 180, we interpret them as negative
                return (tempAngle > 180f) ? tempAngle - 360f : tempAngle;
            }
            Debug.LogWarning("LeftPart is null. Door script can't read angle properly.");
            return 0f;
        }
    }

    // 0..1 measure of how open the door is, from angle
    public float CurrentOpenness
    {
        get
        {
            // fraction from 0..1
            return Mathf.Abs(CurrentDoorAngle) / maxDoorAngle;
        }
    }

    // We set or get a value in [0..1], which the script will attempt to 
    // realize in FixedUpdate by rotating the door parts.
    public float TargetOpenness
    {
        get { return targetOpenness; }
        set
        {
            targetOpenness = Mathf.Clamp01(value);
        }
    }

    private void Awake()
    {
        doorCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        // For now we assume if there's a single local player
        DoorOpener = GameManager.Instance.PlayerCharacter.gameObject;
        InitializeDoor();
    }

    private void FixedUpdate()
    {
        DoorUpdate();
    }

    /// <summary>
    /// The main update that rotates the door from currentAngle to 
    /// a target angle derived from TargetOpenness.
    /// </summary>
    private void DoorUpdate()
    {
        if (LeftPart != null && RightPart != null)
        {
            float currentAngle = CurrentDoorAngle;
            // We derive a final angle from TargetOpenness
            float targetAngle = CalculateTargetAngle();

            float tempOpenness = CurrentOpenness;
            if (isClosed && tempOpenness > 0f)
            {
                isClosed = false;
            }

            // We'll adjust door speed a bit by how far we are from target
            float opennessGap = Mathf.Abs(tempOpenness - targetOpenness);
            float dynamicSpeed = doorOpenSpeed * (1f + opennessGap);

            // If the difference in angles is more than a small threshold, keep rotating
            if (Mathf.Abs(Mathf.Abs(currentAngle) - Mathf.Abs(targetAngle)) > 0.2f)
            {
                float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.fixedDeltaTime * dynamicSpeed);

                // Apply symmetrical angles to left/right parts
                Vector3 leftEuler = LeftPart.transform.localEulerAngles;
                leftEuler.y = newAngle;
                LeftPart.transform.localEulerAngles = leftEuler;

                Vector3 rightEuler = RightPart.transform.localEulerAngles;
                rightEuler.y = -newAngle;
                RightPart.transform.localEulerAngles = rightEuler;
            }
            else
            {
                // If it's basically at the target, check fully open or fully closed
                if (targetOpenness >= 1f - 0.02f && !isFullyOpen)
                {
                    doorCollider.enabled = false; // door is effectively open
                    OnDoorFullyOpened?.Invoke();
                    Debug.LogWarning("Door Fully Opened");
                    isFullyOpen = true;
                }
                else if (!isClosed && tempOpenness <= 0.02f)
                {
                    isClosed = true;
                    isFullyOpen = false;
                    Debug.Log("Door Fully Closed");

                    // Reset angles to initial for a perfect "closed" alignment
                    LeftPart.transform.localEulerAngles = leftStartingLocalEulerAngles;
                    RightPart.transform.localEulerAngles = rightStartingLocalEulerAngles;

                    OnDoorFullyClosed?.Invoke();
                }
            }
        }
        else
        {
            Debug.LogWarning("LeftPart or RightPart is not assigned.");
        }
    }

    private float CalculateTargetAngle()
    {
        float signFactor;
        if (compareX)
            signFactor = Mathf.Sign(LeftPart.transform.position.z - RightPart.transform.position.z);
        else
            signFactor = Mathf.Sign(LeftPart.transform.position.x - RightPart.transform.position.x) * -1f;

        float angle = signFactor * relativePos * TargetOpenness * maxDoorAngle;
        
        return angle;
    }

    public void Interact(InteractInfo info)
    {
        Debug.Log("Door Interact called. The HFSM or sub-state logic will manage open/close.");

        SetRelativePosition();
        // No hooking to input here – the sub-state drives push/pull
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
            Debug.LogWarning("DoorOpener or LeftPart is not assigned.");
        }
    }

    private void InitializeDoor()
    {
        if (LeftPart != null && RightPart != null)
        {
            leftStartingLocalEulerAngles = LeftPart.transform.localEulerAngles;
            rightStartingLocalEulerAngles = RightPart.transform.localEulerAngles;

            Vector3 leftPos = LeftPart.transform.position;
            Vector3 rightPos = RightPart.transform.position;

            bool isXDifferent = !Mathf.Approximately(leftPos.x, rightPos.x);
            bool isZDifferent = !Mathf.Approximately(leftPos.z, rightPos.z);

            switch ((isXDifferent, isZDifferent))
            {
                case (true, false):
                    compareX = false;
                    break;
                case (false, true):
                    compareX = true;
                    break;
                case (true, true):
                    Debug.LogError("Error: Both x and z positions are different. The code might not handle diagonal doors well.");
                    break;
                case (false, false):
                    Debug.LogError("Error: Both x and z positions are the same, the door parts overlap?");
                    break;
            }
        }
        else
        {
            Debug.LogWarning("LeftPart or RightPart is not assigned in the inspector for Door script.");
        }
    }

    public void SetTargetOpenness(float newOpenness)
    {
        TargetOpenness = newOpenness; // clamp in [0..1]
        
    }

    // Called if room starts a combat scenario
    public void OnCombatStartedInRoom(Room room)
    {
        TargetOpenness = 0f;
        canInteract = false;
        doorCollider.enabled = true;
    }

    public void OnCombatEndedInRoom(Room room)
    {
        canInteract = true;
    }

    public bool IsFullyOpen { get { return isFullyOpen; } }
}
