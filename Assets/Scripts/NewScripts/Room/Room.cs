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
    private int remainingEnemyNum;
    //public List<BaseMonster> deadEnemies;
    public event Action<Room> OnCombatStartedInRoom;
    public event Action<Room> OnCombatEndedInRoom;
    private bool hasCombatEncounter = true;
    private bool isCombatActive = false;
    void Start()
    {
        InitializeDoors();
    }
    
    public void StartCombat()
    {
        if (isCombatActive||!hasCombatEncounter) return;
        isCombatActive = true;
        
        OnCombatStartedInRoom?.Invoke(this);

        // do more logic
    }

    public void InitializeDoors()
    {
        foreach (Door door in doors)
        {
            door.Room = this;
            OnCombatStartedInRoom+=door.OnCombatStartedInRoom;
            OnCombatEndedInRoom+=door.OnCombatEndedInRoom;
        }
        foreach (BaseMonster enemy in enemies)
        {
            OnCombatStartedInRoom+=enemy.OnCombatStartedInRoom;
            OnCombatEndedInRoom+=enemy.OnCombatEndedInRoom;
            enemy.OnCharacterDied+=OnEnemyInRoomDied;
        }
        remainingEnemyNum = enemies.Count;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnemyInRoomDied(ICharacter enemyCharacter)
    {
        // Unsubscribe from the OnCharacterDied event
        enemyCharacter.OnCharacterDied -= OnEnemyInRoomDied;
        
        // Attempt to cast the enemyCharacter to BaseMonster
        var baseMonster = enemyCharacter as BaseMonster;
        if (baseMonster != null)
        {
            //enemies.Remove(baseMonster);
            remainingEnemyNum--;
            Debug.Log($"{baseMonster} has been removed from the enemies list.");
        }
        else
        {
            Debug.LogWarning("Attempted to remove a non-monster character from the enemies list.");
        }
        if(remainingEnemyNum==0)
        {
            Debug.Log("Room is Cleared");
            OnCombatEndedInRoom?.Invoke(this);
        }
    }

    public void AddEnemyToRoom(BaseMonster enemyCharacter)
    {
        enemyCharacter.OnCharacterDied+=OnEnemyInRoomDied;
        enemies.Add(enemyCharacter);
        remainingEnemyNum++;
    }
}
