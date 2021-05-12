using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State<PlayerStateMachine>
{
   
    public override void EnterState(PlayerStateMachine owner)
    {
        
    }
    public override void UpdateState(PlayerStateMachine owner)
    {
        //Falling - Implement a better way to check if grounded or not later
        if (!owner.move.isGrounded && owner.rb.velocity.y < -1.0f)        
        {
            owner.animator.SetBool("Fall", true);
            owner.animator.Play("Fall");
            owner.ChangeState<FallState>();
            return;
        }

        if (owner.move.MoveOnInput() && owner.move.isGrounded)
        {
            owner.animator.SetBool("Move", true);
            owner.animator.Play("Move");
            owner.ChangeState<MoveState>();
            return;
        }

        if (owner.move.JumpOnInput())
        {
            owner.animator.SetBool("Jump", true);
            owner.animator.Play("Jump");
            owner.ChangeState<JumpState>();
            return;
        }
    }
    public override void ExitState(PlayerStateMachine owner)
    {

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
