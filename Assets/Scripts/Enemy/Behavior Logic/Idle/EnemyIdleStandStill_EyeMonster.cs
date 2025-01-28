using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle-Stand Still - Eye Monster", menuName = "Enemy Logic/Idle Logic/Stand Still - Eye Monster")]
public class EnemyIdleStandStill_EyeMonster : EnemyIdleSOBase
{
    private EyeMonster eyeMonster;
    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void DoEnterLogic()
    {
        
        base.DoEnterLogic();
        if (agent!=null)
        {
            agent.isStopped = true;
        }
        eyeMonster = enemy as EyeMonster;
        if (eyeMonster == null)
        {
            Debug.LogError("Wrong Script for Monster Type");
            return;
        }
        eyeMonster.OnEyeFullyOpened+=OnEyeFullyOpened;
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        if (agent!=null)
        {
            agent.isStopped = false;
        }
        eyeMonster.OnEyeFullyOpened-=OnEyeFullyOpened;
    }

    public override void DoFrameUpdateLogic()
    {
        //base.DoFrameUpdateLogic();
        //Opens it's eye and then enter chase state. 
        if (enemy.IsAggroed)
        {
            
            eyeMonster.OpenEye();
            
            //Debug.LogWarning("?????????????");
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
    
    private void OnEyeFullyOpened()
    {
        //Debug.LogWarning("?????????????");
        enemy.StateMachine.ChangeState(enemy.ChaseState);
    }
}
