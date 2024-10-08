using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DoubleDoor : InteractableObject
{
    public GameObject LeftPart,RightPart;
    public float OpenTime;
    public bool isOpen;
    public bool CanOpen=true;

    RoomControler room;
    int openTimes;
    float openTimer;

    public override void Interact(object args = null)
    {
        base.Interact(args); 
        if (!isOpen)
            Open();
        else
            Close();
    }
    public void Open()
    {
        //TODO: Maybe add opening from clockwise or anticlockwise
        isOpen = true;
        openTimes++;
    }
    public void Close()
    {
        isOpen = false;
    }

    public void Init(RoomControler iRoom)
    {
        room = iRoom;
        openTimer = 0f;
        openTimes = 0;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        //Just for testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(!isOpen)
                Open();
            else
                Close();
        }
        */

        if (isOpen)
        {
            RightPart.transform.localEulerAngles = new Vector3(0, Mathf.MoveTowardsAngle(RightPart.transform.localEulerAngles.y, 90, 90 * Time.deltaTime / OpenTime), 0);
            LeftPart.transform.localEulerAngles = new Vector3(0, Mathf.MoveTowardsAngle(LeftPart.transform.localEulerAngles.y, -90, 90 * Time.deltaTime / OpenTime), 0);
            openTimer += Time.deltaTime;
            if (openTimer > OpenTime) 
            {
                EnterBattle();
            }
        }
        else
        {
            openTimer = 0f;
            RightPart.transform.localEulerAngles = new Vector3(0, Mathf.MoveTowardsAngle(RightPart.transform.localEulerAngles.y, 0, 90 * Time.deltaTime / OpenTime), 0);
            LeftPart.transform.localEulerAngles = new Vector3(0, Mathf.MoveTowardsAngle(LeftPart.transform.localEulerAngles.y, 0, 90 * Time.deltaTime / OpenTime), 0);
        }
    }

    public void EnterBattle()
    {
        Debug.Log("Enter Battle");
        CanOpen = false;
    }

}
