using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BaseMonster : MonoBehaviour, ICharacter, IHitReceiver,IRoomObject
{
    public event Action<ICharacter> OnCharacterDied;

    private float health = 100f; // Default health
    public float Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0f, MaxHealth);
        }
    }

    public float MaxHealth => 100f;


    public void AddHealth(float amount)
    {
        Health += amount;
    }

    public void TakeDamage(float amount)
    {
        Health -= amount;
        Debug.Log("Monster current health: " + Health);
        if (Health <= 0f) 
        {
            Health = 0f;
            
            Die();
        }
        
    }

    public void Die()
    {
        Debug.Log("MONSTER DIED");
        //TODO: Trigger Event Chain for death
        OnCharacterDied?.Invoke(this);
        //TODO: Play Death Animation and show corpse
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }

    public void OnHit(HitData hitData)
    {
        Debug.Log("Got Hit on " + hitData.HitInfo.HitPoint);
    }
    public AbilitySystemComponent GetAbilitySystemComponent()
    {
        return null;
    }

    public void OnCombatStartedInRoom(Room room)
    {
        //Get Activated and can start attacking plyer
    }
    public void OnCombatEndedInRoom(Room room)
    {

    }
}
