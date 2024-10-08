using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractDetect : MonoBehaviour
{
    [SerializeField]
    private float DetectLength;
    InputControls controls;

    private void Awake()
    {
        controls = new InputControls();
    }

    // Start is called before the first frame update
    private void OnEnable()
    {       
        controls.Enable();
        controls.Player.Interact.started += TryInteract;
    }
    private void OnDisable()
    {
        controls.Player.Interact.started -= TryInteract;

        controls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void TryInteract(InputAction.CallbackContext context)
    {
        RaycastHit hitInfo;
        Debug.DrawLine(transform.position,transform.position+transform.forward*DetectLength,Color.red);
        if (Physics.Raycast(transform.position,transform.forward, out hitInfo, DetectLength))
        {            
            Debug.Log($"{hitInfo.transform.name}, its layer mask: {hitInfo.transform.gameObject.layer}");
            if(hitInfo.transform.gameObject.layer==LayerMask.NameToLayer("Interactable Obj"))
                hitInfo.transform.GetComponent<InteractableObject>().Interact();
        }
    }   

}
