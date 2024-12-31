using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Idle-Random Wander", menuName = "Enemy Logic/Idle Logic/Random Wander")]
public class EnemyIdleRandomWander : EnemyIdleSOBase
{
    [SerializeField] private float wanderRadius = 3.0f;
    [SerializeField] private float wanderDistance = 3.0f;
    [SerializeField] private float wanderJitter = 1f;
    [SerializeField] private float _enemySpeed = 1.0f;

    private Vector3 _targetPosition = Vector3.zero;
    private Vector3 _direction;

    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        _direction = gameObject.transform.position;
        agent.speed = _enemySpeed;
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

        if (agent.hasPath && !HasReachedDestination()) { return; }
        _direction = GetRandomTarget();
        agent.SetDestination(_direction);
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

    private Vector3 GetRandomTarget()
    {
        _targetPosition += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter,
            0,
            Random.Range(-1.0f, 1.0f) * wanderJitter);

        _targetPosition.Normalize();
        _targetPosition *= wanderRadius;

        Vector3 targetLocal = _targetPosition + new Vector3(0, 0, wanderDistance);
        Vector3 targetWorld = gameObject.transform.TransformPoint(targetLocal);

        targetWorld = CheckToNavMesh(targetWorld);

        return targetWorld;
    }

    private Vector3 CheckToNavMesh(Vector3 position)
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(position, out hit, wanderDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        Vector3 directionToCenter = (transform.position - position).normalized;
        return transform.position + directionToCenter * 1.0f;
    }

    private bool HasReachedDestination()
    {
        return agent.remainingDistance <= agent.stoppingDistance;
    }
}
