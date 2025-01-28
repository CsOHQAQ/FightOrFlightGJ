using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[CreateAssetMenu(fileName = "Attack-Eye Monster FireBlast", menuName = "Enemy Logic/Attack Logic/Eye Monster FireBlast")]
public class EnemyAttackEyeMonsterAttack : EnemyAttackSOBase
{
    [SerializeField] private Rigidbody FireBlastPrefab;
    [SerializeField] private float _timeBetweenShots = 2.0f;
    [SerializeField] private float _timeTillExit = 3.0f;
    [SerializeField] private float _distanceToCountExit = 3.0f;
    [SerializeField] private float _flameBlastSpeed = 10f;
    [SerializeField] private float _rotationSpeed = 100.0f;
    private EyeMonster eyeMonster;
    private float _timer;
    private float _exitTimer;

    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void DoEnterLogic()
    {
        
        base.DoEnterLogic();
        eyeMonster = enemy as EyeMonster;
        if (eyeMonster == null)
        {
            Debug.LogError("Wrong Script for Monster Type");
        }
        eyeMonster.StartAttackWindUp();
        eyeMonster.OnEyeFullyClosed+=OnEyeFullyClosed;
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        eyeMonster.OnEyeFullyClosed-=OnEyeFullyClosed;
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();
        eyeMonster.TrackGameObject(playerTransform);


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



    }
    private void OnEyeFullyClosed()
    {
        enemy.StateMachine.ChangeState(enemy.ChaseState);
    }
}
