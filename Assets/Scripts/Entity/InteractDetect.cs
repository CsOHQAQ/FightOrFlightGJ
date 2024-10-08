using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractDetect : MonoBehaviour
{
    [SerializeField]
    private float DetectLength;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //TODO:Use Input System Detect
        if(Input.GetKeyDown(KeyCode.F))
            TryInteract();
    }

    void TryInteract()
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
