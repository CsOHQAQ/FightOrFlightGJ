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
        public Vector3 rotation;
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

    public HandState lastState;

    void Start()
    {
        lastState = HandState.Lowered; // Default starting state
    }

    // Function to set the hand to a specific state immediately
    public void SetHandState(HandState state)
    {
        if (handStateTransforms.ContainsKey(state))
        {
            // Set the left hand transform based on the state data
            leftHand.transform.localPosition = handStateTransforms[state].position;
            leftHand.transform.localRotation = Quaternion.Euler(handStateTransforms[state].rotation);
            leftHand.transform.localScale = handStateTransforms[state].scale;

            // Set the right hand transform by mirroring the left hand's position.x and rotation.y
            Vector3 rightHandPosition = handStateTransforms[state].position;
            rightHandPosition.x *= -1; // Mirror the x position

            Vector3 rightHandRotation = handStateTransforms[state].rotation;
            rightHandRotation.y *= -1; // Mirror the y rotation

            rightHand.transform.localPosition = rightHandPosition;
            rightHand.transform.localRotation = Quaternion.Euler(rightHandRotation);
            rightHand.transform.localScale = handStateTransforms[state].scale; // Keep scale the same

            lastState = state; // Update last state
            Debug.Log($"Hand set to {state} state.");
        }
        else
        {
            Debug.LogWarning($"No transform data assigned for {state} state.");
        }
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
            HandTransformData lastTransform = handStateTransforms[lastState];

            // Interpolate the target transform based on the percentage between the last state and the target state for the left hand
            Vector3 interpolatedPosition = Vector3.Lerp(lastTransform.position, targetTransform.position, percentage);
            Vector3 interpolatedScale = Vector3.Lerp(lastTransform.scale, targetTransform.scale, percentage);
            Quaternion interpolatedRotation = Quaternion.Slerp(Quaternion.Euler(lastTransform.rotation), Quaternion.Euler(targetTransform.rotation), percentage);

            // Mirror the x position and y rotation for the right hand
            Vector3 rightHandTargetPosition = targetTransform.position;
            rightHandTargetPosition.x *= -1; // Mirror x position for right hand

            Vector3 rightHandLastPosition = lastTransform.position;
            rightHandLastPosition.x *= -1; // Mirror x position for right hand last state

            Vector3 rightHandTargetRotation = targetTransform.rotation;
            rightHandTargetRotation.y *= -1; // Mirror y rotation for right hand

            Vector3 rightHandLastRotation = lastTransform.rotation;
            rightHandLastRotation.y *= -1; // Mirror y rotation for right hand last state

            Vector3 initialLeftPosition = leftHand.transform.localPosition;
            Quaternion initialLeftRotation = leftHand.transform.localRotation;
            Vector3 initialLeftScale = leftHand.transform.localScale;

            Vector3 initialRightPosition = rightHand.transform.localPosition;
            Quaternion initialRightRotation = rightHand.transform.localRotation;
            Vector3 initialRightScale = rightHand.transform.localScale;

            float elapsedTime = 0f;

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;

                // Ease in and out (using Mathf.SmoothStep for smoother transition)
                leftHand.transform.localPosition = Vector3.Lerp(initialLeftPosition, interpolatedPosition, Mathf.SmoothStep(0f, 1f, t));
                leftHand.transform.localRotation = Quaternion.Slerp(initialLeftRotation, interpolatedRotation, Mathf.SmoothStep(0f, 1f, t));
                leftHand.transform.localScale = Vector3.Lerp(initialLeftScale, interpolatedScale, Mathf.SmoothStep(0f, 1f, t));

                // Ease the right hand by mirroring left hand positions and rotations
                rightHand.transform.localPosition = Vector3.Lerp(initialRightPosition, Vector3.Lerp(rightHandLastPosition, rightHandTargetPosition, percentage), Mathf.SmoothStep(0f, 1f, t));
                rightHand.transform.localRotation = Quaternion.Slerp(initialRightRotation, Quaternion.Slerp(Quaternion.Euler(rightHandLastRotation), Quaternion.Euler(rightHandTargetRotation), percentage), Mathf.SmoothStep(0f, 1f, t));
                rightHand.transform.localScale = Vector3.Lerp(initialRightScale, interpolatedScale, Mathf.SmoothStep(0f, 1f, t));

                yield return null;
            }

            // Ensure final values are set for both hands
            leftHand.transform.localPosition = interpolatedPosition;
            leftHand.transform.localRotation = interpolatedRotation;
            leftHand.transform.localScale = interpolatedScale;

            rightHand.transform.localPosition = Vector3.Lerp(rightHandLastPosition, rightHandTargetPosition, percentage);
            rightHand.transform.localRotation = Quaternion.Slerp(Quaternion.Euler(rightHandLastRotation), Quaternion.Euler(rightHandTargetRotation), percentage);
            rightHand.transform.localScale = interpolatedScale;

            lastState = targetState; // Update the last state to the new state

            Debug.Log($"Hands eased to {targetState} state with {percentage * 100}% interpolation.");
        }
        else
        {
            Debug.LogWarning($"No transform data assigned for {targetState} or {lastState} state.");
        }
    }

}
