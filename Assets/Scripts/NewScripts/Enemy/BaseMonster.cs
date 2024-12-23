using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMonster : MonoBehaviour, ICharacter, IHitReceiver
{
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
        //TODO: Play Death Animation and show corpse
        Destroy(gameObject);
    }

    public void OnHit(HitData hitData)
    {
        Debug.Log("Got Hit on " + hitData.HitInfo.HitPoint);
    }
    public AbilitySystemComponent GetAbilitySystemComponent()
    {
        return null;
    }
}
