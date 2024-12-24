using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Room : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    public List<Door> doors;
    [SerializeField]
    public List<BaseMonster> enemies;
    //public List<BaseMonster> deadEnemies;
    public event Action<Room> OnCombatStartedInRoom;
    public event Action<Room> OnCombatEndedInRoom;

    private bool isCombatActive = false;
    void Start()
    {
        
    }
    
    public void StartCombat()
    {
        if (isCombatActive) return;
        isCombatActive = true;
        
        

        // do more logic
    }

    public void InitializeDoors()
    {
        foreach (Door door in doors)
        {
            OnCombatStartedInRoom+=door.OnCombatStartedInRoom;
            OnCombatEndedInRoom+=door.OnCombatEndedInRoom;
        }
        foreach (BaseMonster enemy in enemies)
        {
            OnCombatStartedInRoom+=enemy.OnCombatStartedInRoom;
            OnCombatEndedInRoom+=enemy.OnCombatEndedInRoom;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
