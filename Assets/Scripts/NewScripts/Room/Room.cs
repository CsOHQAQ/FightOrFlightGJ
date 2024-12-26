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
        
        OnCombatStartedInRoom?.Invoke(this);

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
            enemy.OnCharacterDied+=OnEnemyInRoomDied;
        }
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
            enemies.Remove(baseMonster);
            Debug.Log($"{baseMonster} has been removed from the enemies list.");
        }
        else
        {
            Debug.LogWarning("Attempted to remove a non-monster character from the enemies list.");
        }
        if(enemies.Count<=0)
        {
            OnCombatEndedInRoom?.Invoke(this);
        }
    }

    public void AddEnemyToRoom(BaseMonster enemyCharacter)
    {
        enemyCharacter.OnCharacterDied+=OnEnemyInRoomDied;
        enemies.Add(enemyCharacter);
    }
}
