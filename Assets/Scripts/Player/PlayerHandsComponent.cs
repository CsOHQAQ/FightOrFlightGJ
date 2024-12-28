using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum HandState
{
    Lowered,
    Raised,
    Pushed
}

public class PlayerHandsComponent : SerializedMonoBehaviour
{
    [System.Serializable]
    public struct HandTransformData
    {
        public Vector3 position;
        public Vector3 rotation; // Rotation in Euler angles
        public Vector3 scale;
    }

    private PlayerCharacter ownerCharacter;

    [SerializeField]
    private GameObject leftHand;

    [SerializeField]
    private GameObject rightHand;

    [SerializeField, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<HandState, HandTransformData> handStateTransforms = new Dictionary<HandState, HandTransformData>();

    [SerializeField]
    private float transitionDuration = 1f;

    private float leftHandRotationYVelocity = 0f;
    private float leftHandRotationZVelocity = 0f;
    private float rightHandRotationYVelocity = 0f;
    private float rightHandRotationZVelocity = 0f;

    private HandState currentState;
    public HandState lastState;

    public void Initialize(PlayerCharacter ownerCharacter)
    {
        this.ownerCharacter = ownerCharacter;
    }

    void Start()
    {
        ChangeState(HandState.Lowered); // Initialize to default state
    }

    void Update()
    {
        OnUpdate(currentState); // Call OnUpdate for the current state
    }

    // State machine core
    public void ChangeState(HandState newState)
    {
        OnStateExit(currentState); // Handle exit logic for the current state
        lastState = currentState; // Update last state
        currentState = newState; // Set new state
        OnStateEnter(currentState); // Handle enter logic for the new state
    }

    private void OnStateEnter(HandState state)
    {
        switch (state)
        {
            case HandState.Lowered:
                StartCoroutine(EaseToState(state));
                break;
            case HandState.Raised:
                ChangeHandsVisibility(true);
                StartCoroutine(EaseToState(state));
                break;

            case HandState.Pushed:
                Debug.Log("Entering Pushed State");
                break;

            default:
                Debug.LogWarning($"Unhandled state: {state}");
                break;
        }
    }

    private void OnStateExit(HandState state)
    {
        switch (state)
        {
            case HandState.Lowered:
            case HandState.Raised:
            case HandState.Pushed:
                Debug.Log($"Exiting {state} State");
                break;

            default:
                Debug.LogWarning($"Unhandled state: {state}");
                break;
        }
    }

    private void OnUpdate(HandState state)
    {
        switch (state)
        {
            case HandState.Pushed:
                UpdateHandTransformsForPush();
                break;
        }
    }

    private void UpdateHandTransformsForPush()
    {
        // Check if the owner character and door are assigned
        if (ownerCharacter?.CurrentDoor == null)
        {
            Debug.LogWarning("Owner character or CurrentDoor is not set.");
            return;
        }

        // Get the door's current openness, which ranges from 0 to 1
        float openness = ownerCharacter.CurrentDoor.CurrentOpenness;

        // Retrieve Raised and Pushed transforms from the dictionary
        if (handStateTransforms.TryGetValue(HandState.Raised, out HandTransformData raisedTransform) &&
            handStateTransforms.TryGetValue(HandState.Pushed, out HandTransformData pushedTransform))
        {
            //Debug.LogWarning(openness);
            // Interpolate the position, scale, and rotation based on openness
            Vector3 interpolatedPosition = Vector3.Lerp(raisedTransform.position, pushedTransform.position, openness);
            Vector3 interpolatedScale = Vector3.Lerp(raisedTransform.scale, pushedTransform.scale, openness);
            Vector3 interpolatedRotation = Vector3.Lerp(raisedTransform.rotation, pushedTransform.rotation, openness);

            // Mirror x position for the right hand
            Vector3 interpolatedRightHandPosition = interpolatedPosition;
            interpolatedRightHandPosition.x *= -1;

            // Apply interpolated transformations to left and right hands
            leftHand.transform.localPosition = interpolatedPosition;
            leftHand.transform.localScale = interpolatedScale;
            leftHand.transform.localEulerAngles = interpolatedRotation;

            rightHand.transform.localPosition = interpolatedRightHandPosition;
            rightHand.transform.localScale = interpolatedScale;
            rightHand.transform.localEulerAngles = new Vector3(interpolatedRotation.x, -interpolatedRotation.y, interpolatedRotation.z);
        }
        else
        {
            Debug.LogWarning("Hand transform data for Raised or Pushed state is missing.");
        }
    }

    // Coroutine to ease both hands to the transform recorded in the given state
    public IEnumerator EaseToState(HandState targetState, float percentage = 1f)
    {
        if (handStateTransforms.ContainsKey(targetState) && handStateTransforms.ContainsKey(lastState))
        {
            percentage = Mathf.Clamp01(percentage);

            HandTransformData targetTransform = handStateTransforms[targetState];
            HandTransformData lastTransform = handStateTransforms[lastState];

            Vector3 actualTargetPosition = lastTransform.position + (targetTransform.position - lastTransform.position) * percentage;
            Vector3 actualTargetScale = lastTransform.scale + (targetTransform.scale - lastTransform.scale) * percentage;
            Vector3 actualTargetRotation = lastTransform.rotation + (targetTransform.rotation - lastTransform.rotation) * percentage;

            Vector3 actualRightHandPosition = actualTargetPosition;
            actualRightHandPosition.x *= -1;

            float elapsedTime = 0f;

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;

                leftHand.transform.localPosition = Vector3.Lerp(leftHand.transform.localPosition, actualTargetPosition, Mathf.SmoothStep(0f, 1f, t));
                leftHand.transform.localScale = Vector3.Lerp(leftHand.transform.localScale, actualTargetScale, Mathf.SmoothStep(0f, 1f, t));

                rightHand.transform.localPosition = Vector3.Lerp(rightHand.transform.localPosition, actualRightHandPosition, Mathf.SmoothStep(0f, 1f, t));
                rightHand.transform.localScale = Vector3.Lerp(rightHand.transform.localScale, actualTargetScale, Mathf.SmoothStep(0f, 1f, t));

                float currentLeftRotationY = Mathf.SmoothDampAngle(leftHand.transform.localEulerAngles.y, actualTargetRotation.y, ref leftHandRotationYVelocity, transitionDuration);
                float currentLeftRotationZ = Mathf.SmoothDampAngle(leftHand.transform.localEulerAngles.z, actualTargetRotation.z, ref leftHandRotationZVelocity, transitionDuration);

                float currentRightRotationY = Mathf.SmoothDampAngle(rightHand.transform.localEulerAngles.y, -actualTargetRotation.y, ref rightHandRotationYVelocity, transitionDuration);
                float currentRightRotationZ = Mathf.SmoothDampAngle(rightHand.transform.localEulerAngles.z, actualTargetRotation.z, ref rightHandRotationZVelocity, transitionDuration);

                leftHand.transform.localEulerAngles = new Vector3(leftHand.transform.localEulerAngles.x, currentLeftRotationY, currentLeftRotationZ);
                rightHand.transform.localEulerAngles = new Vector3(rightHand.transform.localEulerAngles.x, currentRightRotationY, currentRightRotationZ);

                yield return null;
            }

            leftHand.transform.localPosition = actualTargetPosition;
            leftHand.transform.localScale = actualTargetScale;
            leftHand.transform.localEulerAngles = new Vector3(leftHand.transform.localEulerAngles.x, actualTargetRotation.y, actualTargetRotation.z);

            rightHand.transform.localPosition = actualRightHandPosition;
            rightHand.transform.localScale = actualTargetScale;
            rightHand.transform.localEulerAngles = new Vector3(rightHand.transform.localEulerAngles.x, -actualTargetRotation.y, actualTargetRotation.z);

            if(currentState==HandState.Raised)
            {
                Debug.Log("Raise Hand Sequence Finished");
                ChangeState(HandState.Pushed);
            }else if (currentState==HandState.Lowered)
            {
                ChangeHandsVisibility(false);
            }
        }
        else
        {
            Debug.LogWarning($"No transform data assigned for {targetState} or {lastState} state.");
        }
    }

    public void ChangeHandsVisibility(bool isVisible)
    {
        leftHand.SetActive(isVisible);
        rightHand.SetActive(isVisible);
    }
}
