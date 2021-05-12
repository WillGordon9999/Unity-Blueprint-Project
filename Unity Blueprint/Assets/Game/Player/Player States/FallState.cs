using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallState : State<PlayerStateMachine>
{   
    public override void EnterState(PlayerStateMachine owner)
    {
        
    }
    public override void UpdateState(PlayerStateMachine owner)
    {
        bool movement = owner.move.MoveOnInput();

        if (owner.move.isGrounded && !movement)
        {
            owner.animator.SetBool("Move", false);
            owner.animator.Play("Move");
            owner.ChangeState<IdleState>();
            return;
        }

        if (owner.move.isGrounded && movement)
        {
            owner.animator.SetBool("Move", true);
            owner.animator.Play("Fall");
            owner.ChangeState<MoveState>();
            return;
        }
    }
    public override void ExitState(PlayerStateMachine owner)
    {
        owner.animator.SetBool("Fall", false);
    }
    public override void OnCollisionEnter(PlayerStateMachine owner, Collision collision)
    {

    }
    public override void OnCollisionStay(PlayerStateMachine owner, Collision collision)
    {

    }
    public override void OnCollisionExit(PlayerStateMachine owner, Collision collision)
    {

    }
    public override void OnTriggerEnter(PlayerStateMachine owner, Collider collider)
    {

    }
    public override void OnTriggerStay(PlayerStateMachine owner, Collider collider)
    {

    }
    public override void OnTriggerExit(PlayerStateMachine owner, Collider collider)
    {

    }
}
