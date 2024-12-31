using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Chase-Do Nothing", menuName = "Enemy Logic/Chase Logic/DoNothing Chase")]
public class EnemyChaseDoNothing : EnemyChaseSOBase
{
    private float _movementSpeed = 0f;
    [SerializeField] private float _coolDownToForget = 5.0f;
    [SerializeField] private float _rotationSpeed = 100.0f;
    private float _forgetCountDown = 0.0f;

    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        agent.updateRotation = false;
        agent.isStopped = false;
        agent.speed = _movementSpeed;
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        agent.updateRotation = true;
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

        FacePlayer();
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

    public void FacePlayer()
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            _rotationSpeed * Time.deltaTime
            );
    }
}
