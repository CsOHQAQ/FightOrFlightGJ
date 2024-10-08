using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoomControler : MonoBehaviour
{
    public bool isClear;
    public bool isRefreshing;

    public List<MonsterStats> monsters;
    public List<DoubleDoor> doors;

    private CombatManager combatManager;


    private void Start()
    {
        monsters=new List<MonsterStats>();
        InitMonsters();
        doors=new List<DoubleDoor>();
        InitDoors();
        combatManager=GetComponent<CombatManager>();

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


            //Remove Dead Monster
            List<MonsterStats> removeList = new List<MonsterStats>();
            foreach (var monster in monsters)
            {
                if (monster.CurHealth <= 0)
                {
                    removeList.Add(monster);
                }
            }
            foreach (var remove in removeList)
            {
                monsters.Remove(remove);
                Destroy(remove.gameObject);
            }
            if (monsters.Count <= 0)
            {
                isClear = true;
            }


            if (!combatManager.inCombat)
            {
                foreach (MonsterStats monster in monsters)
                {
                    monster.RefreshAwareness(isViewedByPlayer);
                    if (monster.awareLevel == MonsterStats.AwareLevel.Awared)
                    {
                        EngageCombat();
                        return;
                    }
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
        //TODO
        Debug.LogWarning("EnterBattle!");
        combatManager.StartBattle(GameControl.Game.Player.GetComponent<PlayerStats>(),monsters);
        foreach (var door in doors)
        {
            door.Close();
        }

    }
}
