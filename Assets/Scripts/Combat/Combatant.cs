using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class Combatant
{
    
    [SerializeField]
    private float health;
    [SerializeField]
    private float maxHealth;
    [SerializeField]
    private float attack;
    [SerializeField]
    private float defense;

    
    public float Health { get { return health; } }
    public float MaxHealth { get { return maxHealth; } }
    public float Attack { get { return attack; } }
    public float Defense { get { return defense; } }

    public Combatant() { }

    public Combatant(float health, float maxHealth, float attack, float defense)
    {
        this.health = health;
        this.maxHealth = maxHealth;
        this.attack = attack;
        this.defense = defense;
    }

    public void InitializeCombatant(float maxHealth, float health, float attack, float defense)
    {
        this.maxHealth = maxHealth;
        this.health = health;
        this.attack = attack;
        this.defense = defense;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Max(health, 0);
    }
}
