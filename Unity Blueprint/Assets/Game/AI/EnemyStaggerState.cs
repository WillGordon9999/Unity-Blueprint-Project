using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStaggerState : State<EnemyController>
{
    public override void EnterState(EnemyController owner)
    {

    }
    public override void UpdateState(EnemyController owner)
    {
        if (owner.canAct)
        {
            //bool canSee = owner.sense.CheckVision();

            //Placeholder for proper condition to check whether to investigate or not
            owner.stateMachine.ChangeState<EnemyAttackState>();
        }
    }
    public override void ExitState(EnemyController owner)
    {

    }
    public override void OnCollisionEnter(EnemyController owner, Collision collision)
    {

    }
    public override void OnCollisionStay(EnemyController owner, Collision collision)
    {

    }
    public override void OnCollisionExit(EnemyController owner, Collision collision)
    {

    }
    public override void OnTriggerEnter(EnemyController owner, Collider collider)
    {

    }
    public override void OnTriggerStay(EnemyController owner, Collider collider)
    {

    }
    public override void OnTriggerExit(EnemyController owner, Collider collider)
    {

    }
}
