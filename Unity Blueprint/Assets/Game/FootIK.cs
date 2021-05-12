using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IKFrame
{
    public Vector3 LeftTargetPos;
    public Vector3 LeftTargetEuler;
    public Vector3 RightTargetPos;
    public Vector3 RightTargetEuler;
}


public class FootIK : MonoBehaviour
{
    Transform LeftFoot;
    Transform RightFoot;
    Rigidbody rb;
    Animator anim;
    Game.Movement move;
    // Start is called before the first frame update
    void Start()
    {
        LeftFoot = Player.Instance.LeftFootTarget;
        RightFoot = Player.Instance.RightFootTarget;
        rb = Player.Instance.rb;
        anim = GetComponent<Animator>();
        move = rb.GetComponent<Game.Movement>();
    }
    
    // Update is called once per frame
    void LateUpdate()
    {
        //Only shoot the raycasts when you're actually on the ground!
        if (move.isGrounded)
        {
            print("Running foot IK grounded");
            Ray ray = new Ray(LeftFoot.position + Vector3.up * 0.5f, Vector3.down);
            RaycastHit hit;

            //if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f, LayerMask.NameToLayer("Ground")))
            if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f))
            {
                //Debug.Log($"Hit {hit.collider.gameObject.name}");
                LeftFoot.position = hit.point + Vector3.up * 0.05f;
            }

            ray = new Ray(RightFoot.position + Vector3.up * 0.5f, Vector3.down);

            //if (Physics.SphereCast(ray, 0.05f, out hit, 5.0f, LayerMask.NameToLayer("Ground")))
            if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f))
            {
                //Debug.Log($"Hit {hit.collider.gameObject.name}");
                RightFoot.position = hit.point + Vector3.up * 0.05f;
            }
        }
    }

    public void AnimationSetIK(int val)
    {        
        print("Event receive");

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Ground Stand Attack"))
        {
            print("Confirmed Ground Stand Attack");
            if (val == 0)
                Player.Instance.SetFootIK(false);
            else
                Player.Instance.SetFootIK(true);
        }
    }
    
    public void Test()
    {

    }
}
