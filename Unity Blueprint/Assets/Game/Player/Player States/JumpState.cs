using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : State<PlayerStateMachine>
{
    public override void EnterState(PlayerStateMachine owner)
    {

    }
    public override void UpdateState(PlayerStateMachine owner)
    {
        
        if (owner.move.MoveOnInput() && owner.move.isGrounded)
        {            
            owner.animator.SetBool("Move", true);
            owner.animator.Play("Move");
            owner.ChangeState<MoveState>();
            return;
        }

        if (owner.move.isGrounded)
        {
            owner.animator.Play("Idle");
            owner.ChangeState<IdleState>();
            return;
        }

        if (owner.rb.velocity.y < -1.0f && !owner.move.isGrounded)
        {            
            owner.animator.SetBool("Fall", true);
            owner.animator.Play("Fall");
            owner.ChangeState<FallState>();
            return;
        }

    }
    public override void ExitState(PlayerStateMachine owner)
    {
        owner.animator.SetBool("Jump", false);
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
