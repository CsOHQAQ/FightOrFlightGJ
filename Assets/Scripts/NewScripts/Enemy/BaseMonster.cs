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
        if (Health < 0f) 
        {
            Health = 0f;

        }
        Debug.Log("Monster current health: " + Health);
    }

    public void Die()
    {
        Debug.Log("MONSTER DIED");
    }

    public void OnHit(HitData hitData)
    {
        Debug.Log("Got Hit on " + hitData.HitInfo.HitPoint);
    }
}
