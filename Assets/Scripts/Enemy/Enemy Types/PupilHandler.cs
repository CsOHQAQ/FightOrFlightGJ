using UnityEngine;

public class PupilHandler : MonoBehaviour
{
    [Header("Pupil Movement Settings")]
    [Tooltip("Max +/- local offset in X and Y from the parent center.")]
    public Vector2 maxOffset = new Vector2(0.5f, 0.5f);

    [Tooltip("An offset to add to the target’s position in world space.")]
    public Vector3 targetLookOffset = Vector3.zero;

    [Tooltip("Speed at which the pupil moves (lerp factor).")]
    public float lerpSpeed = 10f;

    [Header("Hide/Show Pupil")]
    [Tooltip("If true, the pupil becomes invisible (scaled to zero).")]
    public bool hidePupil = false;

    private Transform parentEye;      // The "eye" transform (parent)
    private Vector3 originalScale;    // The pupil's initial local scale

    private void Start()
    {
        parentEye = transform.parent;
        originalScale = transform.localScale;
    }

    private void Update()
    {
        // Hide/show the pupil by scaling
        transform.localScale = hidePupil ? Vector3.zero : originalScale;
    }

    /// <summary>
    /// Positions the pupil so that its local Z remains fixed, 
    /// and X/Y are derived from angles in XZ and YZ planes (with clamping).
    /// Then rotates around local Y so it "yaws" toward the target horizontally.
    /// </summary>
    public void TrackGameObject(Transform target)
    {
        if (parentEye == null || target == null) return;

        // 1) World point we want to look at
        Vector3 worldLookPos = target.position + targetLookOffset;

        // 2) Convert that vector to the parent's local direction
        Vector3 localDirection = parentEye.InverseTransformDirection(worldLookPos - parentEye.position);

        // Debug line to visualize
        Debug.DrawLine(parentEye.position, worldLookPos, Color.magenta);

        // 3) The pupil's local Z is our 'adjacent' side
        float currentZ = transform.localPosition.z;

        // 4) Angle in the XZ plane => angleX = atan2(x, z)
        float angleX = Mathf.Atan2(localDirection.x, localDirection.z);

        // 5) Angle in the YZ plane => angleY = atan2(y, z)
        float angleY = Mathf.Atan2(localDirection.y, localDirection.z);

        // 6) Opposite side = tan(angle) * adjacent
        float offsetX = Mathf.Tan(angleX) * currentZ;
        float offsetY = Mathf.Tan(angleY) * currentZ;

        // 7) Clamp the local X and Y
        offsetX = Mathf.Clamp(offsetX, -maxOffset.x, maxOffset.x);
        offsetY = Mathf.Clamp(offsetY, -maxOffset.y, maxOffset.y);

        // 8) Final local position (keep the same Z)
        Vector3 desiredLocalPos = new Vector3(offsetX, offsetY, currentZ);

        // 9) Lerp local position
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            desiredLocalPos,
            Time.deltaTime * lerpSpeed
        );

        // 10) Rotate around local Y to "yaw" toward the target horizontally
        // Angle in degrees from parent's forward axis: 
        float yawAngleDeg = angleX * Mathf.Rad2Deg;

        // Build the final local rotation, only rotating around Y
        Quaternion desiredYaw = Quaternion.Euler(0f, yawAngleDeg, 0f);

        // Lerp the pupil's local rotation
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            desiredYaw,
            Time.deltaTime * lerpSpeed
        );
    }
}
