using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Chase-Eye Monster", menuName = "Enemy Logic/Chase Logic/Eye Monster Chase")]
public class EnemyChase_EyeMonster : EnemyChaseSOBase
{
    private float _movementSpeed = 0f;
    [SerializeField] private float _coolDownToForget = 5.0f;
    [SerializeField] private float _rotationSpeed = 100.0f;
    private float _forgetCountDown = 0.0f;

    private EyeMonster eyeMonster;

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
        
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();


        _forgetCountDown = 0.0f;
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();
        eyeMonster.TrackGameObject(playerTransform);

        //FacePlayer();
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
