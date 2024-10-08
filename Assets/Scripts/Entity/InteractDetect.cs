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
        //Input System Detect
        TryInteract();
    }

    void TryInteract()
    {
        RaycastHit hitInfo;
        Debug.DrawLine(transform.position,transform.position+transform.forward*DetectLength,Color.red);
        if (Physics.Raycast(transform.position,transform.forward, out hitInfo, DetectLength,6))
        {            
            Debug.Log($"{hitInfo.transform.name}, its layer mask: {hitInfo.transform.gameObject.layer}");
        }
    }

}
