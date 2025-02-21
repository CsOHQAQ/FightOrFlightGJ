using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractComponent : MonoBehaviour
{
    [SerializeField]
    private float MaxLineCastDistance = 3f;

    [SerializeField] private LayerMask enemyLayer;

    public InteractInfo PerformInteractionCheck(GameObject source, Vector3 direction, float interactValue)
    {
        RaycastHit hit;
        Vector3 start = transform.position;
        Vector3 end = start + direction * MaxLineCastDistance;

        Debug.DrawLine(start, end, Color.red, 1f);

        int layerMask = ~(1 << LayerMask.NameToLayer("Enemy"));

        if (Physics.Raycast(start, direction, out hit, MaxLineCastDistance, layerMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            return new InteractInfo(source, hit.collider.gameObject, interactable, hit, interactValue);
        }

        return new InteractInfo(source, null, null, default, interactValue);
    }
}
    public struct InteractInfo
    {
        public GameObject InteractableObject { get; }
        public GameObject Source {get; }
        public IInteractable Interactable { get; }
        public RaycastHit HitInfo { get; }

        public float InteractValue;

        // Constructor to initialize the struct
        public InteractInfo(GameObject interactionSource, GameObject interactableObject, IInteractable interactable, RaycastHit hitInfo, float interactValue)
        {
            Source = interactionSource;
            InteractableObject = interactableObject;
            if (interactable!=null)
            {Interactable = interactable;}
            else{
                Interactable = null;
            }
            HitInfo = hitInfo;
            InteractValue = interactValue;
        }
    }
    