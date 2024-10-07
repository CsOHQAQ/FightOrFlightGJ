using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal;
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
        InitDoors();
    }
    private void Update()
    {
        if (isRefreshing)
        {
            bool isViewedByPlayer=false;
            foreach (var door in doors)
            {
                if (door.isOpen)
                {
                    isViewedByPlayer = true;
                }
            }

            foreach (MonsterStats monster in monsters)
            {
                monster.RefreshAwareness(isViewedByPlayer);
                if (monster.isAwared)
                {

                    EngageCombat();
                }
            }

        }
    }

    void InitMonsters()
    {
        foreach(MonsterStats monster in GetComponentsInChildren<MonsterStats>())
        {
            //TODO: Other Monster Init
            monsters.Add(monster);
        }
    }
    void InitDoors()
    {
        foreach (var door in GetComponentsInChildren<DoubleDoor>())
        {
            door.Init(this);
            doors.Add(door.GetComponent<DoubleDoor>());            
        }
    }

    void EngageCombat()
    {

    }
}
