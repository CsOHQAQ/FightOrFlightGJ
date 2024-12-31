using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[CreateAssetMenu(fileName = "Attack-Straight-Single FireBlast", menuName = "Enemy Logic/Attack Logic/Straight Single FireBlast")]
public class EnemyAttackSingleStraightFireBlast : EnemyAttackSOBase
{
    [SerializeField] private Rigidbody FireBlastPrefab;
    [SerializeField] private float _timeBetweenShots = 2.0f;
    [SerializeField] private float _timeTillExit = 3.0f;
    [SerializeField] private float _distanceToCountExit = 3.0f;
    [SerializeField] private float _flameBlastSpeed = 10f;
    [SerializeField] private float _rotationSpeed = 100.0f;

    private float _timer;
    private float _exitTimer;

    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        agent.updateRotation = false;
        agent.isStopped = true;
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        agent.updateRotation = true;
        agent.isStopped = false;
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

        if (_timer > _timeBetweenShots)
        {
            _timer = 0f;

            RotateEnemy(true);

            Vector3 direction = (playerTransform.position - enemy.transform.position).normalized;

            // Should have like an Object Pool System to avoid this
            Rigidbody flameBlast = GameObject.Instantiate(FireBlastPrefab, enemy.transform.position, Quaternion.identity);
            flameBlast.velocity = direction * _flameBlastSpeed;
        }
        else
        {
            RotateEnemy(false);
        }

        // Better implementation for the future this is just for testing
        if (Vector3.Distance(playerTransform.position, enemy.transform.position) > _distanceToCountExit)
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

    public void RotateEnemy(bool isShooting)
    {
        Vector3 direction;

        if (isShooting)
        {
            direction = (playerTransform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            if (agent.velocity.sqrMagnitude > 0.01f)
            {
                direction = new Vector3(agent.velocity.x, 0f, agent.velocity.z).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
            else
            {
                return;
            }
        }


    }
}
