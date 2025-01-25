using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Chase-Direct Chase", menuName = "Enemy Logic/Chase Logic/Direct Chase")]
public class EnemyChaseDirectToPlayer : EnemyChaseSOBase
{
    [SerializeField] private float _movementSpeedMultiplier = 1f;
    [SerializeField] private float _coolDownToForget = 7.5f;
    private float _forgetCountDown = 0.0f;

    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        agent.isStopped = false;
        agent.speed = enemy.Speed * _movementSpeedMultiplier;
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        agent.isStopped = true;
        _forgetCountDown = 0.0f;
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

        if (!enemy.IsAggroed)
        {
            _forgetCountDown += Time.deltaTime;
        }
        else
        {
            _forgetCountDown = 0.0f;
        }

        if (!enemy.IsAggroed && _forgetCountDown >= _coolDownToForget)
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
        }

        agent.SetDestination(playerTransform.position);
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
}
