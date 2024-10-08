using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector; // Add this to use Odin attributes

public class CombatManager : MonoBehaviour
{
    //[ShowInInspector, InlineEditor] // Allows inline editing of playerCombatant
    [SerializeField]
    private Combatant playerCombatant;

    //[ShowInInspector, ListDrawerSettings(Expanded = true)] // Allow adding/removing and editing combatants in Inspector
    [SerializeField]
    private List<Combatant> enemyCombatants = new List<Combatant>();

    private int currentEnemyIndex = 0;
    private bool inCombat = true;

    void Start()
    {
        // If the playerCombatant isn't set up in the Inspector, set default values
        if (playerCombatant == null)
        {
            playerCombatant = new Combatant(10, 10, 2, 0);
        }

        // Ensure that the enemy combatants are properly initialized
        for (int i = 0; i < enemyCombatants.Count; i++)
        {
            if (enemyCombatants[i] == null)
            {
                enemyCombatants[i] = new Combatant(2, 2, 1, 0); // Set default values if not set
            }
        }

        inCombat = true;
    }

    void Update()
    {
        if (inCombat)
        {
            TakeTurn();
        }
    }

    void TakeTurn()
    {
        float damageToPlayer = Mathf.Max(0f, enemyCombatants[0].Attack - playerCombatant.Defense);
        playerCombatant.TakeDamage(damageToPlayer);

        float damageToEnemy = Mathf.Max(0f, playerCombatant.Attack - enemyCombatants[0].Defense);
        enemyCombatants[currentEnemyIndex].TakeDamage(damageToEnemy);

        if (playerCombatant.Health <= 0f)
        {
            CombatOutcome(false);
            return;
        }

        if (enemyCombatants[currentEnemyIndex].Health <= 0f)
        {
            currentEnemyIndex++;
            if (currentEnemyIndex >= enemyCombatants.Count)
            {
                CombatOutcome(true);
            }
        }
    }

    void CombatOutcome(bool combatWon)
    {
        if (combatWon)
        {
            Debug.Log("Combat Won!");
        }
        else
        {
            Debug.Log("Combat Lost!");
        }
        inCombat = false;
    }


}
