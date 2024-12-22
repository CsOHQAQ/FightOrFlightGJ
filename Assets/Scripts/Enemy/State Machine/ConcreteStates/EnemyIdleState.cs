using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private Vector3 _targetPosition;
    private Vector3 _direction;

    public EnemyIdleState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {

    }

    public override void AnimationTriggerEvent(Enemy.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }

    public override void EnterState()
    {
        base.EnterState();

        _targetPosition = GetRandomPointInSphere();   

    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        if (enemy.IsAggroed)
        {
            enemy.StateMachine.ChangeState(enemy.ChaseState);
        }

        _direction = (_targetPosition - enemy.transform.position).normalized;

        _direction.y = 0;

        enemy.MoveEnemy(_direction * enemy.RandomMovementSpeed);

        // Check if the enemy is close to the target point (ignoring Y)
        Vector3 flatPosition = new Vector3(enemy.transform.position.x, 0, enemy.transform.position.z);
        Vector3 flatTarget = new Vector3(_targetPosition.x, 0, _targetPosition.z);

        if ((flatPosition - flatTarget).sqrMagnitude < 0.01f)
        {
            // Assign a new target position when the enemy reaches the current one
            _targetPosition = GetRandomPointInSphere();
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    private Vector3 GetRandomPointInSphere()
    {
        Vector3 randomOffSet = Random.insideUnitSphere * enemy.RandomMovementRange;

        randomOffSet.y = 0;

        return enemy.transform.position + randomOffSet;
    }
}
