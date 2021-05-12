using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackState : State<EnemyController>
{
    bool canAttack;
    bool loseTimer;
    StateMachine<EnemyController> stateMachine;

    public override void EnterState(EnemyController owner)
    {
        canAttack = true;
        loseTimer = false;
        stateMachine = owner.stateMachine;
    }
    public override void UpdateState(EnemyController owner)
    {
        bool canSee = owner.sense.CheckVision();

        if (!canSee)
        {
            if (!loseTimer)
                owner.StartCoroutine(LoseTime(owner.timeUntilLost));
        }

        else
        {
            owner.StopCoroutine(LoseTime(owner.timeUntilLost));
            loseTimer = false;
        }
        
        owner.transform.position = Vector3.MoveTowards(owner.transform.position, owner.sense.lastSeenPos, owner.moveSpeed * Time.deltaTime);

        Vector3 dist = (owner.sense.lastSeenPos - owner.transform.position);
        Vector3 dir = dist.normalized;
        dir = new Vector3(dir.x, 0.0f, dir.z);

        owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, Quaternion.LookRotation(dir), owner.rotateSpeed);

        if (canSee && dist.magnitude <= owner.attackRange)
        {
            if (canAttack)
            {
                owner.Attack(owner.target, owner.attackForce);
                canAttack = false;
                owner.StartCoroutine(AttackCoolDown(owner.attackCooldown));
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

    IEnumerator AttackCoolDown(float time)
    {
        yield return new WaitForSeconds(time);
        canAttack = true;
    }

    IEnumerator LoseTime(float time)
    {
        yield return new WaitForSeconds(time);
        loseTimer = false;
        stateMachine.ChangeState<EnemyPatrolState>();
    }
   
}
