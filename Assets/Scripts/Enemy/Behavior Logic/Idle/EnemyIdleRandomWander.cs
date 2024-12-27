using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle-Random Wander", menuName = "Enemy Logic/Idle Logic/Random Wander")]
public class EnemyIdleRandomWander : EnemyIdleSOBase
{
    [SerializeField] private float RandomMovementRange = 5f;
    [SerializeField] private float RandomMovementSpeed = 1f;

    private Vector3 _targetPosition;
    private Vector3 _direction;

    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();

        _targetPosition = GetRandomPointInSphere();
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

        _direction = (_targetPosition - enemy.transform.position).normalized;

        _direction.y = 0;

        enemy.MoveEnemy(_direction * RandomMovementSpeed);

        // Check if the enemy is close to the target point (ignoring Y)
        Vector3 flatPosition = new Vector3(enemy.transform.position.x, 0, enemy.transform.position.z);
        Vector3 flatTarget = new Vector3(_targetPosition.x, 0, _targetPosition.z);

        if ((flatPosition - flatTarget).sqrMagnitude < 0.01f)
        {
            // Assign a new target position when the enemy reaches the current one
            _targetPosition = GetRandomPointInSphere();
        }
    }

    public override void DoPhysicsLogic()
    {
        base.DoPhysicsLogic();
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    private Vector3 GetRandomPointInSphere()
    {
        Vector3 randomOffSet = Random.insideUnitSphere * RandomMovementRange;

        randomOffSet.y = 0;

        return enemy.transform.position + randomOffSet;
    }
}
