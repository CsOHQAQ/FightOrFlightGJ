using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "Chase-Run Away", menuName = "Enemy Logic/Chase Logic/Run Away")]
public class EnemyChaseRunAway : EnemyChaseSOBase
{
    [SerializeField] private float _runAwaySpeed = 1.5f;
    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

        Vector3 runDirection = -(playerTransform.position - enemy.transform.position).normalized;
        runDirection.y = 0f;

        enemy.MoveEnemy(runDirection * _runAwaySpeed);
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
