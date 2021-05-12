using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainState : State<PlayerStateMachine>
{
    public override void EnterState(PlayerStateMachine owner)
    {

    }
    public override void UpdateState(PlayerStateMachine owner)
    {
        owner.move.MoveOnInput();
        owner.move.FallCheck();
        owner.move.JumpOnInput();
        owner.combat.AttackOnInput(100.0f);
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
