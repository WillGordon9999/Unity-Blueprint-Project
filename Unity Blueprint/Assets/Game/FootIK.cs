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

//NOT CURRENTLY IN USE
public class FootIK : MonoBehaviour
{
    Transform LeftFoot;
    Transform RightFoot;
    DitzelGames.FastIK.FastIKFabric LeftFootIK;
    DitzelGames.FastIK.FastIKFabric RightFootIK;
    Rigidbody rb;
    Animator anim;
    Game.Movement move;
    // Start is called before the first frame update
    void Start()
    {
        LeftFoot = Player.Instance.LeftFootTarget;
        RightFoot = Player.Instance.RightFootTarget;
        LeftFootIK = Player.Instance.LeftFootIK;
        RightFootIK = Player.Instance.RightFootIK;

        LeftFootIK.manualSet = true;
        RightFootIK.manualSet = true;

        rb = Player.Instance.rb;
        anim = GetComponent<Animator>();
        move = rb.GetComponent<Game.Movement>();
        StartCoroutine(Loop());
    }
    
    // Update is called once per frame
    void LateUpdate()
    {
        return;
        //Only shoot the raycasts when you're actually on the ground!
        //if (move.isGrounded)
        {

            LeftFoot.transform.SetPositionAndRotation(LeftFootIK.transform.position, LeftFootIK.transform.rotation);
            RightFoot.transform.SetPositionAndRotation(RightFootIK.transform.position, RightFootIK.transform.rotation);

            Ray ray = new Ray(LeftFoot.position + Vector3.up * 0.5f, Vector3.down);
            RaycastHit hit;
            
            //if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f, LayerMask.NameToLayer("Ground")))
            if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f))
            {
                Debug.Log($"Hit {hit.collider.gameObject.name}");
                LeftFoot.position = hit.point + Vector3.up * 0.05f;
                LeftFootIK.ManualApplyCalculations();
            }

            ray = new Ray(RightFoot.position + Vector3.up * 0.5f, Vector3.down);

            //if (Physics.SphereCast(ray, 0.05f, out hit, 5.0f, LayerMask.NameToLayer("Ground")))
            if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f))
            {
                Debug.Log($"Hit {hit.collider.gameObject.name}");
                RightFoot.position = hit.point + Vector3.up * 0.05f;
                RightFootIK.ManualApplyCalculations();
            }
        }
    }

    IEnumerator Loop()
    {
        while(true)
        {
            //yield return new WaitForEndOfFrame();
            yield return null;

            if (move.isGrounded)
            {
                //LeftFoot.transform.SetPositionAndRotation(LeftFootIK.transform.position, LeftFootIK.transform.rotation);
                //RightFoot.transform.SetPositionAndRotation(RightFootIK.transform.position, RightFootIK.transform.rotation);
                LeftFoot.position = LeftFootIK.transform.position;
                RightFoot.position = RightFootIK.transform.position;

                Ray ray = new Ray(LeftFoot.position + Vector3.up * 0.5f, Vector3.down);
                RaycastHit hit;

                //if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f, LayerMask.NameToLayer("Ground")))
                if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f))
                {
                    Debug.Log($"Hit {hit.collider.gameObject.name}");
                    LeftFoot.position = hit.point + Vector3.up * 0.05f;
                    //LeftFootIK.ManualApplyCalculations();
                }

                ray = new Ray(RightFoot.position + Vector3.up * 0.5f, Vector3.down);

                //if (Physics.SphereCast(ray, 0.05f, out hit, 5.0f, LayerMask.NameToLayer("Ground")))
                if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f))
                {
                    Debug.Log($"Hit {hit.collider.gameObject.name}");
                    RightFoot.position = hit.point + Vector3.up * 0.05f;
                    //RightFootIK.ManualApplyCalculations();
                }
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
