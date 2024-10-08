using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector; // Add this to use Odin attributes

public class CombatManager : MonoBehaviour
{
    [SerializeField]
    private Combatant playerCombatant;

    [SerializeField]
    private List<Combatant> enemyCombatants = new List<Combatant>();

    [SerializeField]
    private float turnDuration = 2.0f; // Duration of each turn in seconds

    private float turnTimer = 0f; // Timer to track time between turns
    private int currentEnemyIndex = 0;
    private bool inCombat = true;

    public void StartBattle(PlayerStats player, List<MonsterStats> monsters)
    {
        if (playerCombatant == null)
        {
            playerCombatant = new Combatant(10, 10, 2, 0);
        }

        for (int i = 0; i < enemyCombatants.Count; i++)
        {
            if (enemyCombatants[i] == null)
            {
                enemyCombatants[i] = new Combatant(2, 2, 1, 0);
            }
        }

        inCombat = true;
    }

    void Update()
    {
        if (inCombat)
        {
            // Increment the timer by the time elapsed since the last frame
            turnTimer += Time.deltaTime;

            // Check if the timer has exceeded the turn duration
            if (turnTimer >= turnDuration)
            {
                TakeTurn();
                // Reset the timer after a turn has been processed
                turnTimer = 0f;
            }
        }
    }

    void TakeTurn()
    {
        
        Debug.Log("Starting a new turn...");
        
        // Calculate and log damage to the player. You deal at least 1 damage each turn. 
        float damageToPlayer = Mathf.Max(1f, enemyCombatants[currentEnemyIndex].Attack - playerCombatant.Defense);
        Debug.Log($"Enemy {currentEnemyIndex} attacks Player: Damage = {damageToPlayer}");
        playerCombatant.TakeDamage(damageToPlayer);
        Debug.Log($"Player health after taking damage: {playerCombatant.Health}");

        // Calculate and log damage to the enemy
        float damageToEnemy = Mathf.Max(1f, playerCombatant.Attack - enemyCombatants[currentEnemyIndex].Defense);
        Debug.Log($"Player attacks Enemy {currentEnemyIndex}: Damage = {damageToEnemy}");
        enemyCombatants[currentEnemyIndex].TakeDamage(damageToEnemy);
        Debug.Log($"Enemy {currentEnemyIndex} health after taking damage: {enemyCombatants[currentEnemyIndex].Health}");

        // Check if the player is dead and log it
        if (playerCombatant.Health <= 0f)
        {
            Debug.Log("Player has died.");
            CombatOutcome(false);
            return;
        }

        // Check if the current enemy is dead and log it
        if (enemyCombatants[currentEnemyIndex].Health <= 0f)
        {
            Debug.Log($"Enemy {currentEnemyIndex} has died.");
            currentEnemyIndex++;

            // Check if all enemies are dead and log the combat outcome
            if (currentEnemyIndex >= enemyCombatants.Count)
            {
                CombatOutcome(true);
            }
            else
            {
                Debug.Log($"Next enemy is now Enemy {currentEnemyIndex}");
            }
        }
        else
        {
            Debug.Log($"Enemy {currentEnemyIndex} is still alive.");
        }
    }

    void CombatOutcome(bool combatWon)
    {
        if (combatWon)
        {
            Debug.Log($"Combat Won! All enemies have been defeated. Player's remaining health: {playerCombatant.Health}");
        }
        else
        {
            Debug.Log("Combat Lost! The player has died.");
        }
        inCombat = false;
    }
}
