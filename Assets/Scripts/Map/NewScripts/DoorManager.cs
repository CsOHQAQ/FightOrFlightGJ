using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.InputSystem;

public class DoorManager : MonoBehaviour
{
    public GameObject LeftPart, RightPart;
    public GameObject DoorOpener;

    private bool compareX;
    private float currentOpeness;
    private float targetOpeness;
    [SerializeField, Tooltip("Defines the amount by which the door opens per input."), Range(0f, 1f)]
    private float doorOpenessPerInput;
    private float currentDoorAngle;
    [SerializeField, Tooltip("Defines the angle at which the door is considered to be fully opened."), Range(80f, 179f)]
    private float MaxDoorAngle = 120;
    public float TargetDoorAngle{ get{return targetOpeness * MaxDoorAngle;}}
    public float CurrentDoorAngle { get{
        if (LeftPart != null)
        {
            float tempAngle = LeftPart.transform.localEulerAngles.y;
            if(tempAngle>180f)
            {
                return tempAngle - 360f;
            }
            return tempAngle;
        }
        else
        {
            Debug.LogWarning("LeftPart GameObject is not assigned.");
            return 0f; // Return a default angle if LeftPart is null
        }
    }}
        
    private float relativePos;

void Start()
{
    InitializeDoor();
    Debug.Log(this.CurrentDoorAngle);

    SetRelativePosition();
}

private void SetRelativePosition()
{
    if (DoorOpener != null && LeftPart != null)
    {
        if (compareX)
        {
            // Compare the x positions
            relativePos = DoorOpener.transform.position.x > LeftPart.transform.position.x ? 1f : -1f;
        }
        else
        {
            // Compare the z positions
            relativePos = DoorOpener.transform.position.z > LeftPart.transform.position.z ? 1f : -1f;
        }

        Debug.Log($"Relative Position set based on {(compareX ? "X" : "Z")} axis: {relativePos}");
    }
    else
    {
        Debug.LogWarning("DoorOpener or LeftPart GameObject is not assigned.");
    }
}

    // Update is called once per frame
    void FixedUpdate()
    {
        if (LeftPart != null)
        {
            // Check if current angle is different from the target angle
            float currentAngle = CurrentDoorAngle;
            float targetAngle = TargetDoorAngle;

            if (!Mathf.Approximately(currentAngle, targetAngle))
            {
                // Smoothly interpolate the current angle towards the target angle
                float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.fixedDeltaTime * 2f); // 2f can be adjusted for speed

                // Set the local Y rotation of LeftPart to the new interpolated angle
                Vector3 localEulerAngles = LeftPart.transform.localEulerAngles;
                localEulerAngles.y = newAngle;
                LeftPart.transform.localEulerAngles = localEulerAngles;
            }
        }
        else
        {
            Debug.LogWarning("LeftPart GameObject is not assigned.");
        }
    }

    public void OnMovement(InputValue value)
    {
        float doorInput = value.Get<Vector2>().y;

        if (doorInput != 0)
        {
            // Add the calculated value to targetOpeness and clamp it between 0 and 1
            targetOpeness += doorInput * doorOpenessPerInput;
            targetOpeness = Mathf.Clamp(targetOpeness, 0f, 1f);

            Debug.Log($"Door Input: {doorInput}, Target Openess: {targetOpeness}, Target Door Angle: {this.TargetDoorAngle}");
        }
    }

    public float GetCurrentDoorAngle()
    {
        if (LeftPart != null)
        {
            float tempAngle = LeftPart.transform.localEulerAngles.y;
            if(tempAngle>180f)
            {
                return tempAngle - 360f;
            }
            return tempAngle;
        }
        else
        {
            Debug.LogWarning("LeftPart GameObject is not assigned.");
            return 0f; // Return a default angle if LeftPart is null
        }
    }
    
    

    private void InitializeDoor()
    {
        if (LeftPart != null && RightPart != null)
    {
        Vector3 leftPartPosition = LeftPart.transform.position;
        Vector3 rightPartPosition = RightPart.transform.position;

        //Debug.Log($"Left Part Position: {leftPartPosition}");
        //Debug.Log($"Right Part Position: {rightPartPosition}");

        bool compareX;

        // Check differences in x and z values
        bool isXDifferent = !Mathf.Approximately(leftPartPosition.x, rightPartPosition.x);
        bool isZDifferent = !Mathf.Approximately(leftPartPosition.z, rightPartPosition.z);

        // Switch based on conditions
        switch ((isXDifferent, isZDifferent))
        {
            case (true, false):
                compareX = false;
                Debug.Log($"Only x is different: compareX = {compareX}");
                break;
            case (false, true):
                compareX = true;
                Debug.Log($"Only z is different: compareX = {compareX}");
                break;
            case (true, true):
                Debug.LogError("Error: Both x and z positions are different.");
                break;
            case (false, false):
                Debug.LogError("Error: Both x and z positions are the same.");
                break;
            default:
                Debug.LogWarning("Unexpected condition.");
                break;
        }
    }
    else
    {
        Debug.LogWarning("LeftPart or RightPart GameObject is not assigned in the inspector.");
    }
    }
}
