using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractDetect : MonoBehaviour
{
    [SerializeField]
    private float DetectLength;

    private float scrollValue;
    public float ScrollValue { get { return scrollValue; }}
    InputControls controls;

    private void Awake()
    {
        controls = new InputControls();
    }

    private void OnEnable()
    {
        controls.Enable();
        //controls.Player.Interact.started += TryInteract;
        //controls.Player.OpenDoor.performed += TryInteract; // Subscribe to the OpenDoor action
    }

    private void OnDisable()
    {
        //controls.Player.Interact.started -= TryInteract;
       // controls.Player.OpenDoor.performed -= TryInteract; // Unsubscribe when disabled

        controls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public bool TryInteract(InputAction.CallbackContext context)
    {
        RaycastHit hitInfo;
        Debug.DrawLine(transform.position, transform.position + transform.forward * DetectLength, Color.red);
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, DetectLength))
        {
            //Debug.Log($"{hitInfo.transform.name}, its layer mask: {hitInfo.transform.gameObject.layer}");
            if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Interactable Obj"))
            {
                //interactable object detected.
                hitInfo.transform.GetComponent<InteractableObject>().Interact(this);
                scrollValue += context.ReadValue<Vector2>().y*0.1f;
                scrollValue = Mathf.Clamp(scrollValue, 0f, 1f);
                
                return true;
            }
            scrollValue = 0f;


        }
        return false;
    }


}
