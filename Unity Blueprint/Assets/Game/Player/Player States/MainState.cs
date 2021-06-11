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
        owner.move.FallCheck();
        owner.move.MoveOnInput();
        owner.move.JumpOnInput();
        owner.combat.AttackOnInput(100.0f);

        bool shift = Input.GetKey(KeyCode.LeftShift);
        WeaponInventory.WeaponSlot slot = shift ? WeaponInventory.WeaponSlot.Left : WeaponInventory.WeaponSlot.Right;
      
        owner.weaponInventory.SetScroll();

        if (Input.GetKeyDown(KeyCode.E))
            owner.weaponInventory.EquipWeaponScroll(WeaponInventory.WeaponSlot.Right);

        if (Input.GetKeyDown(KeyCode.Q))
            owner.weaponInventory.EquipWeaponScroll(WeaponInventory.WeaponSlot.Left);

        if (Input.GetKeyDown(KeyCode.Alpha1))
            owner.animator.SetFloat("MoveSet_ID", 0.0f);
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
            owner.animator.SetFloat("MoveSet_ID", 1.0f);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            owner.animator.SetFloat("MoveSet_ID", 2.0f);


        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Testing equip weapon");
            owner.weaponInventory.DrawWeaponScroll(WeaponInventory.WeaponSlot.Right);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            owner.animator.SetBool("IsBlocking", true);
            owner.animator.SetInteger("DefenseID", 3);
            owner.animator.SetBool("FlipAnimation", true);
        }

        if (Input.GetKeyUp(KeyCode.G))
            owner.animator.SetBool("IsBlocking", false);

        if (Input.GetMouseButtonDown(0))        
        {
            //owner.animator.SetInteger("AttackID", 1);
            owner.animator.SetTrigger("WeakAttack");            
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
