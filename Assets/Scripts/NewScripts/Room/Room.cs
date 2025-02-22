using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Room : MonoBehaviour
{
    [SerializeField]
    public List<Door> doors;
    [SerializeField]
    public List<Enemy> enemies;

    [SerializeField]
    private List<ArtifactSO> rewardList;

    private int remainingEnemyNum;
    public event Action<Room> OnCombatStartedInRoom;
    public event Action<Room> OnCombatEndedInRoom;

    private bool hasCombatEncounter = true;
    private bool isCombatActive = false;

    private float roomClearingScore=0f;

    void Start()
    {
        InitializeDoors();
    }
    
    public void StartCombat()
    {
        if (isCombatActive || !hasCombatEncounter) return;
        isCombatActive = true;
        
        OnCombatStartedInRoom?.Invoke(this);

        // Additional logic for starting combat
        Debug.Log("Combat has started!");
    }

    public void InitializeDoors()
    {
        foreach (Door door in doors)
        {
            door.Room = this;
            OnCombatStartedInRoom += door.OnCombatStartedInRoom;
            door.OnDoorFullyOpened += OnDoorFullyOpened;
            OnCombatEndedInRoom   += door.OnCombatEndedInRoom;
        }
        foreach (Enemy enemy in enemies)
        {
            OnCombatStartedInRoom += enemy.OnCombatStartedInRoom;
            OnCombatEndedInRoom   += enemy.OnCombatEndedInRoom;
            enemy.OnCharacterDied += OnEnemyInRoomDied;
            roomClearingScore     += enemy.ScoreOnKill;
        }
        remainingEnemyNum = enemies.Count;
    }

    private void OnDoorFullyOpened()
    {
        // Instead of calling StartCombat() directly, we do a small delay
        StartCoroutine(DelayedStartCombat());
    }

    private IEnumerator DelayedStartCombat()
    {
        // Wait 1 second before actually starting combat
        yield return new WaitForSeconds(1f);
        StartCombat();
    }

    void Update()
    {
        // ...
    }

    private void OnEnemyInRoomDied(ICharacter enemyCharacter)
    {
        enemyCharacter.OnCharacterDied -= OnEnemyInRoomDied;
        
        var deadEnemy = enemyCharacter as Enemy;
        if (deadEnemy != null)
        {
            remainingEnemyNum--;
            Debug.Log($"{deadEnemy} has died. Enemies left: {remainingEnemyNum}");
        }
        else
        {
            Debug.LogWarning("Attempted to remove a non-monster character from the enemies list.");
        }

        if (remainingEnemyNum == 0)
        {
            Debug.Log("Room is Cleared!");
            OnCombatEndedInRoom?.Invoke(this);
            GameManager.Instance.AddScore(roomClearingScore);
        }
    }

    public void AddEnemyToRoom(Enemy enemyCharacter)
    {
        enemyCharacter.OnCharacterDied += OnEnemyInRoomDied;
        enemies.Add(enemyCharacter);
        remainingEnemyNum++;
    }
}