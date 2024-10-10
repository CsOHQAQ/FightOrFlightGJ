using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DoubleDoor : InteractableObject
{
    public GameObject LeftPart, RightPart;
    public float OpenTime;
    public bool isOpen = false;
    public bool CanOpen = true;
    private BoxCollider boxCollider;

    RoomControler room;
    int openTimes;
    float openTimer;

    [SerializeField]
    float maxDoorAngle = 120;

    private float doorOpeness = 0f; // Range from 0 (closed) to 1 (fully open)
    private float rightDoorVelocity = 0f; // Velocity for smooth transition for the right door
    private float leftDoorVelocity = 0f;  // Velocity for smooth transition for the left door
    public float DoorOpeness { get { return Mathf.Abs(cumulativeLeftDoorAngle) / Mathf.Abs(maxDoorAngle); } }
    private float cumulativeRightDoorAngle = 0f; // Tracks the cumulative right door angle
    private float cumulativeLeftDoorAngle = 0f;  // Tracks the cumulative left door angle

    private bool wasClosed = true; // Track if the door was fully closed previously

    // Event that is triggered when the door is fully opened
    public event Action<string> OnDoorFullyOpened;
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }
    public override void Interact(object args = null)
    {
        if (args != null && args is InteractDetect)
        {
            InteractDetect interactDetect = (InteractDetect)args;
            doorOpeness = Mathf.Clamp(interactDetect.ScrollValue, 0f, 1f); // Clamps openness between 0 and 1
        }
    }

    public void Open()
    {
        // Increment openTimes when opening the door from fully closed
        if (wasClosed)
        {
            openTimes++;
            wasClosed = false; // Now the door is opening, it's no longer closed
        }
    }

    public void Close()
    {
        isOpen = false;
        wasClosed = true; // The door is now fully closed
    }

    public void Init(RoomControler iRoom)
    {
        room = iRoom;
        openTimer = 0f;
        openTimes = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (boxCollider.enabled == false)
        {
            // Prevent further movement once the door is fully open
            return;
        }

        maxDoorAngle = Mathf.Abs(maxDoorAngle) * (PlayerSide() ? 1 : -1);

        // Calculate the target angles based on door openness
        float targetRightAngle = Mathf.Lerp(0, maxDoorAngle, doorOpeness);  // Target angle for the right door
        float targetLeftAngle = Mathf.Lerp(0, -maxDoorAngle, doorOpeness);  // Target angle for the left door

        // Smoothly transition to the target angles
        float currentRightAngle = Mathf.SmoothDampAngle(RightPart.transform.localEulerAngles.y, targetRightAngle, ref rightDoorVelocity, OpenTime);
        float currentLeftAngle = Mathf.SmoothDampAngle(LeftPart.transform.localEulerAngles.y, targetLeftAngle, ref leftDoorVelocity, OpenTime);

        // Calculate the angle differences and accumulate them
        float rightAngleDelta = Mathf.DeltaAngle(RightPart.transform.localEulerAngles.y, currentRightAngle);
        float leftAngleDelta = Mathf.DeltaAngle(LeftPart.transform.localEulerAngles.y, currentLeftAngle);

        cumulativeRightDoorAngle += rightAngleDelta;
        cumulativeLeftDoorAngle += leftAngleDelta;

        // Apply the calculated angles to the door parts
        RightPart.transform.localEulerAngles = new Vector3(0, currentRightAngle, 0);
        LeftPart.transform.localEulerAngles = new Vector3(0, currentLeftAngle, 0);

        // Check if the door is fully opened using the cumulative angle tracking
        if (Mathf.Abs(cumulativeRightDoorAngle)>=Mathf.Abs(maxDoorAngle)*0.95f)
        {
            OnDoorFullyOpened?.Invoke(gameObject.name); // Announce which door has fully opened
            boxCollider.enabled = false; // Disable the collider to prevent closing once fully opened
          //  Debug.Log($"{gameObject.name} is fully opened.");
        }

        // Check if the door is starting to open from a closed state
        if (doorOpeness > 0f && wasClosed)
        {
            Open(); // Call the Open method to increment openTimes
        }

        // Set isOpen to true whenever doorOpeness is greater than 0
        if (doorOpeness > 0f)
        {
            isOpen = true;
        }

        // If door is fully closed, reset the wasClosed flag
        if (Mathf.Approximately(doorOpeness, 0f))
        {
            wasClosed = true;
            isOpen = false; // Ensure isOpen is set to false when the door is fully closed
            cumulativeRightDoorAngle = 0f; // Reset the cumulative angle
            cumulativeLeftDoorAngle = 0f;  // Reset the cumulative angle
        }
    }

    public void EnterBattle()
    {
        Debug.Log("Enter Battle");
        CanOpen = false;
    }

    bool PlayerSide()
    {
        Vector2 doorDirect = new Vector2(transform.Find("OpenPosition").localPosition.x, transform.Find("OpenPosition").localPosition.z);
        Vector2 playerDirect = new Vector2(transform.position.x- GameControl.Game.Player.transform.position.x,  transform.position.z- GameControl.Game.Player.transform.position.z);
       // Debug.Log($"player's angle to door {Vector2.Angle(doorDirect, playerDirect)}");
        return Vector2.Angle(doorDirect, playerDirect) < 90;
        
    }

}
