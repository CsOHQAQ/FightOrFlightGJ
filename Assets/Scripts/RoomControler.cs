using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoomControler : MonoBehaviour
{
    public bool isClear;
    public bool isRefreshing;

    public List<MonsterStats> monsters;
    public List<DoubleDoor> doors;
    private void Start()
    {
        monsters=new List<MonsterStats>();
        InitMonsters();
        doors=new List<DoubleDoor>();


    }
    private void Update()
    {
        
    }



    void InitMonsters()
    {
        //TODO: Get the monstersData
    }
    void InitDoors()
    {
        foreach (var door in GameObject.FindGameObjectsWithTag("Door"))
        {
            door.GetComponent<DoubleDoor>().Init(this);
            doors.Add(door.GetComponent<DoubleDoor>());            
        }
    }
}
