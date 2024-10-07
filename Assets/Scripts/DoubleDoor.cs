using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DoubleDoor : MonoBehaviour
{
    public GameObject LeftPart,RightPart;
    public float EnterBattleTime;
    public float OpenTime;
    public bool isOpen;
    public bool CanOpen=true;

    RoomControler room;
    int openTimes;
    float openTimer;
    public void Open()
    {
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

        //Just for testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(!isOpen)
                Open();
            else
                Close();
        }


        if (isOpen)
        {
            LeftPart.transform.rotation = Quaternion.Euler(0, Mathf.MoveTowardsAngle(LeftPart.transform.rotation.eulerAngles.y, -90, 90*Time.deltaTime / OpenTime), 0);
            RightPart.transform.rotation = Quaternion.Euler(0, Mathf.MoveTowardsAngle(RightPart.transform.rotation.eulerAngles.y, 90, 90 * Time.deltaTime / OpenTime), 0);
            openTimer += Time.deltaTime;
            if (openTimer > OpenTime) 
            {
                EnterBattle();
            }
        }
        else
        {
            openTimer = 0f;
            LeftPart.transform.rotation = Quaternion.Euler(0, Mathf.MoveTowardsAngle(LeftPart.transform.rotation.eulerAngles.y, 0, 90 * Time.deltaTime / OpenTime), 0);
            RightPart.transform.rotation = Quaternion.Euler(0, Mathf.MoveTowardsAngle(RightPart.transform.rotation.eulerAngles.y, 0, 90 * Time.deltaTime / OpenTime), 0);
        }
    }

    public void EnterBattle()
    {
        Debug.Log("Enter Battle");
        CanOpen = false;
    }

}
