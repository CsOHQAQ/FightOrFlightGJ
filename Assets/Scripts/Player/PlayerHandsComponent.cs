using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerHandsComponent : SerializedMonoBehaviour
{
    // Enum to represent hand states
    public enum HandState
    {
        Lowered,
        Raised,
        Pushed
    }

    // Struct to hold position, rotation, and scale
    [System.Serializable]
    public struct HandTransformData
    {
        public Vector3 position;
        public Vector3 rotation; // Rotation in Euler angles
        public Vector3 scale;
    }

    // GameObject references for the left and right hands
    [SerializeField]
    private GameObject leftHand;

    [SerializeField]
    private GameObject rightHand;

    // A dictionary mapping hand states to their corresponding hand transform data (position, rotation, scale)
    [SerializeField, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<HandState, HandTransformData> handStateTransforms = new Dictionary<HandState, HandTransformData>();

    // Speed for easing into the hand position/rotation/scale
    [SerializeField]
    private float transitionDuration = 1f;

    private float leftHandRotationYVelocity = 0f;  // For smoothing left hand rotation
    private float leftHandRotationZVelocity = 0f;  // For smoothing left hand rotation
    private float rightHandRotationYVelocity = 0f; // For smoothing right hand rotation
    private float rightHandRotationZVelocity = 0f; // For smoothing right hand rotation

    public HandState lastState;

    void Start()
    {
        lastState = HandState.Lowered; // Default starting state
    }

    // Coroutine to ease both hands to the transform recorded in the given state, with optional interpolation percentage
    public IEnumerator EaseToState(HandState targetState, float percentage = 1f)
    {
        if (handStateTransforms.ContainsKey(targetState) && handStateTransforms.ContainsKey(lastState))
        {
            // Clamp the percentage between 0 and 1
            percentage = Mathf.Clamp01(percentage);

            // Get the target and last state data for left hand
            HandTransformData targetTransform = handStateTransforms[targetState];
            if (targetState == HandState.Pushed)
            {
                lastState = HandState.Raised;
            }

            HandTransformData lastTransform = handStateTransforms[lastState];

            // Calculate the actual target position, rotation, and scale based on percentage
            Vector3 actualTargetPosition = lastTransform.position + (targetTransform.position - lastTransform.position) * percentage;
            Vector3 actualTargetScale = lastTransform.scale + (targetTransform.scale - lastTransform.scale) * percentage;

            // Interpolate the rotation using the percentage
            Vector3 actualTargetRotation = lastTransform.rotation + (targetTransform.rotation - lastTransform.rotation) * percentage;
            //Debug.Log(actualTargetRotation);
            // Mirror the x position for the right hand
            Vector3 actualRightHandPosition = actualTargetPosition;
            actualRightHandPosition.x *= -1; // Mirror x position for the right hand

            // Initial setup for smooth damp rotation
            Vector3 initialLeftRotation = leftHand.transform.localEulerAngles;
            Vector3 initialRightRotation = rightHand.transform.localEulerAngles;

            float elapsedTime = 0f;

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;

                // Ease the position and scale
                leftHand.transform.localPosition = Vector3.Lerp(leftHand.transform.localPosition, actualTargetPosition, Mathf.SmoothStep(0f, 1f, t));
                leftHand.transform.localScale = Vector3.Lerp(leftHand.transform.localScale, actualTargetScale, Mathf.SmoothStep(0f, 1f, t));

                rightHand.transform.localPosition = Vector3.Lerp(rightHand.transform.localPosition, actualRightHandPosition, Mathf.SmoothStep(0f, 1f, t));
                rightHand.transform.localScale = Vector3.Lerp(rightHand.transform.localScale, actualTargetScale, Mathf.SmoothStep(0f, 1f, t));

                // Smoothly transition the y and z rotations for both hands using Mathf.SmoothDampAngle
                float currentLeftRotationY = Mathf.SmoothDampAngle(leftHand.transform.localEulerAngles.y, actualTargetRotation.y, ref leftHandRotationYVelocity, transitionDuration);
                float currentLeftRotationZ = Mathf.SmoothDampAngle(leftHand.transform.localEulerAngles.z, actualTargetRotation.z, ref leftHandRotationZVelocity, transitionDuration);

                float currentRightRotationY = Mathf.SmoothDampAngle(rightHand.transform.localEulerAngles.y, -actualTargetRotation.y, ref rightHandRotationYVelocity, transitionDuration);
                float currentRightRotationZ = Mathf.SmoothDampAngle(rightHand.transform.localEulerAngles.z, actualTargetRotation.z, ref rightHandRotationZVelocity, transitionDuration);

                // Apply the calculated rotations
                leftHand.transform.localEulerAngles = new Vector3(leftHand.transform.localEulerAngles.x, currentLeftRotationY, currentLeftRotationZ);
                rightHand.transform.localEulerAngles = new Vector3(rightHand.transform.localEulerAngles.x, currentRightRotationY, currentRightRotationZ);

                yield return null;
            }

            // Ensure final values are set for both hands
            leftHand.transform.localPosition = actualTargetPosition;
            leftHand.transform.localScale = actualTargetScale;
            leftHand.transform.localEulerAngles = new Vector3(leftHand.transform.localEulerAngles.x, actualTargetRotation.y, actualTargetRotation.z);

            rightHand.transform.localPosition = actualRightHandPosition;
            rightHand.transform.localScale = actualTargetScale;
            rightHand.transform.localEulerAngles = new Vector3(rightHand.transform.localEulerAngles.x, -actualTargetRotation.y, actualTargetRotation.z);

            lastState = targetState; // Update the last state to the new state
        }
        else
        {
            Debug.LogWarning($"No transform data assigned for {targetState} or {lastState} state.");
        }
    }

}
