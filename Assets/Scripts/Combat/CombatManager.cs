using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System; // Add this to use Odin attributes

public class CombatManager:MonoBehaviour
{
    private PlayerStats playerCombatant;

    [SerializeField]
    private GameObject CombatUIPrefab;
    private CombatUI combatUI;

    [SerializeField]
    private List<MonsterStats> enemyCombatants = new List<MonsterStats>();

    [SerializeField]
    private float turnDuration = 2.0f; // Duration of each turn in seconds

    private float turnTimer = 0f; // Timer to track time between turns
    private int currentEnemyIndex = 0;
    private bool inCombat = false;
    private Action<bool> combatResultCallBack;

    public void StartBattle(PlayerStats player, List<MonsterStats> monsters,Action<bool> callback)
    {
        if (player == null || monsters == null)
        {
            if (player == null)
                Debug.LogError("Player Combatant null!!");

            if (monsters== null)
                Debug.LogError("Monster Combatant null!!");
        }
        playerCombatant = player;
        enemyCombatants.Clear();
        for (int i = 0; i < monsters.Count; i++)
        {
            enemyCombatants.Add(monsters[i]);
        }
        inCombat = true;
        combatResultCallBack = callback;
        combatUI=Instantiate(CombatUIPrefab, GameObject.Find("Canvas").transform).GetComponent<CombatUI>();
        combatUI.OnOpen(player,monsters);
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
        
        //Debug.Log("Starting a new turn...");

        combatUI.Attack();
        // Calculate and log damage to the player. You deal at least 1 damage each turn. 
        int damageToPlayer = Mathf.Max(1, enemyCombatants[currentEnemyIndex].Attack - playerCombatant.Defense);
        
        //Reduce durabiity, randomly selects an armor and reduces its durability
        List<Equipment_ScriptableObject> armors = new List<Equipment_ScriptableObject>();
        foreach (var equip in InventoryManager.Instance.GetEquipment())
        {
            if (equip != null)
            {
                if (equip.itemSlot != ITEM_SLOT.WEAPON)
                {
                    armors.Add(equip);
                }
            }
        }
        if (armors.Count > 0)
        {
            Randomer rnd = new Randomer();
            armors[rnd.nextInt(armors.Count)].itemDurability -= 1;
        }

        //Debug.Log($"Enemy {currentEnemyIndex} attacks Player: Damage = {damageToPlayer}");
        playerCombatant.TakeDamage(damageToPlayer);
        
        //Debug.Log($"Player health after taking damage: {playerCombatant.CurHealth}");

        // Calculate and log damage to the enemy
        int damageToEnemy = Mathf.Max(1, playerCombatant.Attack - enemyCombatants[currentEnemyIndex].Defense);
        //Debug.Log($"Player attacks Enemy {currentEnemyIndex}: Damage = {damageToEnemy}");
        
        //Reduce weapon durability
        Equipment_ScriptableObject weapon = null;
        foreach (var equip in InventoryManager.Instance.GetEquipment())
        {
            if (equip != null)
            {
                if (equip.itemSlot == ITEM_SLOT.WEAPON)
                {
                    weapon=equip;
                }
            }
        }
        if (weapon!=null)
        {
            weapon.itemDurability -= 1;
        }

        enemyCombatants[currentEnemyIndex].TakeDamage(damageToEnemy);
        //Debug.Log($"Enemy {currentEnemyIndex} health after taking damage: {enemyCombatants[currentEnemyIndex].CurHealth}");

        // Check if the player is dead and log it
        if (playerCombatant.CurHealth <= 0f)
        {
            //Debug.Log("Player has died.");
            CombatOutcome(false);
            return;
        }

        // Check if the current enemy is dead and log it
        if (enemyCombatants[currentEnemyIndex].CurHealth <= 0)
        {
            //Debug.Log($"Enemy {currentEnemyIndex} has died.");
            currentEnemyIndex++;
            if (currentEnemyIndex < enemyCombatants.Count)
            {
                combatUI.monsterBuffer = enemyCombatants[currentEnemyIndex];
            }
                


            // Check if all enemies are dead and log the combat outcome
            if (currentEnemyIndex >= enemyCombatants.Count)
            {
                CombatOutcome(true);
            }
            else
            {
                //Debug.Log($"Next enemy is now Enemy {currentEnemyIndex}");
            }
        }
        else
        {
            //Debug.Log($"Enemy {currentEnemyIndex} is still alive.");
        }
    }

    void CombatOutcome(bool combatWon)
    {
        foreach(var equip in InventoryManager.Instance.GetEquipment())
        {
            if (equip != null)
            {
                Debug.Log($"{equip.name}'s durability remains {equip.itemDurability}");
            }
        }

        if (combatWon)
        {
            combatUI.ChangeText($"Combat Won!");
            //Debug.Log($"Combat Won! All enemies have been defeated. Player's remaining health: {playerCombatant.CurHealth}");
        }
        else
        {
            combatUI.ChangeText($"Combat Lost!");            
            //Debug.Log("Combat Lost! The player has died.");
        }
        combatUI.SelfClose(3f);
        combatUI = null;
        combatResultCallBack(combatWon);
        inCombat = false;
    }
}
