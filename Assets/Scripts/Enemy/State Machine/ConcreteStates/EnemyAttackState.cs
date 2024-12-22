using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private Transform _playerTransform;

    private float _timer;
    private float _timeBetweenShots = 2.0f;

    private float _exitTimer;
    private float _timeTillExit = 3.0f;
    private float _distanceToCountExit = 3.0f;

    private float _flameBlastSpeed = 10f;


    public EnemyAttackState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override void AnimationTriggerEvent(Enemy.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }

    public override void EnterState()
    {
        base.EnterState();
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        enemy.MoveEnemy(Vector3.zero);

        if (_timer > _timeBetweenShots)
        {
            _timer = 0f;

            Vector3 direction = (_playerTransform.position - enemy.transform.position).normalized;

            // Should have like an Object Pool System to avoid this
            Rigidbody flameBlast = GameObject.Instantiate(enemy.FireBlastPrefab, enemy.transform.position, Quaternion.identity);
            flameBlast.velocity = direction * _flameBlastSpeed;
        }

        // Better implementation for the future this is just for testing
        if (Vector3.Distance(_playerTransform.position, enemy.transform.position) > _distanceToCountExit)
        {
            _exitTimer += Time.deltaTime;

            if (_exitTimer > _timeTillExit)
            {
                enemy.StateMachine.ChangeState(enemy.ChaseState);
            }
        }
        else
        {
            _exitTimer = 0f;
        }

        _timer += Time.deltaTime;
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
