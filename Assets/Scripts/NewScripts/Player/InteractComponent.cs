using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractComponent : MonoBehaviour
{   
    private PlayerInput playerInput;

    [SerializeField]
    private float MaxLineCastDistance = 3f;

    [SerializeField] private LayerMask enemyLayer;


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        
    }

    private void Start()
    {
        //PerformInteractionCheck(transform.right);
    }



    public InteractInfo PerformInteractionCheck(Vector3 direction)
    {
        RaycastHit hit;
        Vector3 start = transform.position;
        Vector3 end = start + direction * MaxLineCastDistance;

        // Draw the line in the Scene view for debugging
        Debug.DrawLine(start, end, Color.red, 100f); // Draws a red line for 0.1 seconds

        
        // Quick Cringe Fix, Should be updated later
        if (Physics.Raycast(start, direction, out hit, MaxLineCastDistance, enemyLayer))
        {
            // Nothing Happens 
        }
        else
        {
            // Perform the raycast from this object's position in the specified direction
            if (Physics.Raycast(start, direction, out hit, MaxLineCastDistance))
            {
                // Check if the object hit has a component that implements IInteractable
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(this.gameObject);

                }
                return new InteractInfo(hit.collider.gameObject, interactable, hit);
            }
        }

        // Return null if no IInteractable was hit
        return new InteractInfo(null,null,hit);
    }

        
}

public struct InteractInfo
{
    public GameObject InteractableObject { get; }
    public IInteractable Interactable { get; }
    public RaycastHit HitInfo { get; }

    // Constructor to initialize the struct
    public InteractInfo(GameObject interactableObject, IInteractable interactable, RaycastHit hitInfo)
    {
        InteractableObject = interactableObject;
        if (interactable!=null)
        {Interactable = interactable;}
        else{
            Interactable = null;
        }
        HitInfo = hitInfo;
    }
}