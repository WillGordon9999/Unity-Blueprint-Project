using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolState : State<EnemyController>
{
    GameObject patrolTarget = null;
    int patrolIndex = 0;
    bool visionTimer = false;
    StateMachine<EnemyController> stateMachine = null;

    public override void EnterState(EnemyController owner)
    {
        if (stateMachine == null)
            stateMachine = owner.stateMachine;

        if (patrolTarget == null)
        {
            if (owner.patrolPoints.Count > 0)                            
                patrolTarget = owner.patrolPoints[patrolIndex];
            
        }
    }
    public override void UpdateState(EnemyController owner)
    {
        if (owner.sense.CheckVision())
        {
            if (!visionTimer)
                owner.StartCoroutine(CheckSight(owner.timeUntilAlert));
        }

        else
        {
            if (visionTimer)
            {
                owner.StopCoroutine(CheckSight(owner.timeUntilAlert));
                visionTimer = false;
            }
        }

        if (patrolTarget != null)
        {
            owner.transform.position = Vector3.MoveTowards(owner.transform.position, patrolTarget.transform.position, owner.moveSpeed * Time.deltaTime);
            Vector3 dir = (patrolTarget.transform.position - owner.transform.position).normalized;
            dir = new Vector3(dir.x, 0.0f, dir.z);
            owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, Quaternion.LookRotation(dir), owner.rotateSpeed);

            if (Vector3.Distance(owner.transform.position, patrolTarget.transform.position) < 1.0f)
            {
                patrolIndex++;

                if (patrolIndex >= owner.patrolPoints.Count)
                    patrolIndex = 0;

                patrolTarget = owner.patrolPoints[patrolIndex];

            }

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

    IEnumerator CheckSight(float time)
    {
        visionTimer = true;
        yield return new WaitForSeconds(time);
        visionTimer = false;
        stateMachine.ChangeState<EnemyAttackState>();
    }
}
