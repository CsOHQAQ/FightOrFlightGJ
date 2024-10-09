using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleDoor : InteractableObject
{
    public GameObject LeftPart, RightPart;
    public float OpenTime;
    public bool isOpen;
    public bool CanOpen = true;

    RoomControler room;
    int openTimes;
    float openTimer;

    [SerializeField]
    float maxDoorAngle = 120;

    private float doorOpeness = 0f; // Range from 0 (closed) to 1 (fully open)
    private float rightDoorVelocity = 0f; // Velocity for smooth transition for the right door
    private float leftDoorVelocity = 0f;  // Velocity for smooth transition for the left door

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
        isOpen = true;
        openTimes++;
    }

    public void Close()
    {
        isOpen = false;
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
        // Calculate the target angles based on door openness
        float targetRightAngle = Mathf.Lerp(0, maxDoorAngle, doorOpeness);  // Target angle for the right door
        float targetLeftAngle = Mathf.Lerp(0, -maxDoorAngle, doorOpeness);  // Target angle for the left door

        // Smoothly transition to the target angles
        float currentRightAngle = Mathf.SmoothDampAngle(RightPart.transform.localEulerAngles.y, targetRightAngle, ref rightDoorVelocity, OpenTime);
        float currentLeftAngle = Mathf.SmoothDampAngle(LeftPart.transform.localEulerAngles.y, targetLeftAngle, ref leftDoorVelocity, OpenTime);

        // Apply the calculated angles to the door parts
        RightPart.transform.localEulerAngles = new Vector3(0, currentRightAngle, 0);
        LeftPart.transform.localEulerAngles = new Vector3(0, currentLeftAngle, 0);

        // Additional logic for handling the door and room states
        if (isOpen)
        {
            if (!room.isClear)
                openTimer += Time.deltaTime;
            else
                openTimer = 0f;

            if (openTimer > OpenTime)
            {
                EnterBattle();
            }
        }
        else
        {
            openTimer = 0f;
        }
    }

    public void EnterBattle()
    {
        Debug.Log("Enter Battle");
        CanOpen = false;
    }
}
