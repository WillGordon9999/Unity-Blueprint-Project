using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKControl : MonoBehaviour
{
    protected Animator animator;
    public bool ikActive = false;

    public float rightFootWeight = 0.5f;
    public float rightFootRotWeight = 0.0f;
    public float leftFootWeight = 0.5f;
    public float leftFootRotWeight = 0.0f;

    [HideInInspector] public Transform rightHandObj = null;
    [HideInInspector] public Transform lookObj = null;

    Transform leftFoot, leftFootTarget, rightFoot, rightFootTarget;
    
    Game.Movement move;

    //FilmStorm Stuff - Source: https://www.youtube.com/watch?v=MonxKdgxi2w    
  
    //[Header("Debug")]
    Vector3 newPelvisPos;
    Vector3 animBodyPos;
    float pelvisYDiff;
    float lastPelvisPosY, lastRightFootPosY, lastLeftFootPosY;

    //These sometimes need to be debugged
    Vector3 rightFootPos, leftFootPos, ikRightFootPos, ikLeftFootPos;
    Quaternion ikLeftFootRotation, ikRightFootRotation;

    GameObject rightFootPosObj, leftFootPosObj, ikRightFootPosObj, ikLeftFootPosObj;


    [Range(0, 2)] public float heightFromGroundRaycast = 1.14f;
    [Range(0, 2)] public float raycastDownDist = 1.5f;
    public LayerMask environmentLayer; //Will use questionably
    public float pelvisOffset = 0f;
    public float pelvisYSpeed = 0.28f;
    public bool usePelvisMove = false;
    public float ikFeetPositionSpeed = 0.5f;

    public string leftFootAnimVariableName = "LeftFootCurve";
    public string rightFootAnimVariableName = "RightFootCurve";

    public bool useProIkFeature = false;
    public bool showSolverDebug = true;
    public bool showDebugObjects = false;



    void Start()
    {
        animator = GetComponent<Animator>();
        move = GetComponent<Game.Movement>();
        leftFoot = Player.Instance.LeftFootIK.transform;
        rightFoot = Player.Instance.RightFootIK.transform;
        leftFootTarget = Player.Instance.LeftFootTarget;
        rightFootTarget = Player.Instance.RightFootTarget;

        if (showDebugObjects)
        {
            rightFootPosObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightFootPosObj.transform.localScale = Vector3.one * 0.25f;
            rightFootPosObj.GetComponent<Renderer>().material.color = Color.green;
            Destroy(rightFootPosObj.GetComponent<SphereCollider>());
        
            leftFootPosObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftFootPosObj.transform.localScale = Vector3.one * 0.25f;
            leftFootPosObj.GetComponent<Renderer>().material.color = Color.green;
            Destroy(leftFootPosObj.GetComponent<SphereCollider>());
        
            ikRightFootPosObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ikRightFootPosObj.transform.localScale = Vector3.one * 0.25f;
            ikRightFootPosObj.GetComponent<Renderer>().material.color = Color.blue;
            Destroy(ikRightFootPosObj.GetComponent<SphereCollider>());
        
            ikLeftFootPosObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ikLeftFootPosObj.transform.localScale = Vector3.one * 0.25f;
            ikLeftFootPosObj.GetComponent<Renderer>().material.color = Color.blue;
            Destroy(ikLeftFootPosObj.GetComponent<SphereCollider>());
        
        }

    }

    private void FixedUpdate()
    {
        if (animator == null || !ikActive) return;

        //Get the Raycasting position
        AdjustFeetTarget(ref rightFootPos, HumanBodyBones.RightFoot);
        AdjustFeetTarget(ref leftFootPos, HumanBodyBones.LeftFoot);

        if (showDebugObjects)
        {
            rightFootPosObj.transform.position = rightFootPos;
            leftFootPosObj.transform.position = leftFootPos;
        }

        //Shoot actual raycasts here
        FeetPositionSolver(rightFootPos, ref ikRightFootPos, ref ikRightFootRotation);
        FeetPositionSolver(leftFootPos, ref ikLeftFootPos, ref ikLeftFootRotation);

        if (showDebugObjects)
        {
            ikRightFootPosObj.transform.position = ikRightFootPos;
            ikLeftFootPosObj.transform.position = ikLeftFootPos;
        }

    }

    //a callback for calculating IK
    void OnAnimatorIK(int layerIndex)
    {
        //if (animator == null || !ikActive || !move.isGrounded) return;
        if (animator == null || !ikActive) return;

        if (usePelvisMove)
            MovePelvisHeight();

        //Right
        //animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootWeight);


        if (useProIkFeature)
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat(rightFootAnimVariableName));

        MoveFeetToIKPoint(AvatarIKGoal.RightFoot, ikRightFootPos, ikRightFootRotation, ref lastRightFootPosY);

        //Left
        //animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootWeight);

        if (useProIkFeature)
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat(leftFootAnimVariableName));

        MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, ikLeftFootPos, ikLeftFootRotation, ref lastLeftFootPosY);

    }

    void MoveFeetToIKPoint(AvatarIKGoal foot, Vector3 posIKHolder, Quaternion rotIKHolder, ref float lastFootPosY)
    {
        Vector3 targetIKPos = animator.GetIKPosition(foot);

        if (posIKHolder != Vector3.zero)
        {
            targetIKPos = transform.InverseTransformPoint(targetIKPos);
            posIKHolder = transform.InverseTransformPoint(posIKHolder);

            float y = Mathf.Lerp(lastFootPosY, posIKHolder.y, ikFeetPositionSpeed);
            targetIKPos.y += y;

            lastFootPosY = y;

            targetIKPos = transform.TransformPoint(targetIKPos);

            animator.SetIKRotation(foot, rotIKHolder);
        }

        animator.SetIKPosition(foot, targetIKPos);
    }

    void MovePelvisHeight()
    {        
        //From testing this will only fire once, on start up
        if (lastPelvisPosY == 0.0f)
        {
            lastPelvisPosY = animator.bodyPosition.y;
            animBodyPos = animator.bodyPosition;
            return;
        }

        if (ikRightFootPos == Vector3.zero || ikLeftFootPos == Vector3.zero || lastPelvisPosY == 0.0f)
        {
            pelvisYDiff = animator.bodyPosition.y - lastPelvisPosY;
            lastPelvisPosY = animator.bodyPosition.y;            
            animBodyPos = animator.bodyPosition;
            return;
        }

        float leftOffset = ikLeftFootPos.y - transform.position.y;
        float rightOffset = ikRightFootPos.y - transform.position.y;

        float totalOffset = (leftOffset < rightOffset) ? leftOffset : rightOffset;

        //newPelvisPos = animator.bodyPosition + Vector3.up * totalOffset;
        newPelvisPos = animator.bodyPosition + transform.TransformDirection(Vector3.up) * totalOffset;

        newPelvisPos.y = Mathf.Lerp(lastPelvisPosY, newPelvisPos.y, pelvisYSpeed);

        pelvisYDiff = newPelvisPos.y - animator.bodyPosition.y;

        //Original
        print("Applying Pelvis");
        animator.bodyPosition = newPelvisPos;
        animBodyPos = animator.bodyPosition;
        lastPelvisPosY = animator.bodyPosition.y;        
    }

    void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIKPos, ref Quaternion feetIKRot)
    {
        RaycastHit hit;

        if (showSolverDebug)
            //Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDist + heightFromGroundRaycast), Color.yellow);
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + transform.TransformDirection(Vector3.down) * (raycastDownDist + heightFromGroundRaycast), Color.yellow);

        //Can I do this without layers? I could use RaycastAll and iterate but that might be a little too costly
        //It may just depend on what is needed for the player
        //if (Physics.Raycast(fromSkyPosition, Vector3.down, out hit, raycastDownDist + heightFromGroundRaycast, environmentLayer))        
        if (Physics.Raycast(fromSkyPosition, transform.TransformDirection(Vector3.down), out hit, raycastDownDist + heightFromGroundRaycast, environmentLayer))
        {
            //print($"IK hit {hit.collider.gameObject.name}");

            feetIKPos = fromSkyPosition;
            feetIKPos.y = hit.point.y + pelvisOffset;
            //feetIKRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation;
            feetIKRot = Quaternion.FromToRotation(transform.TransformDirection(Vector3.up), hit.normal) * transform.rotation;
            return;
        }

        feetIKPos = Vector3.zero; //Fail
    }

    //Get the raycasting firing position
    void AdjustFeetTarget(ref Vector3 feetPos, HumanBodyBones foot)
    {
        feetPos = animator.GetBoneTransform(foot).position;
        //feetPos.y = transform.position.y + heightFromGroundRaycast;
        //feetPos.y = transform.localPosition.y + heightFromGroundRaycast;
        Vector3 pos = transform.InverseTransformPoint(feetPos);
        pos.y = 0.0f;
        pos.y += heightFromGroundRaycast;
        feetPos = transform.TransformPoint(pos);
    }

    /*
    print("IKControl OnAnimatorIK");
        if (animator)
        {
            if (ikActive)
            {
                if (move.isGrounded)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootRotWeight);
                    animator.SetIKPosition(AvatarIKGoal.RightFoot, Player.Instance.RightFootTarget.position);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot, Player.Instance.RightFootTarget.rotation);

                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootRotWeight);
                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, Player.Instance.LeftFootTarget.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, Player.Instance.LeftFootTarget.rotation);
                }
            }
        } 
    */

    private void OnAnimatorMove()
    {
        //leftFootTarget.position = leftFoot.position;
        //leftFootTarget.rotation = leftFoot.rotation;
        //rightFootTarget.position = rightFoot.position;
        //rightFootTarget.rotation = rightFoot.rotation;
        //
        //Ray ray = new Ray(leftFootTarget.position + Vector3.up * 0.5f, Vector3.down);
        //RaycastHit hit;
        //
        ////if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f, LayerMask.NameToLayer("Ground")))
        //if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f))
        //{
        //    Debug.Log($"IK Hit {hit.collider.gameObject.name}");
        //    leftFootTarget.position = hit.point + Vector3.up * 0.05f;
        //    //LeftFootIK.ManualApplyCalculations();
        //}
        //
        //ray = new Ray(rightFootTarget.position + Vector3.up * 0.5f, Vector3.down);
        //
        ////if (Physics.SphereCast(ray, 0.05f, out hit, 5.0f, LayerMask.NameToLayer("Ground")))
        //if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f))
        //{
        //    Debug.Log($"IK Hit {hit.collider.gameObject.name}");
        //    rightFootTarget.position = hit.point + Vector3.up * 0.05f;
        //    //RightFootIK.ManualApplyCalculations();
        //}

    }

    private void LateUpdate()
    {
        //leftFootTarget.position = leftFoot.position;
        //leftFootTarget.rotation = leftFoot.rotation;
        //rightFootTarget.position = rightFoot.position;
        //rightFootTarget.rotation = rightFoot.rotation;
        //
        //Ray ray = new Ray(leftFootTarget.position + Vector3.up * 0.5f, Vector3.down);
        //RaycastHit hit;
        //
        ////if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f, LayerMask.NameToLayer("Ground")))
        //if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f))
        //{
        //    Debug.Log($"IK Hit {hit.collider.gameObject.name}");
        //    leftFootTarget.position = hit.point + Vector3.up * 0.05f;
        //    //LeftFootIK.ManualApplyCalculations();
        //}
        //
        //ray = new Ray(rightFootTarget.position + Vector3.up * 0.5f, Vector3.down);
        //
        ////if (Physics.SphereCast(ray, 0.05f, out hit, 5.0f, LayerMask.NameToLayer("Ground")))
        //if (Physics.SphereCast(ray, 0.05f, out hit, 0.50f))
        //{
        //    Debug.Log($"IK Hit {hit.collider.gameObject.name}");
        //    rightFootTarget.position = hit.point + Vector3.up * 0.05f;
        //    //RightFootIK.ManualApplyCalculations();
        //}
    }

    void OldIK()
    {
        //print("On Animator IK");
        if (animator)
        {

            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {

                // Set the look target position, if one has been assigned
                if (lookObj != null)
                {
                    animator.SetLookAtWeight(1);
                    animator.SetLookAtPosition(lookObj.position);
                }

                // Set the right hand target position and rotation, if one has been assigned
                if (rightHandObj != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
                }

            }

            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                animator.SetLookAtWeight(0);
            }
        }
    }
}
