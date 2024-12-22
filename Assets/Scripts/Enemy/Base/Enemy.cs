using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable, IEnemyMoveable
{
    [field: SerializeField] public float MaxHealth { get; set; }
    public float CurrentHealth { get; set; }
    public Rigidbody RB { get; set; }
    [field: SerializeField] public float RotationSpeed { get; set; } = 5.0f;

    private void Start()
    {
        CurrentHealth = MaxHealth;

        RB = GetComponent<Rigidbody>();
    }

    #region Health/ / Die Functions

    public void Damage(float damageAmount)
    {
        CurrentHealth -= damageAmount;

        if (CurrentHealth <= damageAmount)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    #endregion


    #region Movement Functions

    public void MoveEnemy(Vector3 velocity)
    {
        RB.velocity = velocity;
    }

    public void RotateEnemy(Vector3 velocity)
    {

        if (velocity.sqrMagnitude > 0.01f)
        {
            Vector3 dirction = velocity.normalized;

            Quaternion targetRotation = Quaternion.LookRotation(dirction);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
        
    }

    #endregion

    #region Animation Triggers

    private void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {
        //TODO: fill in once Statemachine is created
    }

    public enum AnimationTriggerType
    {
        EnemyDamaged,
        PlayFootstepSound
    }

    #endregion
}
